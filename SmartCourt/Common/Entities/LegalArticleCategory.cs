namespace SmartCourt.Common.Entities;

public class LegalArticleCategory : AuditableEntity
{
    public string Code { get; set; } = string.Empty;
    public string NameAr { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsDeleted { get; set; }

    public ICollection<LegalArticle> Articles { get; set; } = new List<LegalArticle>();
}
