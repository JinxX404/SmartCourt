using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartCourt.Common.Models;
using SmartCourt.Features.Articles.DTOs;

namespace SmartCourt.Features.Articles;

[ApiController]
[Route("api/[controller]")]
public class ArticlesController : ControllerBase
{
    private readonly IArticleService _articleService;

    public ArticlesController(IArticleService articleService)
    {
        _articleService = articleService;
    }

    // --- Public / General Endpoints ---

    [HttpGet("public")]
    [AllowAnonymous]
    public async Task<ActionResult<PagedResponse<List<ArticleSummaryDto>>>> GetPublishedArticles(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] Guid? categoryId = null,
        [FromQuery] Guid? authorId = null,
        [FromQuery] string? searchQuery = null,
        CancellationToken cancellationToken = default)
    {
        var response = await _articleService.GetPublishedArticlesAsync(pageNumber, pageSize, categoryId, authorId, searchQuery, cancellationToken);
        return Ok(response);
    }

    [HttpGet("public/{id}")]
    [AllowAnonymous]
    public async Task<ActionResult<ApiResponse<ArticleDto>>> GetArticle(Guid id, CancellationToken cancellationToken)
    {
        var response = await _articleService.GetArticleAsync(id, cancellationToken);
        return Ok(response);
    }

    [HttpGet("public/{id}/comments")]
    [AllowAnonymous]
    public async Task<ActionResult<PagedResponse<List<ArticleCommentDto>>>> GetArticleComments(
        Guid id,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        var response = await _articleService.GetArticleCommentsAsync(id, pageNumber, pageSize, cancellationToken);
        return Ok(response);
    }

    // --- Authenticated User Endpoints (Clients/Lawyers) ---

    [HttpPost("{id}/like")]
    [Authorize]
    public async Task<ActionResult<ApiResponse<bool>>> LikeArticle(Guid id, CancellationToken cancellationToken)
    {
        var response = await _articleService.LikeArticleAsync(id, cancellationToken);
        return Ok(response);
    }

    [HttpPost("{id}/comments")]
    [Authorize]
    public async Task<ActionResult<ApiResponse<ArticleCommentDto>>> AddComment(Guid id, [FromBody] CreateCommentRequest request, CancellationToken cancellationToken)
    {
        var response = await _articleService.AddCommentAsync(id, request, cancellationToken);
        return Created(string.Empty, response);
    }

    [HttpPut("{articleId}/comments/{commentId}")]
    [Authorize]
    public async Task<ActionResult<ApiResponse<ArticleCommentDto>>> UpdateComment(Guid articleId, Guid commentId, [FromBody] UpdateCommentRequest request, CancellationToken cancellationToken)
    {
        var response = await _articleService.UpdateCommentAsync(articleId, commentId, request, cancellationToken);
        return Ok(response);
    }

    [HttpDelete("{articleId}/comments/{commentId}")]
    [Authorize]
    public async Task<ActionResult<ApiResponse<bool>>> DeleteComment(Guid articleId, Guid commentId, CancellationToken cancellationToken)
    {
        var response = await _articleService.DeleteCommentAsync(articleId, commentId, cancellationToken);
        return Ok(response);
    }

    [HttpPost("{id}/report")]
    [Authorize]
    public async Task<ActionResult<ApiResponse<bool>>> ReportArticle(Guid id, [FromBody] ReportArticleRequest request, CancellationToken cancellationToken)
    {
        var response = await _articleService.ReportArticleAsync(id, request, cancellationToken);
        return Ok(response);
    }

    [HttpGet("{id}/likers")]
    [Authorize]
    public async Task<ActionResult<PagedResponse<List<ArticleLikerDto>>>> GetArticleLikers(
        Guid id,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        var response = await _articleService.GetArticleLikersAsync(id, pageNumber, pageSize, cancellationToken);
        return Ok(response);
    }


    // --- Lawyer Endpoints ---

    [HttpPost("lawyer")]
    [Authorize(Roles = "Lawyer")]
    public async Task<ActionResult<ApiResponse<ArticleDto>>> CreateArticle([FromForm] CreateArticleRequest request, CancellationToken cancellationToken)
    {
        var response = await _articleService.CreateArticleAsync(request, cancellationToken);
        return Created(string.Empty, response);
    }

    [HttpPut("lawyer/{id}")]
    [Authorize(Roles = "Lawyer")]
    public async Task<ActionResult<ApiResponse<ArticleDto>>> UpdateArticle(Guid id, [FromForm] UpdateArticleRequest request, CancellationToken cancellationToken)
    {
        var response = await _articleService.UpdateArticleAsync(id, request, cancellationToken);
        return Ok(response);
    }

    [HttpDelete("lawyer/{id}")]
    [Authorize(Roles = "Lawyer")]
    public async Task<ActionResult<ApiResponse<bool>>> DeleteArticle(Guid id, CancellationToken cancellationToken)
    {
        var response = await _articleService.DeleteArticleAsync(id, cancellationToken);
        return Ok(response);
    }

    [HttpPut("lawyer/{id}/status")]
    [Authorize(Roles = "Lawyer")]
    public async Task<ActionResult<ApiResponse<ArticleDto>>> ChangeStatus(Guid id, CancellationToken cancellationToken)
    {
        var response = await _articleService.ChangeArticleStatusAsync(id, cancellationToken);
        return Ok(response);
    }

    [HttpGet("lawyer/{id}")]
    [Authorize(Roles = "Lawyer")]
    public async Task<ActionResult<ApiResponse<ArticleDto>>> GetMyArticle(Guid id, CancellationToken cancellationToken)
    {
        var response = await _articleService.GetMyArticleAsync(id, cancellationToken);
        return Ok(response);
    }

    [HttpGet("lawyer/drafts")]
    [Authorize(Roles = "Lawyer")]
    public async Task<ActionResult<PagedResponse<List<ArticleSummaryDto>>>> GetMyDrafts(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        var response = await _articleService.GetMyDraftsAsync(pageNumber, pageSize, cancellationToken);
        return Ok(response);
    }

    [HttpGet("lawyer/published")]
    [Authorize(Roles = "Lawyer")]
    public async Task<ActionResult<PagedResponse<List<ArticleSummaryDto>>>> GetMyPublishedArticles(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        var response = await _articleService.GetMyPublishedAsync(pageNumber, pageSize, cancellationToken);
        return Ok(response);
    }

    [HttpGet("my-likes")]
    [Authorize]
    public async Task<ActionResult<PagedResponse<List<ArticleSummaryDto>>>> GetMyLikedArticles(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        var response = await _articleService.GetMyLikedArticlesAsync(pageNumber, pageSize, cancellationToken);
        return Ok(response);
    }

    // --- Admin Endpoints ---

    [HttpGet("admin/reported")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<PagedResponse<List<ArticleReportDto>>>> GetReportedArticles(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        var response = await _articleService.GetReportedArticlesAsync(pageNumber, pageSize, cancellationToken);
        return Ok(response);
    }

    [HttpGet("admin/deleted-by-admin")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<PagedResponse<List<ArticleSummaryDto>>>> GetAdminDeleted(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        var response = await _articleService.GetAdminDeletedArticlesAsync(pageNumber, pageSize, cancellationToken);
        return Ok(response);
    }

    [HttpGet("admin/deleted-by-lawyer")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<PagedResponse<List<ArticleSummaryDto>>>> GetLawyerDeleted(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        var response = await _articleService.GetLawyerDeletedArticlesAsync(pageNumber, pageSize, cancellationToken);
        return Ok(response);
    }

    [HttpDelete("admin/{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<ApiResponse<bool>>> AdminDeleteArticle(Guid id, CancellationToken cancellationToken)
    {
        var response = await _articleService.AdminDeleteArticleAsync(id, cancellationToken);
        return Ok(response);
    }

    [HttpPut("admin/reports/{reportId}/resolve")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<ApiResponse<bool>>> ResolveReport(Guid reportId, CancellationToken cancellationToken)
    {
        var response = await _articleService.ResolveReportAsync(reportId, cancellationToken);
        return Ok(response);
    }
}
