using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Talentree.Core;
using Talentree.Core.Entities;
using Talentree.Service.Contracts;
using Talentree.Service.DTOs.PlatformSettings;

namespace Talentree.Service.Services
{
    /// <summary>
    /// FR-AD-32: Platform commission and fee configuration.
    /// Uses a single-row upsert pattern — row with Id = 1 is the canonical settings object.
    /// </summary>
    public class CommissionSettingService : ICommissionSettingService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public CommissionSettingService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        /// <inheritdoc/>
        public async Task<CommissionSettingDto> GetCommissionSettingsAsync()
        {
            var settings = await GetOrCreateSettingsAsync();
            return _mapper.Map<CommissionSettingDto>(settings);
        }

        /// <inheritdoc/>
        public async Task<CommissionSettingDto> UpdateCommissionSettingsAsync(UpdateCommissionSettingDto dto, string adminId)
        {
            var settings = await GetOrCreateSettingsAsync();

            settings.PlatformCommissionPercent = dto.PlatformCommissionPercent;
            settings.IsTransactionFeePercent = dto.IsTransactionFeePercent;
            settings.TransactionFeeValue = dto.TransactionFeeValue;
            settings.MinimumPayoutAmount = dto.MinimumPayoutAmount;
            settings.PayoutProcessingFee = dto.PayoutProcessingFee;
            settings.UpdatedAt = DateTime.UtcNow;
            settings.UpdatedBy = adminId;

            _unitOfWork.Repository<CommissionSetting>().Update(settings);
            await _unitOfWork.CompleteAsync();

            return _mapper.Map<CommissionSettingDto>(settings);
        }

        // ── Private helpers ──────────────────────────────────────────

        /// <summary>
        /// Returns the single CommissionSetting row (Id = 1).
        /// Creates and persists a default row if none exists.
        /// </summary>
        private async Task<CommissionSetting> GetOrCreateSettingsAsync()
        {
            var all = await _unitOfWork.Repository<CommissionSetting>()
                .GetAllAsync();

            var settings = all.FirstOrDefault();
            if (settings != null) return settings;

            settings = new CommissionSetting
            {
                PlatformCommissionPercent = 0,
                IsTransactionFeePercent = true,
                TransactionFeeValue = 0,
                MinimumPayoutAmount = 0,
                PayoutProcessingFee = 0
            };
            _unitOfWork.Repository<CommissionSetting>().Add(settings);
            await _unitOfWork.CompleteAsync();
            return settings;
        }
    }
}
