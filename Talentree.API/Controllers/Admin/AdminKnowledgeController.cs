using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Talentree.API.Models;
using Talentree.Service.Contracts;
using Talentree.Service.DTOs.Common;
using Talentree.Service.DTOs.Knowledge;

namespace Talentree.API.Controllers.Admin
{
    /// <summary>
    /// FR-AD-40 to FR-AD-43: Admin Education Content Management.
    /// Provides full CRUD + publish/unpublish/delete/restore for Knowledge Base articles,
    /// plus content analytics.
    /// </summary>
    [Authorize(Roles = "Admin")]
    public class AdminKnowledgeController : BaseApiController
    {
        private readonly IAdminKnowledgeService _adminKnowledgeService;

        public AdminKnowledgeController(IAdminKnowledgeService adminKnowledgeService)
        {
            _adminKnowledgeService = adminKnowledgeService;
        }

        private string GetAdminId() => User.FindFirstValue(ClaimTypes.NameIdentifier)!;

        // ═══════════════════════════════════════════════════════════
        // GET: api/admin/knowledge
        // FR-AD-40: Content Library — paginated, filtered, sorted
        // ═══════════════════════════════════════════════════════════
        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<Pagination<AdminArticleDto>>), StatusCodes.Status200OK)]
        public async Task<ActionResult<ApiResponse<Pagination<AdminArticleDto>>>> GetAllArticles(
            [FromQuery] AdminArticleFilterDto filter)
        {
            var result = await _adminKnowledgeService.GetAllArticlesAsync(filter);

            return Ok(ApiResponse<Pagination<AdminArticleDto>>.SuccessResponse(
                data: result,
                message: $"Retrieved {result.Data.Count} articles (Page {result.PageIndex} of {result.TotalPages})"
            ));
        }

        // ═══════════════════════════════════════════════════════════
        // GET: api/admin/knowledge/{id}
        // FR-AD-40: Single article detail
        // ═══════════════════════════════════════════════════════════
        [HttpGet("{id:int}")]
        [ProducesResponseType(typeof(ApiResponse<AdminArticleDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ApiResponse<AdminArticleDto>>> GetArticle(int id)
        {
            var result = await _adminKnowledgeService.GetArticleByIdAsync(id);

            return Ok(ApiResponse<AdminArticleDto>.SuccessResponse(
                data: result,
                message: "Article retrieved successfully"
            ));
        }

        // ═══════════════════════════════════════════════════════════
        // POST: api/admin/knowledge
        // FR-AD-41: Upload / Create Content
        // ═══════════════════════════════════════════════════════════
        [HttpPost]
        [Consumes("multipart/form-data")]
        [ProducesResponseType(typeof(ApiResponse<AdminArticleDto>), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<ApiResponse<AdminArticleDto>>> CreateArticle(
            [FromForm] CreateArticleDto dto)
        {
            var result = await _adminKnowledgeService.CreateArticleAsync(dto, GetAdminId());

            return CreatedAtAction(
                nameof(GetArticle),
                new { id = result.Id },
                ApiResponse<AdminArticleDto>.SuccessResponse(
                    data: result,
                    message: $"Article '{result.Title}' created successfully as {(result.IsPublished ? "Published" : "Draft")}"
                ));
        }

        // ═══════════════════════════════════════════════════════════
        // PUT: api/admin/knowledge/{id}
        // FR-AD-42: Edit Content
        // ═══════════════════════════════════════════════════════════
        [HttpPut("{id:int}")]
        [Consumes("multipart/form-data")]
        [ProducesResponseType(typeof(ApiResponse<AdminArticleDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ApiResponse<AdminArticleDto>>> UpdateArticle(
            int id,
            [FromForm] UpdateArticleDto dto)
        {
            var result = await _adminKnowledgeService.UpdateArticleAsync(id, dto, GetAdminId());

            return Ok(ApiResponse<AdminArticleDto>.SuccessResponse(
                data: result,
                message: "Article updated successfully"
            ));
        }

        // ═══════════════════════════════════════════════════════════
        // POST: api/admin/knowledge/{id}/publish
        // FR-AD-42: Publish a draft article
        // ═══════════════════════════════════════════════════════════
        [HttpPost("{id:int}/publish")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ApiResponse<object>>> PublishArticle(int id)
        {
            await _adminKnowledgeService.PublishArticleAsync(id, GetAdminId());

            return Ok(ApiResponse<object>.SuccessResponse(
                message: "Article published successfully. It is now visible to Business Owners."
            ));
        }

        // ═══════════════════════════════════════════════════════════
        // POST: api/admin/knowledge/{id}/unpublish
        // FR-AD-42: Unpublish — hides from Business Owners
        // ═══════════════════════════════════════════════════════════
        [HttpPost("{id:int}/unpublish")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ApiResponse<object>>> UnpublishArticle(int id)
        {
            await _adminKnowledgeService.UnpublishArticleAsync(id, GetAdminId());

            return Ok(ApiResponse<object>.SuccessResponse(
                message: "Article unpublished. It is now hidden from Business Owners."
            ));
        }

        // ═══════════════════════════════════════════════════════════
        // DELETE: api/admin/knowledge/{id}
        // FR-AD-42: Soft delete (requires confirmation from frontend)
        // ═══════════════════════════════════════════════════════════
        [HttpDelete("{id:int}")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ApiResponse<object>>> DeleteArticle(int id)
        {
            await _adminKnowledgeService.DeleteArticleAsync(id, GetAdminId());

            return Ok(ApiResponse<object>.SuccessResponse(
                message: "Article deleted. It can be restored from the deleted items view."
            ));
        }

        // ═══════════════════════════════════════════════════════════
        // POST: api/admin/knowledge/{id}/restore
        // FR-AD-42: Restore soft-deleted article
        // ═══════════════════════════════════════════════════════════
        [HttpPost("{id:int}/restore")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ApiResponse<object>>> RestoreArticle(int id)
        {
            await _adminKnowledgeService.RestoreArticleAsync(id, GetAdminId());

            return Ok(ApiResponse<object>.SuccessResponse(
                message: "Article restored as Draft. Publish it when ready."
            ));
        }

        // ═══════════════════════════════════════════════════════════
        // GET: api/admin/knowledge/analytics
        // FR-AD-43: Analytics dashboard summary
        // ═══════════════════════════════════════════════════════════
        [HttpGet("analytics")]
        [ProducesResponseType(typeof(ApiResponse<ContentAnalyticsSummaryDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<ApiResponse<ContentAnalyticsSummaryDto>>> GetAnalyticsSummary()
        {
            var result = await _adminKnowledgeService.GetAnalyticsSummaryAsync();

            return Ok(ApiResponse<ContentAnalyticsSummaryDto>.SuccessResponse(
                data: result,
                message: "Analytics summary retrieved successfully"
            ));
        }

        // ═══════════════════════════════════════════════════════════
        // GET: api/admin/knowledge/analytics/articles
        // FR-AD-43: Per-article analytics (sorted)
        // ═══════════════════════════════════════════════════════════
        [HttpGet("analytics/articles")]
        [ProducesResponseType(typeof(ApiResponse<Pagination<ArticleAnalyticsDto>>), StatusCodes.Status200OK)]
        public async Task<ActionResult<ApiResponse<Pagination<ArticleAnalyticsDto>>>> GetArticleAnalytics(
            [FromQuery] int pageIndex = 1,
            [FromQuery] int pageSize = 20,
            [FromQuery] string sortBy = "views",
            [FromQuery] bool sortDesc = true)
        {
            var result = await _adminKnowledgeService.GetArticleAnalyticsAsync(
                pageIndex, pageSize, sortBy, sortDesc);

            return Ok(ApiResponse<Pagination<ArticleAnalyticsDto>>.SuccessResponse(
                data: result,
                message: $"Retrieved analytics for {result.Data.Count} articles"
            ));
        }
    }
}
