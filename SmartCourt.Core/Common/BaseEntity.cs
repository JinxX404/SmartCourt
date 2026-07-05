using System;

namespace SmartCourt.Core.Common;

public abstract class BaseEntity
{
    public Guid Id { get; set; }
    public bool IsDeleted { get; set; } = false;
}
