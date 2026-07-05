using System;

using SmartCourt.Core.Common;

namespace SmartCourt.Core.Entities;

public class TestEntity : AuditableEntity
{
    public string Name { get; set; } = string.Empty;
}
