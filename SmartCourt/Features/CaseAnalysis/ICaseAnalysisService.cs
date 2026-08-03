using System;
using System.Threading;
using System.Threading.Tasks;
using SmartCourt.Entities;

namespace SmartCourt.Features.CaseAnalysis;

public interface ICaseAnalysisService
{
    Task<CaseProfile> AnalyzeCaseAsync(Guid caseId, CancellationToken cancellationToken = default);
}
