using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Talentree.API.Models;
using Talentree.Core.Enums;
using Talentree.Service.Contracts;
using Talentree.Service.DTOs.PlatformSettings;

namespace Talentree.API.Controllers
{
    /// <summary>
    /// FR-AD-36: Admin management of versioned legal/policy documents.
    /// Each publish creates a new versioned row; previous versions are preserved.
    /// </summary>
    [Authorize(Roles = "Admin")]
    [Route("api/admin/platform/policies")]
    [ApiController]
    public class PlatformPoliciesController : BaseApiController
    {
        private readonly IPolicyService _policyService;

        public PlatformPoliciesController(IPolicyService policyService)
        {
            _policyService = policyService;
        }

        // ═══════════════════════════════════════════════════════════
        // GET: api/admin/platform/policies
        // Lightweight summaries of all 5 policy types (latest version each)
        // ═══════════════════════════════════════════════════════════
        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<List<PolicyDocumentSummaryDto>>), StatusCodes.Status200OK)]
        public async Task<ActionResult<ApiResponse<List<PolicyDocumentSummaryDto>>>> GetAllPolicySummaries()
        {
            var result = await _policyService.GetAllPolicySummariesAsync();
            return Ok(ApiResponse<List<PolicyDocumentSummaryDto>>.SuccessResponse(
                data: result,
                message: $"Retrieved {result.Count} policy summaries"));
        }

        // ═══════════════════════════════════════════════════════════
        // GET: api/admin/platform/policies/{type}
        // Full document content for the currently published version
        // ═══════════════════════════════════════════════════════════
        [HttpGet("{type}")]
        [ProducesResponseType(typeof(ApiResponse<PolicyDocumentDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ApiResponse<PolicyDocumentDto>>> GetCurrentPolicy(
            [FromRoute] PolicyDocumentType type)
        {
            var result = await _policyService.GetCurrentPolicyAsync(type);
            return Ok(ApiResponse<PolicyDocumentDto>.SuccessResponse(
                data: result,
                message: $"Retrieved current {type} policy (v{result.VersionNumber})"));
        }

        // ═══════════════════════════════════════════════════════════
        // GET: api/admin/platform/policies/{type}/history
        // Full version history for a document type (newest first)
        // ═══════════════════════════════════════════════════════════
        [HttpGet("{type}/history")]
        [ProducesResponseType(typeof(ApiResponse<List<PolicyVersionHistoryDto>>), StatusCodes.Status200OK)]
        public async Task<ActionResult<ApiResponse<List<PolicyVersionHistoryDto>>>> GetVersionHistory(
            [FromRoute] PolicyDocumentType type)
        {
            var result = await _policyService.GetVersionHistoryAsync(type);
            return Ok(ApiResponse<List<PolicyVersionHistoryDto>>.SuccessResponse(
                data: result,
                message: $"Retrieved {result.Count} versions for {type}"));
        }

        // ═══════════════════════════════════════════════════════════
        // PUT: api/admin/platform/policies/{type}/publish
        // Publish a new version — increments VersionNumber, unpublishes previous version.
        // Notifies users if RequireUserAcceptance is true.
        // ═══════════════════════════════════════════════════════════
        [HttpPut("{type}/publish")]
        [ProducesResponseType(typeof(ApiResponse<PolicyDocumentDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<ApiResponse<PolicyDocumentDto>>> PublishPolicy(
            [FromRoute] PolicyDocumentType type,
            [FromBody] UpdatePolicyDocumentDto dto)
        {
            var result = await _policyService.PublishPolicyAsync(type, dto, GetCurrentUserId());
            return Ok(ApiResponse<PolicyDocumentDto>.SuccessResponse(
                data: result,
                message: $"{type} published as version {result.VersionNumber}"));
        }

        // ═══════════════════════════════════════════════════════════
        // PUT: api/admin/platform/policies/{type}/draft
        // Save a draft — does NOT affect the currently live version.
        // ═══════════════════════════════════════════════════════════
        [HttpPut("{type}/draft")]
        [ProducesResponseType(typeof(ApiResponse<PolicyDocumentDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<ApiResponse<PolicyDocumentDto>>> SaveDraft(
            [FromRoute] PolicyDocumentType type,
            [FromBody] UpdatePolicyDocumentDto dto)
        {
            var result = await _policyService.SaveDraftPolicyAsync(type, dto, GetCurrentUserId());
            return Ok(ApiResponse<PolicyDocumentDto>.SuccessResponse(
                data: result,
                message: $"Draft saved as version {result.VersionNumber} for {type}"));
        }
    }
}
