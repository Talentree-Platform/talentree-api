using AutoMapper;
using Google.Apis.Auth;
using Guidy.Core.Specifications;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Talentree.Core;
using Talentree.Core.Entities.Identity;
using Talentree.Core.Enums;
using Talentree.Core.Exceptions;
using Talentree.Core.Specifications;
using Talentree.Core.Specifications.BusinessOwnerSpecifications;
using Talentree.Service.Messaging;
using Talentree.Service.Messaging.Contracts;
using Talentree.Service.Contracts;
using Talentree.Service.DTOs.Auth;

namespace Talentree.Service.Services
{
    public class AuthService : IAuthService
    {
        private readonly UserManager<AppUser> _userManager;
        //private readonly RoleManager<IdentityRole> _roleManager;
        private readonly ITokenService _tokenService;
        private readonly IEmailService _emailService;
        private readonly IMapper _mapper;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IConfiguration _configuration;
        private readonly IAIService _aiService;
        private readonly INotificationHelperService _notificationHelper;
        private readonly ILogger<AuthService> _logger;
        private readonly IEventPublisher _eventPublisher;
        private readonly IAuditLogService _auditLogService;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public AuthService(
            UserManager<AppUser> userManager,
            //RoleManager<IdentityRole> roleManager,
            ITokenService tokenService,
            IEmailService emailService,
            IMapper mapper,
            IUnitOfWork unitOfWork,
            IConfiguration configuration,
            IAIService aiService,
            INotificationHelperService notificationHelper,
            ILogger<AuthService> logger,
            IEventPublisher eventPublisher,
            IAuditLogService auditLogService,
            IHttpContextAccessor httpContextAccessor)
        {
            _userManager = userManager;
            //_roleManager = roleManager;
            _tokenService = tokenService;
            _emailService = emailService;
            _mapper = mapper;
            _unitOfWork = unitOfWork;
            _configuration = configuration;
            _aiService = aiService;
            _notificationHelper = notificationHelper;
            _logger = logger;
            _eventPublisher = eventPublisher;
            _auditLogService = auditLogService;
            _httpContextAccessor = httpContextAccessor;
        }
        public async Task<string> RegisterAsync(RegisterDto registerDto)
        {
            // Check if email already exists
            var existingUser = await _userManager.FindByEmailAsync(registerDto.Email);
            if (existingUser != null)
                throw new BadRequestException("Email is already registered");

            
            // Map DTO to Entity
            var user = _mapper.Map<AppUser>(registerDto);

            var result = await _userManager.CreateAsync(user, registerDto.Password);

          
            if (!result.Succeeded)
            {
                var errorsDict = result.Errors
                    .GroupBy(e => e.Code)
                    .ToDictionary(g => g.Key, g => g.Select(e => e.Description).ToArray());

                throw new ValidationException(errorsDict);
            }


            await _userManager.AddToRoleAsync(user, "Customer");


            var otpCode = GenerateOtpCode();


            await SaveOtpCodeAsync(user.Id, otpCode, OtpPurpose.EmailVerification);


            await _emailService.SendOtpAsync(user.Email!, otpCode , OtpPurpose.EmailVerification );
            // add notification for new registration
            await _notificationHelper.NotifyUserRegistered(user.Id);

            return "Registration successful. Please check your email for the verification code.";
        }


        public async Task<string> ResendVerificationEmailAsync(string email)
        {
            // Find user by email
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
                throw new NotFoundException("User not found");

            // Check if already verified
            if (user.EmailConfirmed)
                throw new BadRequestException("Email is already verified");

            // ✅ Invalidate old OTP codes
            var oldOtpsSpec = new OtpCodeSpecification(user.Id, null, OtpPurpose.EmailVerification);
            var oldOtps = await _unitOfWork.Repository<OtpCode>()
                                            .GetAllWithSpecificationsAsync(oldOtpsSpec);

            foreach (var otp in oldOtps)
            {
                if (!otp.IsUsed && otp.ExpiresAt > DateTime.UtcNow)
                {
                    otp.IsUsed = true; // Mark as used so it can't be used
                    _unitOfWork.Repository<OtpCode>().Update(otp);
                }
            }

            await _unitOfWork.CompleteAsync();

            // ✅ Generate new OTP
            var otpCode = GenerateOtpCode();
            await SaveOtpCodeAsync(user.Id, otpCode, OtpPurpose.EmailVerification);

            // ✅ Send verification email
            await _emailService.SendOtpAsync(user.Email!, otpCode, OtpPurpose.EmailVerification);

            return "Verification code has been resent to your email";
        }


        public async Task<AuthResponseDto> LoginAsync(LoginDto loginDto)
        {
            // Find user by email
            var user = await _userManager.FindByEmailAsync(loginDto.Email);
            if (user == null)
            {
                await _auditLogService.LogActionAsync(
                    userId: null,
                    adminId: null,
                    action: "Login Failure",
                    reason: $"User email '{loginDto.Email}' not found."
                );
                await LogLoginHistoryAsync(userId: null, isSuccessful: false, status: "Failed", failureReason: $"User email '{loginDto.Email}' not found.");
                throw new UnauthorizedException("Invalid email or password");
            }

            // Check if email is verified
            if (!user.EmailConfirmed)
            {
                await _auditLogService.LogActionAsync(
                    userId: user.Id,
                    adminId: null,
                    action: "Login Failure",
                    reason: "Email not verified.",
                    entityType: "AppUser",
                    entityId: user.Id
                );
                await LogLoginHistoryAsync(userId: user.Id, isSuccessful: false, status: "Failed", failureReason: "Email not verified.");
                throw new ForbiddenException("Email not verified. Please verify your email first.");
            }

            if (!user.IsActive)
            {
                await _auditLogService.LogActionAsync(
                    userId: user.Id,
                    adminId: null,
                    action: "Login Failure",
                    reason: "Account is deactivated.",
                    entityType: "AppUser",
                    entityId: user.Id
                );
                await LogLoginHistoryAsync(userId: user.Id, isSuccessful: false, status: "Failed", failureReason: "Account deactivated.");
                throw new ForbiddenException("Account is inactive. Please contact the administrator.");
            }

            // Get user roles
            var roles = await _userManager.GetRolesAsync(user);
            var isAdmin = roles.Any(r => r == "SuperAdmin" || r == "Admin" || r == "SupportStaff" || r == "ContentManager");
            var isSuperAdmin = roles.Contains("SuperAdmin");

            // Retrieve database security settings
            var settings = await _unitOfWork.Repository<SecuritySettings>().GetByIdAsync(1);
            if (settings == null)
            {
                settings = new SecuritySettings(); // safe default settings
            }

            // 1. IP Whitelist (Exempting SuperAdmin; fail-open on any exception)
            if (isAdmin && !isSuperAdmin && !string.IsNullOrEmpty(settings.IpWhitelist))
            {
                try
                {
                    var clientIp = _httpContextAccessor.HttpContext?.Connection?.RemoteIpAddress?.ToString();
                    if (!string.IsNullOrEmpty(clientIp))
                    {
                        var whitelistedIps = settings.IpWhitelist.Split(',')
                            .Select(ip => ip.Trim())
                            .Where(ip => !string.IsNullOrEmpty(ip))
                            .ToList();

                        // Match or check fallback loop
                        if (!whitelistedIps.Contains(clientIp))
                        {
                            await _auditLogService.LogActionAsync(
                                userId: user.Id,
                                adminId: null,
                                action: "Login Failure",
                                reason: $"IP {clientIp} not in whitelisted range: {settings.IpWhitelist}",
                                entityType: "AppUser",
                                entityId: user.Id
                            );
                            await LogLoginHistoryAsync(userId: user.Id, isSuccessful: false, status: "Failed", failureReason: $"IP Whitelist Rejection: {clientIp}");
                            throw new ForbiddenException("Access denied: IP address is not whitelisted.");
                        }
                    }
                }
                catch (ForbiddenException)
                {
                    throw;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error checking IP Whitelist. Degraded to fail-open.");
                }
            }

            // 2. Allowed Login Hours (Exempting SuperAdmin; fail-open on any exception)
            if (isAdmin && !isSuperAdmin && settings.AllowedLoginStartTime.HasValue && settings.AllowedLoginEndTime.HasValue)
            {
                try
                {
                    var currentLocalTime = DateTime.Now.TimeOfDay;
                    var start = settings.AllowedLoginStartTime.Value;
                    var end = settings.AllowedLoginEndTime.Value;

                    bool isWithinAllowedWindow;
                    if (start <= end)
                    {
                        isWithinAllowedWindow = currentLocalTime >= start && currentLocalTime <= end;
                    }
                    else
                    {
                        // Overnight window
                        isWithinAllowedWindow = currentLocalTime >= start || currentLocalTime <= end;
                    }

                    if (!isWithinAllowedWindow)
                    {
                        await _auditLogService.LogActionAsync(
                            userId: user.Id,
                            adminId: null,
                            action: "Login Failure",
                            reason: $"Login attempted at {currentLocalTime} which is outside allowed range {start} - {end}",
                            entityType: "AppUser",
                            entityId: user.Id
                        );
                        await LogLoginHistoryAsync(userId: user.Id, isSuccessful: false, status: "Failed", failureReason: "Login hours restricted.");
                        throw new ForbiddenException("Access denied: login time is restricted.");
                    }
                }
                catch (ForbiddenException)
                {
                    throw;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error checking login hours. Degraded to fail-open.");
                }
            }

            // 3. Lockout Check (Exempting SuperAdmin)
            if (isAdmin && !isSuperAdmin)
            {
                if (await _userManager.IsLockedOutAsync(user))
                {
                    await _auditLogService.LogActionAsync(
                        userId: user.Id,
                        adminId: null,
                        action: "Login Failure",
                        reason: "Account is temporarily locked out due to multiple failed attempts.",
                        entityType: "AppUser",
                        entityId: user.Id
                    );
                    await LogLoginHistoryAsync(userId: user.Id, isSuccessful: false, status: "LockedOut", failureReason: "Account locked out.");
                    throw new ForbiddenException($"Account is temporarily locked out. Please try again later.");
                }
            }

            // Verify password
            var isPasswordValid = await _userManager.CheckPasswordAsync(user, loginDto.Password);
            if (!isPasswordValid)
            {
                if (isAdmin && !isSuperAdmin)
                {
                    // Increment failed count
                    await _userManager.AccessFailedAsync(user);

                    // Check if threshold is reached
                    var maxAttempts = settings.MaxFailedAccessAttempts <= 0 ? 5 : settings.MaxFailedAccessAttempts;
                    var lockoutMinutes = settings.LockoutDurationInMinutes <= 0 ? 15 : settings.LockoutDurationInMinutes;
                    
                    var accessFailedCount = await _userManager.GetAccessFailedCountAsync(user);
                    if (accessFailedCount >= maxAttempts)
                    {
                        await _userManager.SetLockoutEndDateAsync(user, DateTimeOffset.UtcNow.AddMinutes(lockoutMinutes));
                        await _auditLogService.LogActionAsync(
                            userId: user.Id,
                            adminId: null,
                            action: "Login Failure",
                            reason: $"Max failed attempts ({maxAttempts}) exceeded. Locked for {lockoutMinutes} minutes.",
                            entityType: "AppUser",
                            entityId: user.Id
                        );
                        await LogLoginHistoryAsync(userId: user.Id, isSuccessful: false, status: "LockedOut", failureReason: "Lockout threshold reached.");
                    }
                    else
                    {
                        await _auditLogService.LogActionAsync(
                            userId: user.Id,
                            adminId: null,
                            action: "Login Failure",
                            reason: "Incorrect password.",
                            entityType: "AppUser",
                            entityId: user.Id
                        );
                        await LogLoginHistoryAsync(userId: user.Id, isSuccessful: false, status: "Failed", failureReason: "Incorrect password.");
                    }
                }
                else
                {
                    await _auditLogService.LogActionAsync(
                        userId: user.Id,
                        adminId: null,
                        action: "Login Failure",
                        reason: "Incorrect password.",
                        entityType: "AppUser",
                        entityId: user.Id
                    );
                    await LogLoginHistoryAsync(userId: user.Id, isSuccessful: false, status: "Failed", failureReason: "Incorrect password.");
                }

                throw new UnauthorizedException("Invalid email or password");
            }

            // Reset failed count on successful password check
            if (isAdmin && !isSuperAdmin)
            {
                await _userManager.ResetAccessFailedCountAsync(user);
            }

            // Forced password change check
            if (user.MustChangePassword)
            {
                await _auditLogService.LogActionAsync(
                    userId: user.Id,
                    adminId: null,
                    action: "Login Failure",
                    reason: "Password change is forced on first login.",
                    entityType: "AppUser",
                    entityId: user.Id
                );
                await LogLoginHistoryAsync(userId: user.Id, isSuccessful: false, status: "Failed", failureReason: "Password change forced.");
                return new AuthResponseDto
                {
                    RequiresPasswordChange = true,
                    UserId = user.Id
                };
            }

            // 4. 2FA Check (Triggered if user has manually enabled 2FA OR if globally required for admins)
            if (user.IsTwoFactorEnabled || (isAdmin && settings.RequireTwoFactorForAdmins))
            {
                // Generate and save OTP code
                var code = GenerateOtpCode();
                await SaveOtpCodeAsync(user.Id, code, OtpPurpose.TwoFactorAuth);

                // OTP Logging Hardening: Only logs OTP code to console in Development environment
                var envName = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
                if (string.Equals(envName, "Development", StringComparison.OrdinalIgnoreCase))
                {
                    _logger.LogWarning($"[MFA EMERGENCY BYPASS] User {user.Email} requested 2FA. Generated OTP code is: {code}");
                }

                try
                {
                    // Send code via email
                    await _emailService.SendOtpAsync(user.Email!, code, OtpPurpose.TwoFactorAuth);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to send 2FA OTP email.");
                }

                // Log pending 2FA authentication
                await LogLoginHistoryAsync(userId: user.Id, isSuccessful: false, status: "2FA Pending", failureReason: "MFA OTP code generated and sent.");

                return new AuthResponseDto
                {
                    RequiresTwoFactor = true,
                    TwoFactorProvider = "Email",
                    UserId = user.Id
                };
            }

            // Proceed standard login
            var accessToken = _tokenService.GenerateAccessToken(user, roles.ToList());
            var refreshToken = _tokenService.GenerateRefreshToken();

            // Save refresh token to database (hashed)
            await SaveRefreshTokenAsync(user.Id.ToString(), refreshToken);

            // Log successful login
            await _auditLogService.LogActionAsync(
                userId: user.Id,
                adminId: user.Id,
                action: "Login Success",
                reason: "User authenticated successfully.",
                entityType: "AppUser",
                entityId: user.Id
            );
            await LogLoginHistoryAsync(userId: user.Id, isSuccessful: true, status: "Success", failureReason: null);

            // Map user to UserInfoDto
            var userInfo = _mapper.Map<UserInfoDto>(user);
            userInfo.Roles = roles.ToList();

            // Return auth response
            return new AuthResponseDto
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken,
                ExpiresAt = DateTime.UtcNow.AddMinutes(_tokenService.GetAccessTokenExpiryMinutes()),
                User = userInfo
            };
        }

        public async Task<AuthResponseDto> VerifyTwoFactorAsync(VerifyTwoFactorDto dto)
        {
            var user = await _userManager.FindByIdAsync(dto.UserId);
            if (user == null)
                throw new UnauthorizedException("Invalid request details");

            if (!user.IsActive)
                throw new ForbiddenException("Account is inactive.");

            var roles = await _userManager.GetRolesAsync(user);
            var isSuperAdmin = roles.Contains("SuperAdmin");

            // Check Emergency Bypass Code for SuperAdmin
            bool isBypassUsed = false;
            if (isSuperAdmin)
            {
                var bypassCode = _configuration["SeedData:SuperAdminEmergency2FaBypassCode"] ?? "TalentreeEmergencyBypass2026!";
                if (dto.OtpCode == bypassCode)
                {
                    isBypassUsed = true;
                }
            }

            if (!isBypassUsed)
            {
                // Retrieve the active OTP for TwoFactorAuth
                var spec = new OtpCodeSpecification(user.Id, dto.OtpCode, OtpPurpose.TwoFactorAuth);
                var otpEntity = await _unitOfWork.Repository<OtpCode>().GetByIdWithSpecificationsAsync(spec);

                if (otpEntity == null || !otpEntity.IsValid)
                {
                    if (!isSuperAdmin)
                    {
                        await _userManager.AccessFailedAsync(user);
                    }
                    // Log failed 2FA verification
                    await _auditLogService.LogActionAsync(
                        userId: user.Id,
                        adminId: null,
                        action: "2FA Verification Failure",
                        reason: "Invalid or expired OTP code.",
                        entityType: "AppUser",
                        entityId: user.Id
                    );
                    await LogLoginHistoryAsync(userId: user.Id, isSuccessful: false, status: "Failed", failureReason: "Invalid or expired 2FA OTP code.");
                    throw new UnauthorizedException("Invalid or expired verification code.");
                }

                // Mark OTP as used
                otpEntity.IsUsed = true;
                otpEntity.UsedAt = DateTime.UtcNow;
                _unitOfWork.Repository<OtpCode>().Update(otpEntity);
                await _unitOfWork.CompleteAsync();
            }

            // Generate tokens
            var accessToken = _tokenService.GenerateAccessToken(user, roles.ToList());
            var refreshToken = _tokenService.GenerateRefreshToken();
            await SaveRefreshTokenAsync(user.Id.ToString(), refreshToken);

            // Reset failed access attempts
            if (!isSuperAdmin)
            {
                await _userManager.ResetAccessFailedCountAsync(user);
            }

            // Log successful login
            if (isBypassUsed)
            {
                await _auditLogService.LogActionAsync(
                    userId: user.Id,
                    adminId: user.Id,
                    action: "Emergency 2FA Bypass Used",
                    reason: "SuperAdmin bypassed 2FA using emergency recovery code.",
                    entityType: "AppUser",
                    entityId: user.Id
                );
                await LogLoginHistoryAsync(userId: user.Id, isSuccessful: true, status: "Success (Emergency Bypass)", failureReason: null);
            }
            else
            {
                await _auditLogService.LogActionAsync(
                    userId: user.Id,
                    adminId: user.Id,
                    action: "2FA Verification Success",
                    reason: "Two-factor authentication succeeded.",
                    entityType: "AppUser",
                    entityId: user.Id
                );
                await LogLoginHistoryAsync(userId: user.Id, isSuccessful: true, status: "Success (2FA)", failureReason: null);
            }

            var userInfo = _mapper.Map<UserInfoDto>(user);
            userInfo.Roles = roles.ToList();

            return new AuthResponseDto
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken,
                ExpiresAt = DateTime.UtcNow.AddMinutes(_tokenService.GetAccessTokenExpiryMinutes()),
                User = userInfo
            };
        }

        public async Task<AuthResponseDto> ChangeForcedPasswordAsync(ChangeForcedPasswordDto dto)
        {
            var user = await _userManager.FindByIdAsync(dto.UserId);
            if (user == null)
                throw new UnauthorizedException("User not found");

            if (!user.MustChangePassword)
                throw new BadRequestException("Password change is not forced for this account.");

            // Verify temporary password
            var isTempValid = await _userManager.CheckPasswordAsync(user, dto.TemporaryPassword);
            if (!isTempValid)
            {
                await _auditLogService.LogActionAsync(
                    userId: user.Id,
                    adminId: null,
                    action: "Login Failure",
                    reason: "Invalid temporary password during forced password change.",
                    entityType: "AppUser",
                    entityId: user.Id
                );
                await LogLoginHistoryAsync(userId: user.Id, isSuccessful: false, status: "Failed", failureReason: "Invalid temporary password during change.");
                throw new UnauthorizedException("Invalid temporary password");
            }

            // Change password
            var result = await _userManager.ChangePasswordAsync(user, dto.TemporaryPassword, dto.NewPassword);
            if (!result.Succeeded)
            {
                var errorsDict = result.Errors
                    .GroupBy(e => e.Code)
                    .ToDictionary(g => g.Key, g => g.Select(e => e.Description).ToArray());
                throw new ValidationException(errorsDict);
            }

            user.MustChangePassword = false;
            await _userManager.UpdateAsync(user);

            // Log successful password change
            await _auditLogService.LogActionAsync(
                userId: user.Id,
                adminId: user.Id,
                action: "Password Changed",
                reason: "Forced password changed successfully on first login.",
                entityType: "AppUser",
                entityId: user.Id,
                beforeValues: "{\"mustChangePassword\": true}",
                afterValues: "{\"mustChangePassword\": false}"
            );

            // Successful login record
            await LogLoginHistoryAsync(userId: user.Id, isSuccessful: true, status: "Success (Forced Password Change)", failureReason: null);

            var roles = await _userManager.GetRolesAsync(user);
            var accessToken = _tokenService.GenerateAccessToken(user, roles.ToList());
            var refreshToken = _tokenService.GenerateRefreshToken();
            await SaveRefreshTokenAsync(user.Id.ToString(), refreshToken);

            var userInfo = _mapper.Map<UserInfoDto>(user);
            userInfo.Roles = roles.ToList();

            return new AuthResponseDto
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken,
                ExpiresAt = DateTime.UtcNow.AddMinutes(_tokenService.GetAccessTokenExpiryMinutes()),
                User = userInfo
            };
        }

        private async Task LogLoginHistoryAsync(string? userId, bool isSuccessful, string status, string? failureReason)
        {
            try
            {
                var context = _httpContextAccessor.HttpContext;
                var ip = context?.Connection?.RemoteIpAddress?.ToString() ?? "Unknown";
                var userAgent = context?.Request?.Headers["User-Agent"].ToString() ?? "Unknown";
                
                // Parse Device
                var device = "Unknown Device";
                if (!string.IsNullOrEmpty(userAgent))
                {
                    if (userAgent.Contains("Android")) device = "Android Device";
                    else if (userAgent.Contains("iPhone")) device = "iOS Device";
                    else if (userAgent.Contains("iPad")) device = "iPad Device";
                    else if (userAgent.Contains("Windows")) device = "Windows PC";
                    else if (userAgent.Contains("Macintosh")) device = "Mac PC";
                    else if (userAgent.Contains("Linux")) device = "Linux PC";
                }

                var history = new LoginHistory
                {
                    UserId = userId,
                    IpAddress = ip,
                    DeviceInfo = userAgent.Length > 500 ? userAgent.Substring(0, 500) : userAgent,
                    Location = "Unknown",
                    LoginAt = DateTime.UtcNow,
                    IsSuccessful = isSuccessful,
                    Status = status,
                    FailureReason = failureReason,
                    UserAgent = userAgent,
                    Device = device
                };

                _unitOfWork.Repository<LoginHistory>().Add(history);
                await _unitOfWork.CompleteAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error writing login history log.");
            }
        }

        // ═══════════════════════════════════════════════════════════
        // REFRESH TOKEN
        // ═══════════════════════════════════════════════════════════

        public async Task<AuthResponseDto> RefreshTokenAsync(RefreshTokenDto refreshTokenDto)
        {
            // Hash the incoming refresh token
            var tokenHash = _tokenService.HashToken(refreshTokenDto.RefreshToken);

            // Find refresh token in database using Specification Pattern
            var spec = new RefreshTokenWithUserSpecification(tokenHash);
            var storedToken =
          await _unitOfWork.Repository<RefreshToken>()
                           .GetByIdWithSpecificationsAsync(spec);


            if (storedToken == null)
                throw new BadRequestException("Invalid refresh token");


            // Validate token
            if (!storedToken.IsActive)
                throw new BadRequestException("Refresh token is expired or revoked");


            // Get user
            var user = storedToken.User;

            // Get user roles
            var roles = await _userManager.GetRolesAsync(user);

            // Generate new JWT access token
            var newAccessToken = _tokenService.GenerateAccessToken(user, roles.ToList());
    

            // Generate new refresh token
            var newRefreshToken = _tokenService.GenerateRefreshToken();

            // Revoke old refresh token
            storedToken.RevokedAt = DateTime.UtcNow;
            _unitOfWork.Repository<RefreshToken>().Update(storedToken);


            // Save new refresh token
            await SaveRefreshTokenAsync(user.Id.ToString(), newRefreshToken);

            // Save changes
            await _unitOfWork.CompleteAsync();
            // Predict churn risk on every login (using centralized background queue)
            var userId = user.Id;
            await _eventPublisher.PublishAsync("ai.churn", new ChurnPredictionMessage { UserId = userId });

            // Map user to UserInfoDto
            var userInfo = _mapper.Map<UserInfoDto>(user);
            userInfo.Roles = roles.ToList();

            // Return new tokens
            return new AuthResponseDto
            {
                AccessToken = newAccessToken,
                RefreshToken = newRefreshToken,
                ExpiresAt = DateTime.UtcNow.AddMinutes(_tokenService.GetAccessTokenExpiryMinutes()),
                User = userInfo
            };
        }

        // ═══════════════════════════════════════════════════════════
        // LOGOUT
        // ═══════════════════════════════════════════════════════════

        public async Task LogoutAsync(string refreshToken)
        {
            // Hash the refresh token
            var tokenHash = _tokenService.HashToken(refreshToken);

            // Find token in database
            var spec = new RefreshTokenWithUserSpecification(tokenHash);
            var storedToken = await _unitOfWork.Repository<RefreshToken>()
                           .GetByIdWithSpecificationsAsync(spec);

            if (storedToken == null)
                throw new Exception("Invalid refresh token");

            // Revoke token
            storedToken.RevokedAt = DateTime.UtcNow;
            _unitOfWork.Repository<RefreshToken>().Update(storedToken);

            // Save changes
            await _unitOfWork.CompleteAsync();

        }

        // ═══════════════════════════════════════════════════════════
        // VERIFY EMAIL
        // ═══════════════════════════════════════════════════════════
        public async Task<AuthResponseDto> VerifyEmailAsync(VerifyEmailDto verifyEmailDto)
        {

            var user = await _userManager.FindByEmailAsync(verifyEmailDto.Email);
            if (user == null)
                throw new BadRequestException("Invalid verification request");


            if (user.EmailConfirmed)
                throw new BadRequestException("Email is already verified");

            var spec = new OtpCodeSpecification(user.Id, verifyEmailDto.OtpCode, OtpPurpose.EmailVerification);
            var otpEntity = await _unitOfWork.Repository<OtpCode>()
                                              .GetByIdWithSpecificationsAsync(spec);

            if (otpEntity == null || !otpEntity.IsValid)
                throw new BadRequestException("Invalid or expired verification code");


            user.EmailConfirmed = true;
            await _userManager.UpdateAsync(user);

            otpEntity.IsUsed = true;
            otpEntity.UsedAt = DateTime.UtcNow;
            _unitOfWork.Repository<OtpCode>().Update(otpEntity);
            await _unitOfWork.CompleteAsync();


            var roles = await _userManager.GetRolesAsync(user);
            var accessToken = _tokenService.GenerateAccessToken(user, roles.ToList());
            var refreshToken = _tokenService.GenerateRefreshToken();
            await SaveRefreshTokenAsync(user.Id.ToString(), refreshToken);


            // ✅ ADD NOTIFICATION
            await _notificationHelper.NotifyEmailVerified(user.Id);

            _logger.LogInformation("User {UserId} verified email", user.Id);

            var userInfo = _mapper.Map<UserInfoDto>(user);
            userInfo.Roles = roles.ToList();

            return new AuthResponseDto
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken,
                ExpiresAt = DateTime.UtcNow.AddMinutes(_tokenService.GetAccessTokenExpiryMinutes()),
                User = userInfo
            };
        }


        // ═══════════════════════════════════════════════════════════
        // FORGOT PASSWORD
        // ═══════════════════════════════════════════════════════════

        public async Task ForgotPasswordAsync(ForgotPasswordDto forgotPasswordDto)
        {
            // Find user by email
            var user = await _userManager.FindByEmailAsync(forgotPasswordDto.Email);

            // Don't reveal if user exists or not (security best practice)
            if (user == null)
                return; // Silent fail

            var otpCode = GenerateOtpCode();

            await SaveOtpCodeAsync(user.Id, otpCode, OtpPurpose.ResetPassword);

            await _emailService.SendOtpAsync(user.Email!, otpCode , OtpPurpose.ResetPassword);
        }

        // ═══════════════════════════════════════════════════════════
        // RESET PASSWORD
        // ═══════════════════════════════════════════════════════════

        public async Task ResetPasswordAsync(ResetPasswordDto resetPasswordDto)
        {
            // Find user by email
            var user = await _userManager.FindByEmailAsync(resetPasswordDto.Email);
            if (user == null)
                throw new NotFoundException("User not found");

            // ✅ Find OTP code in database
            var spec = new OtpCodeSpecification(user.Id, resetPasswordDto.OtpCode, OtpPurpose.ResetPassword);
            var otpEntity = await _unitOfWork.Repository<OtpCode>()
                                              .GetByIdWithSpecificationsAsync(spec);

            // ✅ Validate OTP
            if (otpEntity == null || !otpEntity.IsValid)
                throw new BadRequestException("Invalid or expired verification code");
            // Hash the new password
            var passwordHasher = new PasswordHasher<AppUser>();
            user.PasswordHash = passwordHasher.HashPassword(user, resetPasswordDto.NewPassword);

            // Update security stamp (invalidates existing tokens)
            await _userManager.UpdateSecurityStampAsync(user);

            // Save changes
            var updateResult = await _userManager.UpdateAsync(user);

            if (!updateResult.Succeeded)
            {
                var errorsDict = updateResult.Errors
                    .GroupBy(e => e.Code)
                    .ToDictionary(g => g.Key, g => g.Select(e => e.Description).ToArray());

                throw new ValidationException(errorsDict);
            }

            //  Mark OTP as used
            otpEntity.IsUsed = true;
            otpEntity.UsedAt = DateTime.UtcNow;
            _unitOfWork.Repository<OtpCode>().Update(otpEntity);

            // Revoke all refresh tokens (security measure)
            var activeTokensSpec = new ActiveRefreshTokensForUserSpecification(user.Id);
            var activeTokens = await _unitOfWork.Repository<RefreshToken>().GetAllWithSpecificationsAsync(activeTokensSpec);

            foreach (var token in activeTokens)
            {
                token.RevokedAt = DateTime.UtcNow;
                _unitOfWork.Repository<RefreshToken>().Update(token);
            }

            await _unitOfWork.CompleteAsync();
            // ✅ ADD NOTIFICATION
            await _notificationHelper.NotifyPasswordResetSuccess(user.Id);

            _logger.LogInformation("User {UserId} reset password", user.Id);
        }

        // ═══════════════════════════════════════════════════════════
        // GOOGLE LOGIN
        // ═══════════════════════════════════════════════════════════


        public async Task<AuthResponseDto> GoogleLoginAsync(ExternalLoginDto externalLoginDto)
        {

            if (_configuration == null)
                throw new Exception("IConfiguration is not injected correctly in the constructor.");

            var clientId = _configuration["Google:ClientId"];

            if (string.IsNullOrEmpty(clientId))
                throw new Exception("ClientId was found as null or empty in appsettings.json.");

            // Verify Google ID token
            var payload = await GoogleJsonWebSignature.ValidateAsync(externalLoginDto.IdToken, new GoogleJsonWebSignature.ValidationSettings
            {
                Audience = new[] { clientId }
            });

            // Extract user info from Google payload
            var email = payload.Email;
            var firstName = payload.GivenName;
            var lastName = payload.FamilyName;
            var pictureUrl = payload.Picture;

            // Check if user exists
            var user = await _userManager.FindByEmailAsync(email);

            if (user == null)
            {
                // Create new user
                user = new AppUser
                {
                    UserName = email,
                    Email = email,
                    DisplayName = $"{firstName} {lastName}",
                    EmailConfirmed = true 

                };

                var result = await _userManager.CreateAsync(user);

                if (!result.Succeeded)
                {
                    var errorsDict = result.Errors
                        .GroupBy(e => e.Code)
                        .ToDictionary(g => g.Key, g => g.Select(e => e.Description).ToArray());

                    throw new ValidationException(errorsDict);
                }


                // Assign Customer role
                await _userManager.AddToRoleAsync(user, "Customer");
                // ✅ SEND NOTIFICATION FOR NEW USER
                await _notificationHelper.NotifyUserRegistered(user.Id);

            }

            // Get user roles
            var roles = await _userManager.GetRolesAsync(user);

            // Generate JWT tokens
            var accessToken = _tokenService.GenerateAccessToken(user, roles.ToList());
            var refreshToken = _tokenService.GenerateRefreshToken();

            // Save refresh token
            await SaveRefreshTokenAsync(user.Id.ToString(), refreshToken);

            // Map user to DTO
            var userInfo = _mapper.Map<UserInfoDto>(user);
            userInfo.Roles = roles.ToList();

            return new AuthResponseDto
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken,
                ExpiresAt = DateTime.UtcNow.AddMinutes(_tokenService.GetAccessTokenExpiryMinutes()),
                User = userInfo
            };
        }

        // ═══════════════════════════════════════════════════════════
        // FACEBOOK LOGIN
        // ═══════════════════════════════════════════════════════════

        public async Task<AuthResponseDto> FacebookLoginAsync(ExternalLoginDto externalLoginDto)
        {
            // Verify Facebook access token with Graph API
            using var httpClient = new HttpClient();
            var response = await httpClient.GetStringAsync(
                $"https://graph.facebook.com/me?fields=id,email,first_name,last_name,picture&access_token={externalLoginDto.IdToken}");

            var facebookUser = System.Text.Json.JsonSerializer.Deserialize<FacebookUserInfo>(response);

            if (facebookUser == null || string.IsNullOrEmpty(facebookUser.Email))
                throw new BadRequestException("Failed to get user info from Facebook");


            // Check if user exists
            var user = await _userManager.FindByEmailAsync(facebookUser.Email);

            if (user == null)
            {
                // Create new user
                user = new AppUser
                {
                    UserName = facebookUser.Email,
                    Email = facebookUser.Email,
                    DisplayName = $"{facebookUser.FirstName} {facebookUser.LastName}",
                    EmailConfirmed = true 
                };

                var result = await _userManager.CreateAsync(user);

                if (!result.Succeeded)
                {
                    var errorsDict = result.Errors
                        .GroupBy(e => e.Code)
                        .ToDictionary(g => g.Key, g => g.Select(e => e.Description).ToArray());

                    throw new ValidationException(errorsDict);
                }


                // Assign Customer role
                await _userManager.AddToRoleAsync(user, "Customer");
                // ✅ SEND NOTIFICATION FOR NEW USER
                await _notificationHelper.NotifyUserRegistered(user.Id);

            }

            // Get user roles
            var roles = await _userManager.GetRolesAsync(user);

            // Generate JWT tokens
            var accessToken = _tokenService.GenerateAccessToken(user, roles.ToList());
            var refreshToken = _tokenService.GenerateRefreshToken();

            // Save refresh token
            await SaveRefreshTokenAsync(user.Id.ToString(), refreshToken);

            // Map user to DTO
            var userInfo = _mapper.Map<UserInfoDto>(user);
            userInfo.Roles = roles.ToList();

            return new AuthResponseDto
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken,
                ExpiresAt = DateTime.UtcNow.AddMinutes(_tokenService.GetAccessTokenExpiryMinutes()),
                User = userInfo
            };
        }

        // Helper class for Facebook response
        private class FacebookUserInfo
        {
            public string? Id { get; set; }
            public string? Email { get; set; }
            public string? FirstName { get; set; }
            public string? LastName { get; set; }
            public FacebookPicture? Picture { get; set; }
        }

        private class FacebookPicture
        {
            public FacebookPictureData? Data { get; set; }
        }

        private class FacebookPictureData
        {
            public string? Url { get; set; }
        }



        // Helpers methods

        private string GenerateOtpCode()
        {
            return Random.Shared.Next(100000, 999999).ToString();
        }


        private async Task SaveOtpCodeAsync(string userId, string code, OtpPurpose purpose)
        {
            var otpEntity = new OtpCode
            {
                UserId = userId,
                Code = code,
                Purpose = purpose,
                ExpiresAt = DateTime.UtcNow.AddMinutes(5), // OTP expires after 5 minutes
                CreatedAt = DateTime.UtcNow
            };

            _unitOfWork.Repository<OtpCode>().Add(otpEntity);
            await _unitOfWork.CompleteAsync();
        }

        private async Task SaveRefreshTokenAsync(string userId, string refreshToken)
        {
            var tokenHash = _tokenService.HashToken(refreshToken);

            var refreshTokenEntity = new RefreshToken
            {
                UserId = userId,
                TokenHash = tokenHash,
                ExpiresAt = DateTime.UtcNow.AddDays(_tokenService.GetRefreshTokenExpiryDays()),
                CreatedAt = DateTime.UtcNow
            };

            _unitOfWork.Repository<RefreshToken>().Add(refreshTokenEntity);
            await _unitOfWork.CompleteAsync();

        }

        // Talentree.Service/Services/AuthService.cs

        // Talentree.Service/Services/AuthService.cs

        public async Task<string> RegisterBusinessOwnerAsync(BusinessOwnerRegisterDto registerDto)
        {
            // 1️⃣ Check if email exists
            var existingUser = await _userManager.FindByEmailAsync(registerDto.Email);
            if (existingUser != null)
                throw new BadRequestException("Email is already registered");

            // 2️⃣ Check if business name is unique
            var spec = new BusinessNameExistsSpecification(registerDto.BusinessName);
            var existingBusiness = await _unitOfWork.Repository<BusinessOwnerProfile>()
                .GetByIdWithSpecificationsAsync(spec);

            if (existingBusiness != null)
                throw new BadRequestException("Business name already exists. Please choose a different name.");

            // 3️⃣ Map DTO to AppUser
            var user = _mapper.Map<AppUser>(registerDto);

            // 4️⃣ Create user account
            var result = await _userManager.CreateAsync(user, registerDto.Password);

            if (!result.Succeeded)
            {
                var errorsDict = result.Errors
                    .GroupBy(e => e.Code)
                    .ToDictionary(g => g.Key, g => g.Select(e => e.Description).ToArray());

                throw new ValidationException(errorsDict);
            }

            // 5️⃣ Assign BusinessOwner role
            await _userManager.AddToRoleAsync(user, "BusinessOwner");

            // 6️⃣ Map DTO to BusinessOwnerProfile
            var businessProfile = _mapper.Map<BusinessOwnerProfile>(registerDto);
            businessProfile.UserId = user.Id;

            _unitOfWork.Repository<BusinessOwnerProfile>().Add(businessProfile);
            await _unitOfWork.CompleteAsync();

            // 7️⃣ Send verification email
            var otpCode = GenerateOtpCode();
            await SaveOtpCodeAsync(user.Id, otpCode, OtpPurpose.EmailVerification);
            await _emailService.SendOtpAsync(user.Email!, otpCode, OtpPurpose.EmailVerification);

            // ✅ SEND NOTIFICATION
            await _notificationHelper.NotifyUserRegistered(user.Id);

            return "Business owner registration successful. Please check your email for verification code.";
        }


    }
}
