using Talentree.Service.DTOs.PlatformSettings;

namespace Talentree.Service.Contracts
{
    /// <summary>
    /// FR-AD-34: Admin management of tax configuration.
    /// Uses a single-row upsert pattern (Id = 1).
    /// </summary>
    public interface ITaxSettingsService
    {
        /// <summary>Returns the current tax settings. Creates defaults if none exist.</summary>
        Task<TaxSettingsDto> GetTaxSettingsAsync();

        /// <summary>Overwrites the current tax settings.</summary>
        Task<TaxSettingsDto> UpdateTaxSettingsAsync(UpdateTaxSettingsDto dto, string adminId);
    }
}
