using AutoMapper;
using Guidy.Core.Specifications;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Talentree.Core;
using Talentree.Core.Entities.Identity;
using Talentree.Core.Enums;
using Talentree.Core.Exceptions;
using Talentree.Core.Specifications;
using Talentree.Core.Specifications.AccountSettingsSpecifications;
using Talentree.Core.Specifications.BusinessOwnerSpecifications;
using Talentree.Service.Contracts;
using Talentree.Service.DTOs.AccountSettings;
using Talentree.Service.DTOs.Notification;

namespace Talentree.Service.Services
{
    public class AccountSettingsService : IAccountSettingsService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly UserManager<AppUser> _userManager;
        private readonly IImageService _imageService;
        private readonly IEncryptionService _encryptionService;
        private readonly IAIService _aiService;
        private readonly ITokenService _tokenService;
        private readonly IEmailService _emailService;

        private readonly INotificationService _notificationService;  
        private readonly ILogger<AccountSettingsService> _logger;
        public AccountSettingsService(
            IUnitOfWork unitOfWork,
            UserManager<AppUser> userManager,
            IImageService imageService,
            IEncryptionService encryptionService,
            IAIService aiService,
            INotificationService notificationService,  
            ILogger<AccountSettingsService> logger,
            ITokenService tokenService,
            IEmailService emailService)    
        {
            _unitOfWork = unitOfWork;
            _userManager = userManager;
            _imageService = imageService;
            _encryptionService = encryptionService;
            _aiService = aiService;
            _notificationService = notificationService;  
            _logger = logger;
            _tokenService = tokenService;
            _emailService = emailService;
        }

        // ─────────────────────────────────────────────
        // FR-BO-32: Get Profile
        // ─────────────────────────────────────────────
        public async Task<ProfileDto> GetProfileAsync(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId)
                ?? throw new NotFoundException("User not found");

            var spec = new BusinessOwnerProfileByUserIdSpecification(userId);
            var profile = await _unitOfWork.Repository<BusinessOwnerProfile>()
                .GetByIdWithSpecificationsAsync(spec)
                ?? throw new NotFoundException("Business owner profile not found");

            return new ProfileDto
            {
                DisplayName = user.DisplayName,
                Email = user.Email!,
                PhoneNumber = profile.PhoneNumber ?? user.PhoneNumber,
                ProfilePhotoUrl = profile.ProfilePhotoUrl,
                BusinessName = profile.BusinessName,
                BusinessDescription = profile.BusinessDescription,
                BusinessAddress = profile.BusinessAddress,
                BusinessLogoUrl = profile.BusinessLogoUrl,
                BusinessStatus = profile.Status.ToString(),
                FacebookLink = profile.FacebookLink,
                InstagramLink = profile.InstagramLink,
                WebsiteLink = profile.WebsiteLink
            };
        }

        // ─────────────────────────────────────────────
        // FR-BO-32: Update Profile
        // ─────────────────────────────────────────────
        public async Task<ProfileDto> UpdateProfileAsync(
            string userId,
            UpdateProfileDto dto,
            IFormFile? profilePhoto,
            IFormFile? businessLogo)
        {
            var user = await _userManager.FindByIdAsync(userId)
                ?? throw new NotFoundException("User not found");

            var spec = new BusinessOwnerProfileByUserIdSpecification(userId);
            var profile = await _unitOfWork.Repository<BusinessOwnerProfile>()
                .GetByIdWithSpecificationsAsync(spec)
                ?? throw new NotFoundException("Business owner profile not found");

            // Check business name uniqueness (if changed)
            if (!profile.BusinessName.Equals(dto.BusinessName, StringComparison.OrdinalIgnoreCase))
            {
                var nameSpec = new BusinessNameExistsSpecification(dto.BusinessName);
                var existing = await _unitOfWork.Repository<BusinessOwnerProfile>()
                    .GetByIdWithSpecificationsAsync(nameSpec);
                if (existing != null)
                    throw new BadRequestException("Business name is already taken");
            }

            // Update user display name and phone
            user.DisplayName = $"{dto.FirstName} {dto.LastName}".Trim();
            user.PhoneNumber = dto.PhoneNumber;
            await _userManager.UpdateAsync(user);

            // Update profile fields
            profile.BusinessName = dto.BusinessName;
            profile.BusinessDescription = dto.BusinessDescription;
            profile.BusinessAddress = dto.BusinessAddress;
            profile.PhoneNumber = dto.PhoneNumber;
            profile.FacebookLink = dto.FacebookLink;
            profile.InstagramLink = dto.InstagramLink;
            profile.WebsiteLink = dto.WebsiteLink;

            // Upload profile photo (max 2MB)
            if (profilePhoto != null)
            {
                if (profilePhoto.Length > 2 * 1024 * 1024)
                    throw new BadRequestException("Profile photo must be less than 2MB");
                if (!_imageService.IsValidImage(profilePhoto))
                    throw new BadRequestException("Profile photo must be JPEG or PNG");

                if (!string.IsNullOrEmpty(profile.ProfilePhotoUrl))
                    await _imageService.DeleteImageAsync(profile.ProfilePhotoUrl);

                profile.ProfilePhotoUrl = await _imageService.UploadImageAsync(profilePhoto, "profiles");
            }

            // Upload business logo (max 2MB)
            if (businessLogo != null)
            {
                if (businessLogo.Length > 2 * 1024 * 1024)
                    throw new BadRequestException("Business logo must be less than 2MB");
                if (!_imageService.IsValidImage(businessLogo))
                    throw new BadRequestException("Business logo must be JPEG or PNG");

                if (!string.IsNullOrEmpty(profile.BusinessLogoUrl))
                    await _imageService.DeleteImageAsync(profile.BusinessLogoUrl);

                profile.BusinessLogoUrl = await _imageService.UploadImageAsync(businessLogo, "logos");
            }

            _unitOfWork.Repository<BusinessOwnerProfile>().Update(profile);
            await _unitOfWork.CompleteAsync();

            // Notify AI to recompute profile completeness
            _ = Task.Run(() => _aiService.ComputeProfileAsync(userId));

            // Email change — send OTP (handled separately via FR-BO-32 email verification flow)
            // We don't change email here directly — we send OTP first
            if (!string.IsNullOrEmpty(dto.NewEmail) &&
    !dto.NewEmail.Equals(user.Email, StringComparison.OrdinalIgnoreCase))
            {
                await RequestEmailChangeAsync(userId, dto.NewEmail);
            }

            return await GetProfileAsync(userId);
        }

        // ─────────────────────────────────────────────
        // FR-BO-33: Get Payment Info
        // ─────────────────────────────────────────────
        public async Task<PaymentInfoDto> GetPaymentInfoAsync(string userId, string currentPassword)
        {
            var user = await _userManager.FindByIdAsync(userId)
                ?? throw new NotFoundException("User not found");

            // Require password re-entry
            var passwordValid = await _userManager.CheckPasswordAsync(user, currentPassword);
            if (!passwordValid)
                throw new UnauthorizedException("Invalid password");

            var spec = new PaymentInfoByUserSpecification(userId);
            var payment = await _unitOfWork.Repository<BusinessOwnerPaymentInfo>()
                .GetByIdWithSpecificationsAsync(spec);

            if (payment == null) return new PaymentInfoDto(); // not set yet

            var decryptedAccount = _encryptionService.Decrypt(payment.AccountNumberEncrypted);

            return new PaymentInfoDto
            {
                BankName = payment.BankName,
                AccountHolderName = payment.AccountHolderName,
                MaskedAccountNumber = _encryptionService.MaskAccountNumber(decryptedAccount),
                RoutingSwiftCode = payment.RoutingSwiftCode,
                LastChangedAt = payment.LastChangedAt
            };
        }

        // ─────────────────────────────────────────────
        // FR-BO-33: Upsert Payment Info
        // ─────────────────────────────────────────────
        public async Task UpsertPaymentInfoAsync(
            string userId,
            UpsertPaymentInfoDto dto,
            string ipAddress)
        {
            var user = await _userManager.FindByIdAsync(userId)
                ?? throw new NotFoundException("User not found");

            // Require password re-entry
            var passwordValid = await _userManager.CheckPasswordAsync(user, dto.CurrentPassword);
            if (!passwordValid)
                throw new UnauthorizedException("Invalid password");

            var spec = new PaymentInfoByUserSpecification(userId);
            var existing = await _unitOfWork.Repository<BusinessOwnerPaymentInfo>()
                .GetByIdWithSpecificationsAsync(spec);

            var encrypted = _encryptionService.Encrypt(dto.AccountNumber);
            bool isNewPaymentInfo = existing == null;

            if (existing == null)
            {
                _unitOfWork.Repository<BusinessOwnerPaymentInfo>().Add(new BusinessOwnerPaymentInfo
                {
                    UserId = userId,
                    BankName = dto.BankName,
                    AccountHolderName = dto.AccountHolderName,
                    AccountNumberEncrypted = encrypted,
                    RoutingSwiftCode = dto.RoutingSwiftCode,
                    LastChangedAt = DateTime.UtcNow,
                    LastChangedByIp = ipAddress
                });
            }
            else
            {
                existing.BankName = dto.BankName;
                existing.AccountHolderName = dto.AccountHolderName;
                existing.AccountNumberEncrypted = encrypted;
                existing.RoutingSwiftCode = dto.RoutingSwiftCode;
                existing.LastChangedAt = DateTime.UtcNow;
                existing.LastChangedByIp = ipAddress;
                _unitOfWork.Repository<BusinessOwnerPaymentInfo>().Update(existing);
            }

            await _unitOfWork.CompleteAsync();

            // ✅ SEND NOTIFICATION
            if (isNewPaymentInfo)
            {
                await _notificationService.CreateNotificationAsync(new CreateNotificationDto
                {
                    UserId = userId,
                    Type = NotificationType.Account,
                    Title = "Payment Information Added ✅",
                    Message = $"Your bank account ({dto.BankName}) has been added successfully.",
                    ActionUrl = "/settings/payment",
                    ActionText = "View Payment Info",
                    Priority = NotificationPriority.Normal,
                    SendEmail = false,
                    RelatedEntityType = "PaymentInfo"
                });

                _logger.LogInformation("New payment info added for user {UserId}", userId);
            }
            else
            {
                await _notificationService.CreateNotificationAsync(new CreateNotificationDto
                {
                    UserId = userId,
                    Type = NotificationType.Account,
                    Title = "Payment Information Updated ✅",
                    Message = $"Your bank account ({dto.BankName}) has been updated successfully.",
                    ActionUrl = "/settings/payment",
                    ActionText = "View Payment Info",
                    Priority = NotificationPriority.Normal,
                    SendEmail = false,
                    RelatedEntityType = "PaymentInfo"
                });

                _logger.LogInformation("Payment info updated for user {UserId}", userId);
            }



        }

        // ─────────────────────────────────────────────
        // FR-BO-34: Change Password
        // ─────────────────────────────────────────────
        public async Task ChangePasswordAsync(string userId, ChangePasswordDto dto)
        {
            if (dto.NewPassword != dto.ConfirmNewPassword)
                throw new BadRequestException("New passwords do not match");

            var user = await _userManager.FindByIdAsync(userId)
                ?? throw new NotFoundException("User not found");

            var result = await _userManager.ChangePasswordAsync(
                user, dto.CurrentPassword, dto.NewPassword);

            if (!result.Succeeded)
                throw new BadRequestException(result.Errors.First().Description);
            // ✅ SEND NOTIFICATION
            await _notificationService.CreateNotificationAsync(new CreateNotificationDto
            {
                UserId = userId,
                Type = NotificationType.Account,
                Title = "Password Changed ✅",
                Message = "Your password has been changed successfully. If you didn't make this change, please contact support immediately.",
                ActionUrl = "/settings/security",
                ActionText = "View Security Settings",
                Priority = NotificationPriority.High,
                SendEmail = true,
                RelatedEntityType = "Account"
            });

            _logger.LogInformation("User {UserId} changed password successfully", userId);
        }
        // ─────────────────────────────────────────────
        // FR-BO-34: Get Login History
        // ─────────────────────────────────────────────
        public async Task<IReadOnlyList<LoginHistoryDto>> GetLoginHistoryAsync(
            string userId, int pageIndex, int pageSize)
        {
            var spec = new LoginHistoryByUserSpecification(userId, pageIndex, pageSize);
            var history = await _unitOfWork.Repository<LoginHistory>()
                .GetAllWithSpecificationsAsync(spec);

            return history.Select(h => new LoginHistoryDto
            {
                IpAddress = h.IpAddress,
                DeviceInfo = h.DeviceInfo,
                Location = h.Location,
                LoginAt = h.LoginAt,
                IsSuccessful = h.IsSuccessful
            }).ToList();
        }

        // ─────────────────────────────────────────────
        // FR-BO-34: Revoke All Other Sessions
        // ─────────────────────────────────────────────
        public async Task RevokeAllOtherSessionsAsync(string userId, string currentRefreshToken)
        {
            var allTokensSpec = new ActiveRefreshTokensForUserSpecification(userId);
            var tokens = await _unitOfWork.Repository<RefreshToken>()
                .GetAllWithSpecificationsAsync(allTokensSpec);

            // Hash the current token to compare against stored hashes
            var currentHash = _tokenService.HashToken(currentRefreshToken);
            int revokedCount = 0;

            foreach (var token in tokens)
            {
                // Keep the current session active — revoke everything else
                if (token.TokenHash != currentHash)
                {
                    token.RevokedAt = DateTime.UtcNow;
                    _unitOfWork.Repository<RefreshToken>().Update(token);
                }
            }

            if (revokedCount > 0)
            {
                await _unitOfWork.CompleteAsync();

                // ✅ SEND NOTIFICATION
                await _notificationService.CreateNotificationAsync(new CreateNotificationDto
                {
                    UserId = userId,
                    Type = NotificationType.Account,
                    Title = "Sessions Revoked 🔒",
                    Message = $"All other sessions ({revokedCount}) have been revoked. Only this device is active.",
                    ActionUrl = "/settings/security",
                    ActionText = "View Active Sessions",
                    Priority = NotificationPriority.High,
                    SendEmail = true,
                    RelatedEntityType = "Account"
                });

                _logger.LogInformation("User {UserId} revoked {Count} other sessions", userId, revokedCount);
            }
            else
            {
                _logger.LogInformation("User {UserId} has no other active sessions to revoke", userId);
            }

        }

        // ─────────────────────────────────────────────
        // FR-BO-35: Get Preferences
        // ─────────────────────────────────────────────
        public async Task<PreferencesDto> GetPreferencesAsync(string userId)
        {
            var spec = new UserPreferencesByUserSpecification(userId);
            var prefs = await _unitOfWork.Repository<UserPreferences>()
                .GetByIdWithSpecificationsAsync(spec);

            // Return defaults if not set yet
            prefs ??= new UserPreferences();

            return new PreferencesDto
            {
                Timezone = prefs.Timezone,
                DateFormat = prefs.DateFormat,
                CurrencyDisplay = prefs.CurrencyDisplay,
                DashboardLayout = prefs.DashboardLayout
            };
        }

        // ─────────────────────────────────────────────
        // FR-BO-35: Update Preferences
        // ─────────────────────────────────────────────
        public async Task<PreferencesDto> UpdatePreferencesAsync(
            string userId, UpdatePreferencesDto dto)
        {
            var spec = new UserPreferencesByUserSpecification(userId);
            var existing = await _unitOfWork.Repository<UserPreferences>()
                .GetByIdWithSpecificationsAsync(spec);

            if (existing == null)
            {
                _unitOfWork.Repository<UserPreferences>().Add(new UserPreferences
                {
                    UserId = userId,
                    Timezone = dto.Timezone,
                    DateFormat = dto.DateFormat,
                    CurrencyDisplay = dto.CurrencyDisplay,
                    DashboardLayout = dto.DashboardLayout
                });
            }
            else
            {
                existing.Timezone = dto.Timezone;
                existing.DateFormat = dto.DateFormat;
                existing.CurrencyDisplay = dto.CurrencyDisplay;
                existing.DashboardLayout = dto.DashboardLayout;
                _unitOfWork.Repository<UserPreferences>().Update(existing);
            }

            await _unitOfWork.CompleteAsync();
            return await GetPreferencesAsync(userId);
        }
        // ─────────────────────────────────────────────
        // FR-BO-32: Request Email Change → sends OTP
        // ─────────────────────────────────────────────
        public async Task RequestEmailChangeAsync(string userId, string newEmail)
        {
            var user = await _userManager.FindByIdAsync(userId)
                ?? throw new NotFoundException("User not found");

            // Check new email not already taken
            var existing = await _userManager.FindByEmailAsync(newEmail);
            if (existing != null)
                throw new BadRequestException("This email is already registered");

            // Generate OTP and save with purpose = ChangeEmail
            var otpCode = GenerateOtpCode();

            var otpEntity = new OtpCode
            {
                UserId = userId,
                Code = otpCode,
                Purpose = OtpPurpose.EmailVerification,
                ExpiresAt = DateTime.UtcNow.AddMinutes(5),
                CreatedAt = DateTime.UtcNow
            };

            _unitOfWork.Repository<OtpCode>().Add(otpEntity);
            await _unitOfWork.CompleteAsync();

            // Send OTP to the NEW email
            await _emailService.SendOtpAsync(newEmail, otpCode, OtpPurpose.EmailVerification);

            // ✅ SEND NOTIFICATION - In-app only (email notification is the OTP email itself)
            await _notificationService.CreateNotificationAsync(new CreateNotificationDto
            {
                UserId = userId,
                Type = NotificationType.Account,
                Title = "Email Change Requested 📧",
                Message = $"A verification code has been sent to {newEmail}. Please confirm within 5 minutes.",
                ActionUrl = "/settings/verify-email",
                ActionText = "Verify Email",
                Priority = NotificationPriority.High,
                SendEmail = false,  // Email already sent via OTP
                RelatedEntityType = "Account"
            });

            _logger.LogInformation("Email change requested for user {UserId} to {NewEmail}", userId, newEmail);

        }

        // ─────────────────────────────────────────────
        // FR-BO-32: Confirm Email Change with OTP
        // ─────────────────────────────────────────────
        public async Task ConfirmEmailChangeAsync(string userId, string otpCode, string newEmail)
        {
            var user = await _userManager.FindByIdAsync(userId)
                ?? throw new NotFoundException("User not found");

            // Validate OTP
            var spec = new OtpCodeSpecification(userId, otpCode, OtpPurpose.EmailVerification);
            var otpEntity = await _unitOfWork.Repository<OtpCode>()
                .GetByIdWithSpecificationsAsync(spec);

            if (otpEntity == null || !otpEntity.IsValid)
                throw new BadRequestException("Invalid or expired verification code");

            // Change the email
            var token = await _userManager.GenerateChangeEmailTokenAsync(user, newEmail);
            var result = await _userManager.ChangeEmailAsync(user, newEmail, token);

            if (!result.Succeeded)
                throw new BadRequestException(result.Errors.First().Description);

            // Update username too (since username = email in this project)
            await _userManager.SetUserNameAsync(user, newEmail);

            // Mark OTP as used
            otpEntity.IsUsed = true;
            otpEntity.UsedAt = DateTime.UtcNow;
            _unitOfWork.Repository<OtpCode>().Update(otpEntity);
            await _unitOfWork.CompleteAsync();

            // ✅ SEND NOTIFICATION
            await _notificationService.CreateNotificationAsync(new CreateNotificationDto
            {
                UserId = userId,
                Type = NotificationType.Account,
                Title = "Email Changed Successfully ✅",
                Message = $"Your email has been changed to {newEmail}. You can now log in with the new email.",
                ActionUrl = "/settings/profile",
                ActionText = "View Profile",
                Priority = NotificationPriority.High,
                SendEmail = true,
                RelatedEntityType = "Account"
            });
        }

        // Helper
        private static string GenerateOtpCode()
            => Random.Shared.Next(100000, 999999).ToString();
    }
}