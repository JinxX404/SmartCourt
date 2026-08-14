using SmartCourt.Common.Models;
using SmartCourt.Features.Articles.DTOs;

namespace SmartCourt.Features.Articles;

public interface IArticleService
{
    // Article Category Methods
    Task<ApiResponse<List<CategoryDto>>> GetCategoriesAsync(CancellationToken cancellationToken);
    Task<ApiResponse<CategoryDto>> CreateCategoryAsync(CreateCategoryRequest request, CancellationToken cancellationToken);
    Task<ApiResponse<CategoryDto>> UpdateCategoryAsync(Guid id, UpdateCategoryRequest request, CancellationToken cancellationToken);
    Task<ApiResponse<bool>> DeleteCategoryAsync(Guid id, CancellationToken cancellationToken);

    // Article Methods - Public/General
    Task<PagedResponse<List<ArticleSummaryDto>>> GetPublishedArticlesAsync(int pageNumber, int pageSize, Guid? categoryId, Guid? authorId, string? searchQuery, CancellationToken cancellationToken);
    Task<ApiResponse<ArticleDto>> GetArticleAsync(Guid id, CancellationToken cancellationToken);
    Task<PagedResponse<List<ArticleCommentDto>>> GetArticleCommentsAsync(Guid id, int pageNumber, int pageSize, CancellationToken cancellationToken);

    // Engagement Methods (Clients/Lawyers)
    Task<ApiResponse<bool>> LikeArticleAsync(Guid id, CancellationToken cancellationToken);
    Task<ApiResponse<ArticleCommentDto>> AddCommentAsync(Guid id, CreateCommentRequest request, CancellationToken cancellationToken);
    Task<ApiResponse<ArticleCommentDto>> UpdateCommentAsync(Guid articleId, Guid commentId, UpdateCommentRequest request, CancellationToken cancellationToken);
    Task<ApiResponse<bool>> DeleteCommentAsync(Guid articleId, Guid commentId, CancellationToken cancellationToken);
    Task<ApiResponse<bool>> ReportArticleAsync(Guid id, ReportArticleRequest request, CancellationToken cancellationToken);

    // Lawyer Methods
    Task<ApiResponse<ArticleDto>> CreateArticleAsync(CreateArticleRequest request, CancellationToken cancellationToken);
    Task<ApiResponse<ArticleDto>> UpdateArticleAsync(Guid id, UpdateArticleRequest request, CancellationToken cancellationToken);
    Task<ApiResponse<bool>> DeleteArticleAsync(Guid id, CancellationToken cancellationToken);
    Task<ApiResponse<ArticleDto>> ChangeArticleStatusAsync(Guid id, CancellationToken cancellationToken);
    Task<ApiResponse<ArticleDto>> GetMyArticleAsync(Guid id, CancellationToken cancellationToken);
    Task<PagedResponse<List<ArticleSummaryDto>>> GetMyDraftsAsync(int pageNumber, int pageSize, CancellationToken cancellationToken);
    Task<PagedResponse<List<ArticleSummaryDto>>> GetMyPublishedAsync(int pageNumber, int pageSize, CancellationToken cancellationToken);
    Task<PagedResponse<List<ArticleSummaryDto>>> GetMyLikedArticlesAsync(int pageNumber, int pageSize, CancellationToken cancellationToken);

    // Admin Methods
    Task<PagedResponse<List<ArticleReportDto>>> GetReportedArticlesAsync(int pageNumber, int pageSize, CancellationToken cancellationToken);
    Task<PagedResponse<List<ArticleSummaryDto>>> GetAdminDeletedArticlesAsync(int pageNumber, int pageSize, CancellationToken cancellationToken);
    Task<PagedResponse<List<ArticleSummaryDto>>> GetLawyerDeletedArticlesAsync(int pageNumber, int pageSize, CancellationToken cancellationToken);
    Task<ApiResponse<bool>> AdminDeleteArticleAsync(Guid id, CancellationToken cancellationToken);
    Task<ApiResponse<bool>> ResolveReportAsync(Guid reportId, CancellationToken cancellationToken);
}
