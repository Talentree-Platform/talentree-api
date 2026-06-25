using Talentree.Service.DTOs.PlatformSettings;

namespace Talentree.Service.Contracts
{
    /// <summary>
    /// FR-AD-33: Admin management of shipping configuration.
    /// Uses a single-row upsert pattern (Id = 1).
    /// </summary>
    public interface IShippingSettingsService
    {
        /// <summary>Returns the current shipping settings. Creates defaults if none exist.</summary>
        Task<ShippingSettingsDto> GetShippingSettingsAsync();

        /// <summary>Overwrites the current shipping settings.</summary>
        Task<ShippingSettingsDto> UpdateShippingSettingsAsync(UpdateShippingSettingsDto dto, string adminId);
    }
}
