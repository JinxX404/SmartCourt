using SmartCourt.Common.Entities;
using SmartCourt.Common.Enums;

namespace SmartCourt.Entities
{
    public class CaseProfile : AuditableEntity
    {
        public Guid Id { get; set; }
        public Guid CaseId { get; set; }
        public Case Case { get; set; } = null!;
        public Specialization Specialization { get; set; }
        public LawyerLevel RequiredLawyerLevelId { get; set; }
        public CaseComplexity Complexity { get; set; }
    }
}
