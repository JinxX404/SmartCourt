using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using SmartCourt.Features.Matching.DTOs;

namespace SmartCourt.Features.Matching;

public interface IMatchingService
{
    Task<List<ScoredLawyerCandidate>> FindAndScoreMatchesAsync(Guid caseId, CancellationToken cancellationToken = default);
    Task<FinalizeResultDto> ProcessMatchingAndPersistAsync(Guid caseId, CancellationToken cancellationToken = default);
    Task<FinalizeResultDto> GetRecommendationsAsync(Guid caseId, Guid currentUserId, CancellationToken cancellationToken = default);
}
