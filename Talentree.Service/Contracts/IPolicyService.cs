using Talentree.Core.Enums;
using Talentree.Service.DTOs.PlatformSettings;

namespace Talentree.Service.Contracts
{
    /// <summary>
    /// FR-AD-36: Admin management of versioned legal/policy documents.
    /// </summary>
    public interface IPolicyService
    {
        /// <summary>Returns a lightweight summary of all policy document types (latest version each).</summary>
        Task<List<PolicyDocumentSummaryDto>> GetAllPolicySummariesAsync();

        /// <summary>Returns the currently published version of a specific document type.</summary>
        Task<PolicyDocumentDto> GetCurrentPolicyAsync(PolicyDocumentType type);

        /// <summary>Returns the full version history for a specific document type (newest first).</summary>
        Task<List<PolicyVersionHistoryDto>> GetVersionHistoryAsync(PolicyDocumentType type);

        /// <summary>
        /// Saves and immediately publishes a new version of a policy document.
        /// Automatically increments VersionNumber and unpublishes the previously active version.
        /// Sends user notifications if RequireUserAcceptance is true.
        /// </summary>
        Task<PolicyDocumentDto> PublishPolicyAsync(PolicyDocumentType type, UpdatePolicyDocumentDto dto, string adminId);

        /// <summary>
        /// Saves a draft version of a policy document without publishing it.
        /// IsPublished = false; does not affect the currently live version.
        /// </summary>
        Task<PolicyDocumentDto> SaveDraftPolicyAsync(PolicyDocumentType type, UpdatePolicyDocumentDto dto, string adminId);
    }
}
