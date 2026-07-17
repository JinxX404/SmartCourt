using SmartCourt.Common.Entities;
using System;


namespace SmartCourt.Entities;

public class TestEntity : AuditableEntity
{
    public string Name { get; set; } = string.Empty;
}
