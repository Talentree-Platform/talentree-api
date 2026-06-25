using Talentree.Service.DTOs.PlatformSettings;

namespace Talentree.Service.Contracts
{
    /// <summary>
    /// FR-AD-32: Admin management of platform commission and fee configuration.
    /// Uses a single-row upsert pattern (Id = 1).
    /// </summary>
    public interface ICommissionSettingService
    {
        /// <summary>Returns the current commission settings. Creates defaults if none exist.</summary>
        Task<CommissionSettingDto> GetCommissionSettingsAsync();

        /// <summary>Overwrites the current commission settings. Changes apply to new transactions only.</summary>
        Task<CommissionSettingDto> UpdateCommissionSettingsAsync(UpdateCommissionSettingDto dto, string adminId);
    }
}
