using Microsoft.AspNetCore.Http;
using Talentree.Service.DTOs.AccountSettings;

namespace Talentree.Service.Contracts
{
    public interface IAccountSettingsService
    {
        // FR-BO-32: Profile
        Task<ProfileDto> GetProfileAsync(string userId);
        Task<ProfileDto> UpdateProfileAsync(string userId, UpdateProfileDto dto,
            IFormFile? profilePhoto, IFormFile? businessLogo);

        // FR-BO-33: Payment Info
        Task<PaymentInfoDto> GetPaymentInfoAsync(string userId, string currentPassword);
        Task UpsertPaymentInfoAsync(string userId, UpsertPaymentInfoDto dto, string ipAddress);

        // FR-BO-34: Security
        Task ChangePasswordAsync(string userId, ChangePasswordDto dto);
        Task<IReadOnlyList<LoginHistoryDto>> GetLoginHistoryAsync(string userId, int pageIndex, int pageSize);
        Task RevokeAllOtherSessionsAsync(string userId, string currentRefreshToken);

        // FR-BO-35: Preferences
        Task<PreferencesDto> GetPreferencesAsync(string userId);
        Task<PreferencesDto> UpdatePreferencesAsync(string userId, UpdatePreferencesDto dto);
    }
}