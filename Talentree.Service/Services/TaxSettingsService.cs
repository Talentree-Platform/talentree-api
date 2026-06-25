using AutoMapper;
using Talentree.Core;
using Talentree.Core.Entities;
using Talentree.Service.Contracts;
using Talentree.Service.DTOs.PlatformSettings;

namespace Talentree.Service.Services
{
    /// <summary>
    /// FR-AD-34: Platform tax configuration.
    /// Uses a single-row upsert pattern — row with Id = 1 is the canonical settings object.
    /// </summary>
    public class TaxSettingsService : ITaxSettingsService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public TaxSettingsService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        /// <inheritdoc/>
        public async Task<TaxSettingsDto> GetTaxSettingsAsync()
        {
            var settings = await GetOrCreateSettingsAsync();
            return MapToDto(settings);
        }

        /// <inheritdoc/>
        public async Task<TaxSettingsDto> UpdateTaxSettingsAsync(UpdateTaxSettingsDto dto, string adminId)
        {
            var settings = await GetOrCreateSettingsAsync();

            settings.TaxEnabled = dto.TaxEnabled;
            settings.TaxRate = dto.TaxRate;
            settings.IsInclusive = dto.IsInclusive;
            settings.TaxExemptCategoryIds = dto.TaxExemptCategoryIds.Count > 0
                ? string.Join(",", dto.TaxExemptCategoryIds)
                : null;
            settings.UpdatedAt = DateTime.UtcNow;
            settings.UpdatedBy = adminId;

            _unitOfWork.Repository<TaxSettings>().Update(settings);
            await _unitOfWork.CompleteAsync();

            return MapToDto(settings);
        }

        // ── Private helpers ──────────────────────────────────────────

        private async Task<TaxSettings> GetOrCreateSettingsAsync()
        {
            var all = await _unitOfWork.Repository<TaxSettings>()
                .GetAllAsync();

            var settings = all.FirstOrDefault();
            if (settings != null) return settings;

            settings = new TaxSettings
            {
                TaxEnabled = false,
                TaxRate = 0,
                IsInclusive = false
            };
            _unitOfWork.Repository<TaxSettings>().Add(settings);
            await _unitOfWork.CompleteAsync();
            return settings;
        }

        /// <summary>
        /// Manual mapping for TaxSettings → TaxSettingsDto to handle the
        /// comma-separated TaxExemptCategoryIds → List&lt;int&gt; conversion.
        /// </summary>
        private static TaxSettingsDto MapToDto(TaxSettings settings) => new()
        {
            TaxEnabled = settings.TaxEnabled,
            TaxRate = settings.TaxRate,
            IsInclusive = settings.IsInclusive,
            TaxExemptCategoryIds = string.IsNullOrWhiteSpace(settings.TaxExemptCategoryIds)
                ? new List<int>()
                : settings.TaxExemptCategoryIds
                    .Split(',', StringSplitOptions.RemoveEmptyEntries)
                    .Select(int.Parse)
                    .ToList(),
            UpdatedAt = settings.UpdatedAt ?? settings.CreatedAt,
            UpdatedBy = settings.UpdatedBy
        };
    }
}
