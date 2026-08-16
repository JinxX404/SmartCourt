using Microsoft.EntityFrameworkCore;
using SmartCourt.Common.Entities;
using SmartCourt.Common.Enums;
using SmartCourt.Common.Exceptions;
using SmartCourt.Common.Models;
using SmartCourt.Features.Articles.DTOs;
using SmartCourt.Interfaces;
using SmartCourt.Interfaces.Providers;
using SmartCourt.Persistence;
using Microsoft.AspNetCore.Http;
using SmartCourt.Infrastructure.Providers.Events;
using SmartCourt.Features.Articles.Events;

namespace SmartCourt.Features.Articles;

public class ArticleService : IArticleService
{
    private readonly ApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IFileStorageService _fileStorageService;
    private readonly IOutboxWriter _outboxWriter;

    public ArticleService(
        ApplicationDbContext context, 
        ICurrentUserService currentUserService, 
        IHttpContextAccessor httpContextAccessor,
        IFileStorageService fileStorageService,
        IOutboxWriter outboxWriter)
    {
        _context = context;
        _currentUserService = currentUserService;
        _httpContextAccessor = httpContextAccessor;
        _fileStorageService = fileStorageService;
        _outboxWriter = outboxWriter;
    }

    private bool IsAdmin() => _httpContextAccessor.HttpContext?.User.IsInRole("Admin") == true;

    public async Task<ApiResponse<List<CategoryDto>>> GetCategoriesAsync(CancellationToken cancellationToken)
    {
        var categories = await _context.LegalArticleCategories
            .AsNoTracking()
            .Where(c => !c.IsDeleted)
            .OrderBy(c => c.NameAr)
            .Select(c => new CategoryDto
            {
                Id = c.Id,
                Code = c.Code,
                NameAr = c.NameAr,
                Description = c.Description
            })
            .ToListAsync(cancellationToken);

        return ApiResponse<List<CategoryDto>>.Ok(categories);
    }

    public async Task<ApiResponse<CategoryDto>> CreateCategoryAsync(CreateCategoryRequest request, CancellationToken cancellationToken)
    {
        if (await _context.LegalArticleCategories.AnyAsync(c => c.Code == request.Code && !c.IsDeleted, cancellationToken))
            throw new BusinessException("التصنيف بهذا الرمز موجود مسبقاً.");

        var category = new LegalArticleCategory
        {
            Code = request.Code,
            NameAr = request.NameAr,
            Description = request.Description
        };

        _context.LegalArticleCategories.Add(category);
        await _context.SaveChangesAsync(cancellationToken);

        return ApiResponse<CategoryDto>.Created(new CategoryDto
        {
            Id = category.Id,
            Code = category.Code,
            NameAr = category.NameAr,
            Description = category.Description
        });
    }

    public async Task<ApiResponse<CategoryDto>> UpdateCategoryAsync(Guid id, UpdateCategoryRequest request, CancellationToken cancellationToken)
    {
        var category = await _context.LegalArticleCategories
            .FirstOrDefaultAsync(c => c.Id == id && !c.IsDeleted, cancellationToken)
            ?? throw new NotFoundException("التصنيف غير موجود.");

        category.NameAr = request.NameAr;
        category.Description = request.Description;

        await _context.SaveChangesAsync(cancellationToken);

        return ApiResponse<CategoryDto>.Ok(new CategoryDto
        {
            Id = category.Id,
            Code = category.Code,
            NameAr = category.NameAr,
            Description = category.Description
        });
    }

    public async Task<ApiResponse<bool>> DeleteCategoryAsync(Guid id, CancellationToken cancellationToken)
    {
        var category = await _context.LegalArticleCategories
            .Include(c => c.Articles)
            .FirstOrDefaultAsync(c => c.Id == id && !c.IsDeleted, cancellationToken)
            ?? throw new NotFoundException("التصنيف غير موجود.");

        if (category.Articles.Any(a => !a.IsDeleted))
            throw new BusinessException("لا يمكن حذف التصنيف لأنه مرتبط بمقالات نشطة.");

        category.IsDeleted = true;
        await _context.SaveChangesAsync(cancellationToken);

        return ApiResponse<bool>.Ok(true);
    }

    // Article Methods - Public/General
    public async Task<PagedResponse<List<ArticleSummaryDto>>> GetPublishedArticlesAsync(int pageNumber, int pageSize, Guid? categoryId, Guid? authorId, string? searchQuery, CancellationToken cancellationToken)
    {
        var query = _context.LegalArticles
            .AsNoTracking()
            .Include(a => a.Category)
            .Include(a => a.Author)
            .Where(a => a.Status == ArticleStatus.Published && !a.IsDeleted);

        if (categoryId.HasValue)
            query = query.Where(a => a.CategoryId == categoryId.Value);

        if (authorId.HasValue)
            query = query.Where(a => a.AuthorId == authorId.Value);

        if (!string.IsNullOrWhiteSpace(searchQuery))
        {
            var lowerQuery = searchQuery.ToLower();
            query = query.Where(a => a.Title.ToLower().Contains(lowerQuery) || 
                                     a.Content.ToLower().Contains(lowerQuery) || 
                                     (a.Tags != null && a.Tags.ToLower().Contains(lowerQuery)));
        }

        var total = await query.CountAsync(cancellationToken);
        
        var articlesList = await query
            .OrderByDescending(a => a.CreatedAt)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        var articles = new List<ArticleSummaryDto>();
        foreach (var a in articlesList)
        {
            articles.Add(await MapToArticleSummaryDtoAsync(a, cancellationToken));
        }

        var totalPages = (int)Math.Ceiling(total / (double)pageSize);
        return PagedResponse<List<ArticleSummaryDto>>.OkPaged(articles, pageNumber, pageSize, total, totalPages);
    }

    public async Task<ApiResponse<ArticleDto>> GetArticleAsync(Guid id, CancellationToken cancellationToken)
    {
        var article = await _context.LegalArticles
            .Include(a => a.Category)
            .Include(a => a.Author)
            .FirstOrDefaultAsync(a => a.Id == id && a.Status == ArticleStatus.Published && !a.IsDeleted, cancellationToken)
            ?? throw new NotFoundException("المقال غير موجود.");

        bool isLikedByCurrentUser = false;

        // Unique View Tracking
        if (_currentUserService.UserId.HasValue)
        {
            var userId = _currentUserService.UserId.Value;
            var hasViewed = await _context.ArticleViews
                .AnyAsync(v => v.ArticleId == article.Id && v.UserId == userId, cancellationToken);
            
            if (!hasViewed)
            {
                _context.ArticleViews.Add(new ArticleView { ArticleId = article.Id, UserId = userId });
                article.ViewCount++;
                await _context.SaveChangesAsync(cancellationToken);
            }

            isLikedByCurrentUser = await _context.ArticleLikes.AnyAsync(l => l.ArticleId == article.Id && l.UserId == userId, cancellationToken);
        }
        else
        {
            // Anonymous view increment
            article.ViewCount++;
            try
            {
                await _context.SaveChangesAsync(cancellationToken);
            }
            catch (DbUpdateConcurrencyException)
            {
                // Ignore concurrency exception for anonymous views
            }
        }

        return ApiResponse<ArticleDto>.Ok(await MapToArticleDtoAsync(article, isLikedByCurrentUser, cancellationToken));
    }

    public async Task<PagedResponse<List<ArticleCommentDto>>> GetArticleCommentsAsync(Guid id, int pageNumber, int pageSize, CancellationToken cancellationToken)
    {
        if (!await _context.LegalArticles.AnyAsync(a => a.Id == id && a.Status == ArticleStatus.Published && !a.IsDeleted, cancellationToken))
            throw new NotFoundException("المقال غير موجود.");

        var query = _context.ArticleComments
            .AsNoTracking()
            .Include(c => c.User)
            .Where(c => c.ArticleId == id && !c.IsDeleted)
            .OrderBy(c => c.CreatedAt)
            .Select(c => new ArticleCommentDto
            {
                Id = c.Id,
                ArticleId = c.ArticleId,
                UserId = c.UserId,
                UserName = c.User.FullName,
                Content = c.Content,
                CreatedAt = c.CreatedAt
            });

        var total = await query.CountAsync(cancellationToken);
        var comments = await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        var totalPages = (int)Math.Ceiling(total / (double)pageSize);
        return PagedResponse<List<ArticleCommentDto>>.OkPaged(comments, pageNumber, pageSize, total, totalPages);
    }

    // Engagement Methods (Clients/Lawyers)
    public async Task<ApiResponse<bool>> LikeArticleAsync(Guid id, CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId ?? throw new ForbiddenAccessException("مطلوب تسجيل الدخول.");

        var article = await _context.LegalArticles
            .FirstOrDefaultAsync(a => a.Id == id && a.Status == ArticleStatus.Published && !a.IsDeleted, cancellationToken)
            ?? throw new NotFoundException("المقال غير موجود.");

        var existingLike = await _context.ArticleLikes
            .FirstOrDefaultAsync(l => l.ArticleId == id && l.UserId == userId, cancellationToken);

        if (existingLike != null)
        {
            _context.ArticleLikes.Remove(existingLike);
            if (article.LikesCount > 0)
                article.LikesCount--;
        }
        else
        {
            _context.ArticleLikes.Add(new ArticleLike { ArticleId = id, UserId = userId });
            article.LikesCount++;

            // Batch/threshold likes notifications
            if (article.LikesCount % 5 == 0)
            {
                await _outboxWriter.EnqueueAsync(
                    new OutboxEvent(
                        EventType: ArticleEventTypes.ArticleLikeThresholdReached,
                        EventVersion: 1,
                        Payload: new ArticleLikeThresholdReachedV1(article.Id, article.AuthorId, article.LikesCount),
                        AggregateType: nameof(LegalArticle),
                        AggregateId: article.Id,
                        CorrelationId: Guid.NewGuid()),
                    cancellationToken);
            }
        }
        
        await _context.SaveChangesAsync(cancellationToken);

        return ApiResponse<bool>.Ok(existingLike == null);
    }

    public async Task<ApiResponse<ArticleCommentDto>> AddCommentAsync(Guid id, CreateCommentRequest request, CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId ?? throw new ForbiddenAccessException("مطلوب تسجيل الدخول.");

        var article = await _context.LegalArticles
            .FirstOrDefaultAsync(a => a.Id == id && a.Status == ArticleStatus.Published && !a.IsDeleted, cancellationToken)
            ?? throw new NotFoundException("المقال غير موجود.");

        var comment = new ArticleComment
        {
            ArticleId = id,
            UserId = userId,
            Content = request.Content
        };

        _context.ArticleComments.Add(comment);
        article.CommentsCount++;

        await _outboxWriter.EnqueueAsync(
            new OutboxEvent(
                EventType: ArticleEventTypes.ArticleCommentAdded,
                EventVersion: 1,
                Payload: new ArticleCommentAddedV1(article.Id, comment.Id, article.AuthorId, userId),
                AggregateType: nameof(LegalArticle),
                AggregateId: article.Id,
                CorrelationId: Guid.NewGuid()),
            cancellationToken);

        await _context.SaveChangesAsync(cancellationToken);

        var createdComment = await _context.ArticleComments
            .Include(c => c.User)
            .FirstAsync(c => c.Id == comment.Id, cancellationToken);

        return ApiResponse<ArticleCommentDto>.Created(new ArticleCommentDto
        {
            Id = createdComment.Id,
            ArticleId = createdComment.ArticleId,
            UserId = createdComment.UserId,
            UserName = createdComment.User.FullName,
            Content = createdComment.Content,
            CreatedAt = createdComment.CreatedAt
        });
    }

    public async Task<ApiResponse<ArticleCommentDto>> UpdateCommentAsync(Guid articleId, Guid commentId, UpdateCommentRequest request, CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId ?? throw new ForbiddenAccessException("مطلوب تسجيل الدخول.");

        var comment = await _context.ArticleComments
            .Include(c => c.User)
            .FirstOrDefaultAsync(c => c.Id == commentId && c.ArticleId == articleId && !c.IsDeleted, cancellationToken)
            ?? throw new NotFoundException("التعليق غير موجود.");

        if (comment.UserId != userId)
            throw new ForbiddenAccessException("لا تملك صلاحية تعديل هذا التعليق.");

        comment.Content = request.Content;
        await _context.SaveChangesAsync(cancellationToken);

        var dto = new ArticleCommentDto
        {
            Id = comment.Id,
            ArticleId = comment.ArticleId,
            UserId = comment.UserId,
            UserName = comment.User.FullName,
            Content = comment.Content,
            CreatedAt = comment.CreatedAt
        };

        return ApiResponse<ArticleCommentDto>.Ok(dto);
    }

    public async Task<ApiResponse<bool>> DeleteCommentAsync(Guid articleId, Guid commentId, CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId ?? throw new ForbiddenAccessException("مطلوب تسجيل الدخول.");

        var comment = await _context.ArticleComments
            .FirstOrDefaultAsync(c => c.Id == commentId && c.ArticleId == articleId && !c.IsDeleted, cancellationToken)
            ?? throw new NotFoundException("التعليق غير موجود.");

        if (comment.UserId != userId && !IsAdmin())
            throw new ForbiddenAccessException("لا تملك صلاحية حذف هذا التعليق.");

        comment.IsDeleted = true;
        
        var article = await _context.LegalArticles.FindAsync(new object[] { articleId }, cancellationToken);
        if (article != null && article.CommentsCount > 0)
        {
            article.CommentsCount--;
        }

        await _context.SaveChangesAsync(cancellationToken);
        return ApiResponse<bool>.Ok(true);
    }

    public async Task<ApiResponse<bool>> ReportArticleAsync(Guid id, ReportArticleRequest request, CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId ?? throw new ForbiddenAccessException("مطلوب تسجيل الدخول.");

        var article = await _context.LegalArticles
            .FirstOrDefaultAsync(a => a.Id == id && a.Status == ArticleStatus.Published && !a.IsDeleted, cancellationToken)
            ?? throw new NotFoundException("المقال غير موجود.");

        if (article.AuthorId == userId)
            throw new BusinessException("لا يمكنك الإبلاغ عن مقالك الخاص.");

        var existingReport = await _context.ArticleReports
            .AnyAsync(r => r.ArticleId == id && r.ReporterId == userId && !r.IsResolved, cancellationToken);
        
        if (existingReport)
            throw new BusinessException("لقد قمت بالإبلاغ عن هذا المقال مسبقاً.");

        var report = new ArticleReport
        {
            ArticleId = id,
            ReporterId = userId,
            Reason = request.Reason
        };

        _context.ArticleReports.Add(report);

        await _outboxWriter.EnqueueAsync(
            new OutboxEvent(
                EventType: ArticleEventTypes.ArticleReported,
                EventVersion: 1,
                Payload: new ArticleReportedV1(article.Id, report.Id),
                AggregateType: nameof(LegalArticle),
                AggregateId: article.Id,
                CorrelationId: Guid.NewGuid()),
            cancellationToken);

        await _context.SaveChangesAsync(cancellationToken);
        return ApiResponse<bool>.Ok(true);
    }

    public async Task<PagedResponse<List<ArticleLikerDto>>> GetArticleLikersAsync(Guid articleId, int pageNumber, int pageSize, CancellationToken cancellationToken)
    {
        if (!await _context.LegalArticles.AnyAsync(a => a.Id == articleId && a.Status == ArticleStatus.Published && !a.IsDeleted, cancellationToken))
            throw new NotFoundException("المقال غير موجود.");

        var query = _context.ArticleLikes
            .AsNoTracking()
            .Include(l => l.User)
            .Where(l => l.ArticleId == articleId)
            .OrderByDescending(l => l.CreatedAt);

        var total = await query.CountAsync(cancellationToken);
        var likersList = await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        var likers = new List<ArticleLikerDto>();
        foreach (var l in likersList)
        {
            var profilePic = await GetImageUrlSafeAsync(l.User.ProfilePictureUrl, cancellationToken);
            likers.Add(new ArticleLikerDto(l.User.Id, l.User.FullName, profilePic));
        }

        var totalPages = (int)Math.Ceiling(total / (double)pageSize);
        return PagedResponse<List<ArticleLikerDto>>.OkPaged(likers, pageNumber, pageSize, total, totalPages);
    }


    // Lawyer Methods
    public async Task<ApiResponse<ArticleDto>> CreateArticleAsync(CreateArticleRequest request, CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId ?? throw new ForbiddenAccessException("مطلوب تسجيل الدخول.");

        var lawyerProfile = await _context.LawyerProfiles
            .FirstOrDefaultAsync(lp => lp.UserId == userId, cancellationToken)
            ?? throw new ForbiddenAccessException("فقط المحامون يمكنهم إضافة مقالات.");

        var user = await _context.Users.FindAsync(new object[] { userId }, cancellationToken);
        if (user == null || user.Status != SmartCourt.Features.Auth.Enums.UserStatus.Active)
            throw new ForbiddenAccessException("فقط المحامون الموثقون يمكنهم نشر مقالات.");

        if (!await _context.LegalArticleCategories.AnyAsync(c => c.Id == request.CategoryId && !c.IsDeleted, cancellationToken))
            throw new NotFoundException("التصنيف غير موجود.");

        string? featuredImageUrl = null;
        if (request.FeaturedImage is { Length: > 0 })
        {
            var storagePath = $"articles/{Guid.NewGuid()}_{request.FeaturedImage.FileName}";
            using var stream = request.FeaturedImage.OpenReadStream();
            var uploadResult = await _fileStorageService.UploadAsync(stream, storagePath, request.FeaturedImage.FileName, cancellationToken);
            featuredImageUrl = uploadResult.StoragePath;
        }

        var article = new LegalArticle
        {
            Title = request.Title,
            Content = request.Content,
            Tags = request.Tags,
            FeaturedImageUrl = featuredImageUrl,
            CategoryId = request.CategoryId,
            AuthorId = userId,
            Status = request.IsDraft ? ArticleStatus.Draft : ArticleStatus.Published,
            ViewCount = 0
        };

        _context.LegalArticles.Add(article);
        await _context.SaveChangesAsync(cancellationToken);

        var createdArticle = await _context.LegalArticles
            .Include(a => a.Category)
            .Include(a => a.Author)
            .FirstAsync(a => a.Id == article.Id, cancellationToken);

        return ApiResponse<ArticleDto>.Created(await MapToArticleDtoAsync(createdArticle, false, cancellationToken));
    }

    public async Task<ApiResponse<ArticleDto>> UpdateArticleAsync(Guid id, UpdateArticleRequest request, CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId ?? throw new ForbiddenAccessException("مطلوب تسجيل الدخول.");

        var article = await _context.LegalArticles
            .Include(a => a.Category)
            .Include(a => a.Author)
            .FirstOrDefaultAsync(a => a.Id == id && !a.IsDeleted && !a.IsDeletedByAdmin, cancellationToken)
            ?? throw new NotFoundException("المقال غير موجود.");

        if (article.AuthorId != userId)
            throw new ForbiddenAccessException("لا تملك صلاحية تعديل هذا المقال.");

        if (!await _context.LegalArticleCategories.AnyAsync(c => c.Id == request.CategoryId && !c.IsDeleted, cancellationToken))
            throw new NotFoundException("التصنيف غير موجود.");

        article.Title = request.Title;
        article.Content = request.Content;
        article.Tags = request.Tags;
        article.CategoryId = request.CategoryId;
        article.Status = request.IsDraft ? ArticleStatus.Draft : ArticleStatus.Published;

        if (request.FeaturedImage is { Length: > 0 })
        {
            if (!string.IsNullOrEmpty(article.FeaturedImageUrl))
            {
                try { await _fileStorageService.DeleteAsync(article.FeaturedImageUrl, cancellationToken); } catch { /* Ignore */ }
            }

            var storagePath = $"articles/{Guid.NewGuid()}_{request.FeaturedImage.FileName}";
            using var stream = request.FeaturedImage.OpenReadStream();
            var uploadResult = await _fileStorageService.UploadAsync(stream, storagePath, request.FeaturedImage.FileName, cancellationToken);
            article.FeaturedImageUrl = uploadResult.StoragePath;
        }

        await _context.SaveChangesAsync(cancellationToken);

        return ApiResponse<ArticleDto>.Ok(await MapToArticleDtoAsync(article, false, cancellationToken));
    }

    public async Task<ApiResponse<bool>> DeleteArticleAsync(Guid id, CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId ?? throw new ForbiddenAccessException("مطلوب تسجيل الدخول.");

        var article = await _context.LegalArticles
            .FirstOrDefaultAsync(a => a.Id == id && !a.IsDeleted, cancellationToken)
            ?? throw new NotFoundException("المقال غير موجود.");

        if (article.AuthorId != userId)
            throw new ForbiddenAccessException("لا تملك صلاحية حذف هذا المقال.");

        article.IsDeleted = true;
        article.IsDeletedByAdmin = false;

        await _context.SaveChangesAsync(cancellationToken);

        if (!string.IsNullOrEmpty(article.FeaturedImageUrl))
        {
            try { await _fileStorageService.DeleteAsync(article.FeaturedImageUrl, cancellationToken); } catch { /* Ignore */ }
        }
        
        return ApiResponse<bool>.Ok(true);
    }

    public async Task<ApiResponse<ArticleDto>> ChangeArticleStatusAsync(Guid id, CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId ?? throw new ForbiddenAccessException("مطلوب تسجيل الدخول.");

        var user = await _context.Users.FindAsync(new object[] { userId }, cancellationToken);
        if (user == null || user.Status != SmartCourt.Features.Auth.Enums.UserStatus.Active)
            throw new ForbiddenAccessException("فقط المحامون الموثقون يمكنهم تغيير حالة المقالات.");

        var article = await _context.LegalArticles
            .Include(a => a.Category)
            .Include(a => a.Author)
            .FirstOrDefaultAsync(a => a.Id == id && !a.IsDeleted, cancellationToken)
            ?? throw new NotFoundException("المقال غير موجود.");

        if (article.AuthorId != userId)
            throw new ForbiddenAccessException("لا تملك صلاحية تعديل حالة المقال.");

        article.Status = article.Status == ArticleStatus.Draft ? ArticleStatus.Published : ArticleStatus.Draft;
        await _context.SaveChangesAsync(cancellationToken);

        return ApiResponse<ArticleDto>.Ok(await MapToArticleDtoAsync(article, false, cancellationToken));
    }

    public async Task<ApiResponse<ArticleDto>> GetMyArticleAsync(Guid id, CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId ?? throw new ForbiddenAccessException("مطلوب تسجيل الدخول.");

        var article = await _context.LegalArticles
            .Include(a => a.Category)
            .Include(a => a.Author)
            .FirstOrDefaultAsync(a => a.Id == id && a.AuthorId == userId && !a.IsDeleted, cancellationToken)
            ?? throw new NotFoundException("المقال غير موجود.");

        return ApiResponse<ArticleDto>.Ok(await MapToArticleDtoAsync(article, false, cancellationToken));
    }

    public async Task<PagedResponse<List<ArticleSummaryDto>>> GetMyDraftsAsync(int pageNumber, int pageSize, CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId ?? throw new ForbiddenAccessException("مطلوب تسجيل الدخول.");

        var query = _context.LegalArticles
            .AsNoTracking()
            .Include(a => a.Category)
            .Include(a => a.Author)
            .Where(a => a.AuthorId == userId && a.Status == ArticleStatus.Draft && !a.IsDeleted);

        return await GetPagedArticlesAsync(query, pageNumber, pageSize, cancellationToken);
    }

    public async Task<PagedResponse<List<ArticleSummaryDto>>> GetMyPublishedAsync(int pageNumber, int pageSize, CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId ?? throw new ForbiddenAccessException("مطلوب تسجيل الدخول.");

        var query = _context.LegalArticles
            .AsNoTracking()
            .Include(a => a.Category)
            .Include(a => a.Author)
            .Where(a => a.AuthorId == userId && a.Status == ArticleStatus.Published && !a.IsDeleted);

        return await GetPagedArticlesAsync(query, pageNumber, pageSize, cancellationToken);
    }

    public async Task<PagedResponse<List<ArticleSummaryDto>>> GetMyLikedArticlesAsync(int pageNumber, int pageSize, CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId ?? throw new ForbiddenAccessException("مطلوب تسجيل الدخول.");

        var query = _context.ArticleLikes
            .AsNoTracking()
            .Where(l => l.UserId == userId)
            .Select(l => l.Article)
            .Include(a => a.Category)
            .Include(a => a.Author)
            .Where(a => a.Status == ArticleStatus.Published && !a.IsDeleted);

        return await GetPagedArticlesAsync(query, pageNumber, pageSize, cancellationToken);
    }

    // Admin Methods
    public async Task<PagedResponse<List<ArticleReportDto>>> GetReportedArticlesAsync(int pageNumber, int pageSize, CancellationToken cancellationToken)
    {
        var query = _context.ArticleReports
            .AsNoTracking()
            .Include(r => r.Article)
            .Include(r => r.Reporter)
            .Where(r => !r.IsResolved && !r.Article.IsDeleted);

        var total = await query.CountAsync(cancellationToken);
        var reports = await query
            .OrderBy(r => r.CreatedAt)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .Select(r => new ArticleReportDto
            {
                Id = r.Id,
                ArticleId = r.ArticleId,
                ArticleTitle = r.Article.Title,
                ReporterId = r.ReporterId,
                ReporterName = r.Reporter.FullName,
                Reason = r.Reason,
                CreatedAt = r.CreatedAt,
                IsResolved = r.IsResolved
            })
            .ToListAsync(cancellationToken);

        var totalPages = (int)Math.Ceiling(total / (double)pageSize);
        return PagedResponse<List<ArticleReportDto>>.OkPaged(reports, pageNumber, pageSize, total, totalPages);
    }

    public async Task<PagedResponse<List<ArticleSummaryDto>>> GetAdminDeletedArticlesAsync(int pageNumber, int pageSize, CancellationToken cancellationToken)
    {
        var query = _context.LegalArticles
            .AsNoTracking()
            .Include(a => a.Category)
            .Include(a => a.Author)
            .Where(a => a.IsDeleted && a.IsDeletedByAdmin);

        return await GetPagedArticlesAsync(query, pageNumber, pageSize, cancellationToken);
    }

    public async Task<PagedResponse<List<ArticleSummaryDto>>> GetLawyerDeletedArticlesAsync(int pageNumber, int pageSize, CancellationToken cancellationToken)
    {
        var query = _context.LegalArticles
            .AsNoTracking()
            .Include(a => a.Category)
            .Include(a => a.Author)
            .Where(a => a.IsDeleted && !a.IsDeletedByAdmin);

        return await GetPagedArticlesAsync(query, pageNumber, pageSize, cancellationToken);
    }

    public async Task<ApiResponse<bool>> AdminDeleteArticleAsync(Guid id, CancellationToken cancellationToken)
    {
        var article = await _context.LegalArticles
            .FirstOrDefaultAsync(a => a.Id == id && !a.IsDeleted, cancellationToken)
            ?? throw new NotFoundException("المقال غير موجود.");

        article.IsDeleted = true;
        article.IsDeletedByAdmin = true;

        await _outboxWriter.EnqueueAsync(
            new OutboxEvent(
                EventType: ArticleEventTypes.ArticleDeletedByAdmin,
                EventVersion: 1,
                Payload: new ArticleDeletedByAdminV1(article.Id, article.AuthorId, article.Title),
                AggregateType: nameof(LegalArticle),
                AggregateId: article.Id,
                CorrelationId: Guid.NewGuid()),
            cancellationToken);

        await _context.SaveChangesAsync(cancellationToken);

        if (!string.IsNullOrEmpty(article.FeaturedImageUrl))
        {
            try { await _fileStorageService.DeleteAsync(article.FeaturedImageUrl, cancellationToken); } catch { /* Ignore */ }
        }
        
        return ApiResponse<bool>.Ok(true);
    }

    public async Task<ApiResponse<bool>> ResolveReportAsync(Guid reportId, CancellationToken cancellationToken)
    {
        var report = await _context.ArticleReports
            .FirstOrDefaultAsync(r => r.Id == reportId, cancellationToken)
            ?? throw new NotFoundException("البلاغ غير موجود.");

        report.IsResolved = true;
        await _context.SaveChangesAsync(cancellationToken);
        
        return ApiResponse<bool>.Ok(true);
    }

    private async Task<PagedResponse<List<ArticleSummaryDto>>> GetPagedArticlesAsync(IQueryable<LegalArticle> query, int pageNumber, int pageSize, CancellationToken cancellationToken)
    {
        var total = await query.CountAsync(cancellationToken);
        
        var articlesList = await query
            .OrderByDescending(a => a.CreatedAt)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        var articles = new List<ArticleSummaryDto>();
        foreach (var a in articlesList)
        {
            articles.Add(await MapToArticleSummaryDtoAsync(a, cancellationToken));
        }

        var totalPages = (int)Math.Ceiling(total / (double)pageSize);
        return PagedResponse<List<ArticleSummaryDto>>.OkPaged(articles, pageNumber, pageSize, total, totalPages);
    }

    private async Task<string?> GetImageUrlSafeAsync(string? storagePath, CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(storagePath))
            return null;

        try
        {
            return await _fileStorageService.GetDownloadUrlAsync(storagePath, cancellationToken);
        }
        catch
        {
            return null;
        }
    }

    private async Task<ArticleDto> MapToArticleDtoAsync(LegalArticle a, bool isLikedByCurrentUser, CancellationToken cancellationToken) => new()
    {
        Id = a.Id,
        Title = a.Title,
        Content = a.Content,
        Tags = a.Tags,
        FeaturedImageUrl = await GetImageUrlSafeAsync(a.FeaturedImageUrl, cancellationToken),
        ViewCount = a.ViewCount,
        LikesCount = a.LikesCount,
        CommentsCount = a.CommentsCount,
        IsLikedByCurrentUser = isLikedByCurrentUser,
        Status = a.Status,
        CategoryId = a.CategoryId,
        Category = new CategoryDto
        {
            Id = a.Category.Id,
            Code = a.Category.Code,
            NameAr = a.Category.NameAr,
            Description = a.Category.Description
        },
        AuthorId = a.AuthorId,
        AuthorName = a.Author.FullName,
        CreatedAt = a.CreatedAt,
        UpdatedAt = a.UpdatedAt
    };

    private async Task<ArticleSummaryDto> MapToArticleSummaryDtoAsync(LegalArticle a, CancellationToken cancellationToken) => new()
    {
        Id = a.Id,
        Title = a.Title,
        FeaturedImageUrl = await GetImageUrlSafeAsync(a.FeaturedImageUrl, cancellationToken),
        ViewCount = a.ViewCount,
        LikesCount = a.LikesCount,
        CommentsCount = a.CommentsCount,
        Status = a.Status,
        CategoryId = a.CategoryId,
        CategoryNameAr = a.Category.NameAr,
        AuthorId = a.AuthorId,
        AuthorName = a.Author.FullName,
        CreatedAt = a.CreatedAt
    };
}
