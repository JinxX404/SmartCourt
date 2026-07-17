using SmartCourt.Common.Entities;
using System;

namespace SmartCourt.Entities;

public class SampleEntity : AuditableEntity
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
}
