using AutoMapper;
using Talentree.Core;
using Talentree.Core.Entities;
using Talentree.Core.Enums;
using Talentree.Core.Exceptions;
using Talentree.Core.Specifications;
using Talentree.Service.Contracts;
using Talentree.Service.DTOs.PlatformSettings;

namespace Talentree.Service.Services
{
    /// <summary>
    /// FR-AD-36: Admin management of versioned legal/policy documents.
    /// Publishing a new version does NOT delete old rows — full history is preserved.
    /// </summary>
    public class PolicyService : IPolicyService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly INotificationHelperService _notificationHelper;

        public PolicyService(IUnitOfWork unitOfWork, IMapper mapper, INotificationHelperService notificationHelper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _notificationHelper = notificationHelper;
        }

        /// <inheritdoc/>
        public async Task<List<PolicyDocumentSummaryDto>> GetAllPolicySummariesAsync()
        {
            var all = await _unitOfWork.Repository<PlatformPolicy>()
                .GetAllAsync();

            // Return the latest version of each document type
            return all
                .GroupBy(p => p.DocumentType)
                .Select(g => g.OrderByDescending(p => p.VersionNumber).First())
                .Select(p => _mapper.Map<PolicyDocumentSummaryDto>(p))
                .ToList();
        }

        /// <inheritdoc/>
        public async Task<PolicyDocumentDto> GetCurrentPolicyAsync(PolicyDocumentType type)
        {
            var all = await _unitOfWork.Repository<PlatformPolicy>()
                .GetAllWithSpecificationsAsync(new Core.Specifications.PlatformSettingsSpecifications.PlatformPolicySpecification(type));

            var published = all.FirstOrDefault(p => p.IsPublished)
                ?? all.OrderByDescending(p => p.VersionNumber).FirstOrDefault()
                ?? throw new NotFoundException($"No policy document found for type '{type}'.");

            return _mapper.Map<PolicyDocumentDto>(published);
        }

        /// <inheritdoc/>
        public async Task<List<PolicyVersionHistoryDto>> GetVersionHistoryAsync(PolicyDocumentType type)
        {
            var all = await _unitOfWork.Repository<PlatformPolicy>()
                .GetAllWithSpecificationsAsync(new Core.Specifications.PlatformSettingsSpecifications.PlatformPolicySpecification(type));

            return all
                .OrderByDescending(p => p.VersionNumber)
                .Select(p => _mapper.Map<PolicyVersionHistoryDto>(p))
                .ToList();
        }

        /// <inheritdoc/>
        public async Task<PolicyDocumentDto> PublishPolicyAsync(
            PolicyDocumentType type, UpdatePolicyDocumentDto dto, string adminId)
        {
            var all = (await _unitOfWork.Repository<PlatformPolicy>()
                .GetAllWithSpecificationsAsync(new Core.Specifications.PlatformSettingsSpecifications.PlatformPolicySpecification(type)))
                .ToList();

            // Unpublish all previous versions of this document type
            foreach (var prev in all.Where(p => p.IsPublished))
            {
                prev.IsPublished = false;
                prev.UpdatedAt = DateTime.UtcNow;
                prev.UpdatedBy = adminId;
                _unitOfWork.Repository<PlatformPolicy>().Update(prev);
            }

            var nextVersion = all.Count > 0
                ? all.Max(p => p.VersionNumber) + 1
                : 1;

            var newPolicy = new PlatformPolicy
            {
                DocumentType = type,
                Content = dto.Content,
                VersionNumber = nextVersion,
                IsPublished = true,
                RequireUserAcceptance = dto.RequireUserAcceptance,
                PublishedAt = DateTime.UtcNow,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = adminId
            };

            _unitOfWork.Repository<PlatformPolicy>().Add(newPolicy);
            await _unitOfWork.CompleteAsync();

            // Notify all users if they must accept the new version
            if (dto.RequireUserAcceptance)
            {
                await _notificationHelper.NotifyAllAdmins(
                    title: $"Policy Updated: {type}",
                    message: $"A new version (v{nextVersion}) of the {type} policy has been published and requires user acceptance.",
                    type: Talentree.Core.Enums.NotificationType.System,
                    actionUrl: $"/policies/{type}"
                );
            }

            return _mapper.Map<PolicyDocumentDto>(newPolicy);
        }

        /// <inheritdoc/>
        public async Task<PolicyDocumentDto> SaveDraftPolicyAsync(
            PolicyDocumentType type, UpdatePolicyDocumentDto dto, string adminId)
        {
            var all = (await _unitOfWork.Repository<PlatformPolicy>()
                .GetAllWithSpecificationsAsync(new Core.Specifications.PlatformSettingsSpecifications.PlatformPolicySpecification(type)))
                .ToList();

            var nextVersion = all.Count > 0
                ? all.Max(p => p.VersionNumber) + 1
                : 1;

            var draft = new PlatformPolicy
            {
                DocumentType = type,
                Content = dto.Content,
                VersionNumber = nextVersion,
                IsPublished = false,
                RequireUserAcceptance = dto.RequireUserAcceptance,
                PublishedAt = null,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = adminId
            };

            _unitOfWork.Repository<PlatformPolicy>().Add(draft);
            await _unitOfWork.CompleteAsync();

            return _mapper.Map<PolicyDocumentDto>(draft);
        }
    }
}
