using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartCourt.Common.Models;
using SmartCourt.Features.Articles.DTOs;

namespace SmartCourt.Features.Articles;

[ApiController]
[Route("api/[controller]")]
public class ArticleCategoriesController : ControllerBase
{
    private readonly IArticleService _articleService;

    public ArticleCategoriesController(IArticleService articleService)
    {
        _articleService = articleService;
    }

    [HttpGet("public")]
    [AllowAnonymous]
    public async Task<ActionResult<ApiResponse<List<CategoryDto>>>> GetCategories(CancellationToken cancellationToken)
    {
        var response = await _articleService.GetCategoriesAsync(cancellationToken);
        return Ok(response);
    }

    [HttpPost("admin")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<ApiResponse<CategoryDto>>> CreateCategory([FromBody] CreateCategoryRequest request, CancellationToken cancellationToken)
    {
        var response = await _articleService.CreateCategoryAsync(request, cancellationToken);
        return Created(string.Empty, response);
    }

    [HttpPut("admin/{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<ApiResponse<CategoryDto>>> UpdateCategory(Guid id, [FromBody] UpdateCategoryRequest request, CancellationToken cancellationToken)
    {
        var response = await _articleService.UpdateCategoryAsync(id, request, cancellationToken);
        return Ok(response);
    }

    [HttpDelete("admin/{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<ApiResponse<bool>>> DeleteCategory(Guid id, CancellationToken cancellationToken)
    {
        var response = await _articleService.DeleteCategoryAsync(id, cancellationToken);
        return Ok(response);
    }
}
