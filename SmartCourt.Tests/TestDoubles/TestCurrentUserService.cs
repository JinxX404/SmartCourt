using System;
using SmartCourt.Interfaces;

namespace SmartCourt.Tests.TestDoubles;

public class TestCurrentUserService : ICurrentUserService
{
    public Guid? UserId { get; set; }
    public bool IsAuthenticated => UserId.HasValue;
}
