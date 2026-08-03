using System;
using MediatR;
using SmartCourt.Common.Models;
using SmartCourt.Features.Matching.DTOs;

namespace SmartCourt.Features.Case.FinalizeCase;

public class FinalizeCaseCommand : IRequest<ApiResponse<FinalizeResultDto>>
{
    public Guid CaseId { get; set; }
}
