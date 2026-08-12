using System.ComponentModel.DataAnnotations;

namespace SmartCourt.Features.Articles.DTOs;

public class CreateCommentRequest
{
    [Required(ErrorMessage = "محتوى التعليق مطلوب.")]
    [StringLength(1000, ErrorMessage = "التعليق يجب ألا يتجاوز 1000 حرف.")]
    public string Content { get; set; } = string.Empty;
}
