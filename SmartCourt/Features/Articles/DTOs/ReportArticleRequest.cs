using System.ComponentModel.DataAnnotations;

namespace SmartCourt.Features.Articles.DTOs;

public class ReportArticleRequest
{
    [Required(ErrorMessage = "سبب البلاغ مطلوب.")]
    [StringLength(1000, ErrorMessage = "السبب يجب ألا يتجاوز 1000 حرف.")]
    public string Reason { get; set; } = string.Empty;
}
