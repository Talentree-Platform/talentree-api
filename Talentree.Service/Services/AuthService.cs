using AutoMapper;
using Google.Apis.Auth;
using Guidy.Core.Specifications;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Talentree.Core;
using Talentree.Core.Entities.Identity;
using Talentree.Core.Enums;
using Talentree.Core.Exceptions;
using Talentree.Core.Specifications;
using Talentree.Core.Specifications.BusinessOwnerSpecifications;
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

        public AuthService(
            UserManager<AppUser> userManager,
            //RoleManager<IdentityRole> roleManager,
            ITokenService tokenService,
            IEmailService emailService,
            IMapper mapper,
            IUnitOfWork unitOfWork,
            IConfiguration configuration)
        {
            _userManager = userManager;
            //_roleManager = roleManager;
            _tokenService = tokenService;
            _emailService = emailService;
            _mapper = mapper;
            _unitOfWork = unitOfWork;
            _configuration = configuration;
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


            await SaveOtpCodeAsync(user.Id, otpCode, OtpPurpose.VerifyEmail);


            await _emailService.SendOtpAsync(user.Email!, otpCode , OtpPurpose.VerifyEmail );

            return "Registration successful. Please check your email for the verification code.";
        }



        public async Task<AuthResponseDto> LoginAsync(LoginDto loginDto)
        {
            // Find user by email
            var user = await _userManager.FindByEmailAsync(loginDto.Email);
            if (user == null)
                throw new UnauthorizedException("Invalid email or password");


            // Check if email is verified
            if (!user.EmailConfirmed)
                throw new ForbiddenException("Email not verified. Please verify your email first.");


            // Verify password
            var isPasswordValid = await _userManager.CheckPasswordAsync(user, loginDto.Password);
            if (!isPasswordValid)
                throw new UnauthorizedException("Invalid email or password");


            // Get user roles
            var roles = await _userManager.GetRolesAsync(user);

            // Generate JWT access token
            var accessToken = _tokenService.GenerateAccessToken(user, roles.ToList());

            // Generate refresh token
            var refreshToken = _tokenService.GenerateRefreshToken();

            // Save refresh token to database (hashed)
            await SaveRefreshTokenAsync(user.Id.ToString(), refreshToken);

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

            var spec = new OtpCodeSpecification(user.Id, verifyEmailDto.OtpCode, OtpPurpose.VerifyEmail);
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
            await SaveOtpCodeAsync(user.Id, otpCode, OtpPurpose.VerifyEmail);
            await _emailService.SendOtpAsync(user.Email!, otpCode, OtpPurpose.VerifyEmail);

            // 8️⃣ Notify admin (TODO)
            // await _emailService.NotifyAdminNewBusinessOwnerAsync(businessProfile);

            return "Business owner registration successful. Please check your email for verification code.";
        }


    }
}
