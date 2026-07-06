using System;

using SmartCourt.Common;

namespace SmartCourt.Entities;

public class TestEntity : AuditableEntity
{
    public string Name { get; set; } = string.Empty;
}
