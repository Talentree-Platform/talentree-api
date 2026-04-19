using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Talentree.API.Models;
using Talentree.Service.Contracts;
using Talentree.Service.DTOs.Common;
using Talentree.Service.DTOs.Knowledge;

namespace Talentree.API.Controllers
{
    [Authorize(Roles = "BusinessOwner")]
    public class KnowledgeBaseController : BaseApiController
    {
        private readonly IKnowledgeService _knowledgeService;

        public KnowledgeBaseController(IKnowledgeService knowledgeService)
        {
            _knowledgeService = knowledgeService;
        }

        private string GetUserId() =>
            User.FindFirstValue(ClaimTypes.NameIdentifier)!;

        // ═══════════════════════════════════════════════════════════
        // GET: api/knowledge-base
        // FR-BO-36: Browse articles with search, filter, category
        // ═══════════════════════════════════════════════════════════
        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<Pagination<ArticleDto>>), StatusCodes.Status200OK)]
        public async Task<ActionResult<ApiResponse<Pagination<ArticleDto>>>> GetArticles(
            [FromQuery] ArticleFilterDto filter)
        {
            var result = await _knowledgeService.GetArticlesAsync(GetUserId(), filter);

            return Ok(ApiResponse<Pagination<ArticleDto>>.SuccessResponse(
                data: result,
                message: $"Retrieved {result.Data.Count} articles " +
                         $"(Page {result.PageIndex} of {result.TotalPages})"
            ));
        }

        // ═══════════════════════════════════════════════════════════
        // GET: api/knowledge-base/{id}
        // FR-BO-36: Get single article
        // ═══════════════════════════════════════════════════════════
        [HttpGet("{id:int}")]
        [ProducesResponseType(typeof(ApiResponse<ArticleDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ApiResponse<ArticleDto>>> GetArticle(int id)
        {
            var result = await _knowledgeService.GetArticleByIdAsync(id, GetUserId());

            return Ok(ApiResponse<ArticleDto>.SuccessResponse(
                data: result,
                message: "Article retrieved successfully"
            ));
        }

        // ═══════════════════════════════════════════════════════════
        // POST: api/knowledge-base/{id}/bookmark
        // FR-BO-36: Bookmark an article
        // ═══════════════════════════════════════════════════════════
        [HttpPost("{id:int}/bookmark")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<ApiResponse<object>>> BookmarkArticle(int id)
        {
            await _knowledgeService.BookmarkArticleAsync(id, GetUserId());

            return Ok(ApiResponse<object>.SuccessResponse(
                message: "Article bookmarked successfully"
            ));
        }

        // ═══════════════════════════════════════════════════════════
        // DELETE: api/knowledge-base/{id}/bookmark
        // FR-BO-36: Remove bookmark
        // ═══════════════════════════════════════════════════════════
        [HttpDelete("{id:int}/bookmark")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ApiResponse<object>>> UnbookmarkArticle(int id)
        {
            await _knowledgeService.UnbookmarkArticleAsync(id, GetUserId());

            return Ok(ApiResponse<object>.SuccessResponse(
                message: "Bookmark removed successfully"
            ));
        }

        // ═══════════════════════════════════════════════════════════
        // GET: api/knowledge-base/bookmarks
        // FR-BO-36: Get all bookmarked articles
        // ═══════════════════════════════════════════════════════════
        [HttpGet("bookmarks")]
        [ProducesResponseType(typeof(ApiResponse<List<ArticleDto>>), StatusCodes.Status200OK)]
        public async Task<ActionResult<ApiResponse<List<ArticleDto>>>> GetBookmarks()
        {
            var result = await _knowledgeService.GetBookmarkedArticlesAsync(GetUserId());

            return Ok(ApiResponse<List<ArticleDto>>.SuccessResponse(
                data: result,
                message: $"Retrieved {result.Count} bookmarked articles"
            ));
        }

        // ═══════════════════════════════════════════════════════════
        // GET: api/knowledge-base/recommendations
        // FR-BO-37: Get recommended articles
        // ═══════════════════════════════════════════════════════════
        [HttpGet("recommendations")]
        [ProducesResponseType(typeof(ApiResponse<List<RecommendedArticleDto>>), StatusCodes.Status200OK)]
        public async Task<ActionResult<ApiResponse<List<RecommendedArticleDto>>>> GetRecommendations()
        {
            var result = await _knowledgeService.GetRecommendationsAsync(GetUserId());

            return Ok(ApiResponse<List<RecommendedArticleDto>>.SuccessResponse(
                data: result,
                message: "Recommendations retrieved successfully"
            ));
        }
    }
}