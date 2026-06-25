using AutoMapper;
using Talentree.Core;
using Talentree.Core.Entities;
using Talentree.Service.Contracts;
using Talentree.Service.DTOs.PlatformSettings;

namespace Talentree.Service.Services
{
    /// <summary>
    /// FR-AD-33: Platform shipping configuration.
    /// Uses a single-row upsert pattern — row with Id = 1 is the canonical settings object.
    /// </summary>
    public class ShippingSettingsService : IShippingSettingsService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public ShippingSettingsService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        /// <inheritdoc/>
        public async Task<ShippingSettingsDto> GetShippingSettingsAsync()
        {
            var settings = await GetOrCreateSettingsAsync();
            return _mapper.Map<ShippingSettingsDto>(settings);
        }

        /// <inheritdoc/>
        public async Task<ShippingSettingsDto> UpdateShippingSettingsAsync(UpdateShippingSettingsDto dto, string adminId)
        {
            var settings = await GetOrCreateSettingsAsync();

            settings.IsFlatRatePerItem = dto.IsFlatRatePerItem;
            settings.FlatRate = dto.FlatRate;
            settings.FreeShippingEnabled = dto.FreeShippingEnabled;
            settings.FreeShippingThreshold = dto.FreeShippingThreshold;
            settings.EstimatedDeliveryDomesticDays = dto.EstimatedDeliveryDomesticDays;
            settings.EstimatedDeliveryInternationalDays = dto.EstimatedDeliveryInternationalDays;
            settings.InternationalShippingEnabled = dto.InternationalShippingEnabled;
            settings.UpdatedAt = DateTime.UtcNow;
            settings.UpdatedBy = adminId;

            _unitOfWork.Repository<ShippingSettings>().Update(settings);
            await _unitOfWork.CompleteAsync();

            return _mapper.Map<ShippingSettingsDto>(settings);
        }

        // ── Private helpers ──────────────────────────────────────────

        private async Task<ShippingSettings> GetOrCreateSettingsAsync()
        {
            var all = await _unitOfWork.Repository<ShippingSettings>()
                .GetAllAsync();

            var settings = all.FirstOrDefault();
            if (settings != null) return settings;

            settings = new ShippingSettings
            {
                IsFlatRatePerItem = false,
                FlatRate = 0,
                FreeShippingEnabled = false,
                FreeShippingThreshold = 0,
                EstimatedDeliveryDomesticDays = 3,
                EstimatedDeliveryInternationalDays = 14,
                InternationalShippingEnabled = false
            };
            _unitOfWork.Repository<ShippingSettings>().Add(settings);
            await _unitOfWork.CompleteAsync();
            return settings;
        }
    }
}
