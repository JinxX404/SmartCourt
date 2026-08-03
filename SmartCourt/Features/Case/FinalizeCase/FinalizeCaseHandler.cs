using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SmartCourt.Common.Enums;
using SmartCourt.Common.Exceptions;
using SmartCourt.Common.Models;
using SmartCourt.Features.Case.BusinessRules;
using SmartCourt.Features.CaseAnalysis;
using SmartCourt.Features.Matching;
using SmartCourt.Features.Matching.DTOs;
using SmartCourt.Interfaces;
using SmartCourt.Persistence;

namespace SmartCourt.Features.Case.FinalizeCase;

public class FinalizeCaseHandler(
    ApplicationDbContext dbContext,
    ICaseAnalysisService caseAnalysisService,
    IMatchingService matchingService,
    ICurrentUserService currentUserService,
    ILogger<FinalizeCaseHandler> logger) : IRequestHandler<FinalizeCaseCommand, ApiResponse<FinalizeResultDto>>
{
    private readonly ApplicationDbContext _dbContext = dbContext;
    private readonly ICaseAnalysisService _caseAnalysisService = caseAnalysisService;
    private readonly IMatchingService _matchingService = matchingService;
    private readonly ICurrentUserService _currentUserService = currentUserService;
    private readonly ILogger<FinalizeCaseHandler> _logger = logger;

    public async Task<ApiResponse<FinalizeResultDto>> Handle(FinalizeCaseCommand request, CancellationToken cancellationToken)
    {
        var currentUserId = _currentUserService.UserId
            ?? throw new UnauthorizedAccessException("المستخدم غير مصرح له.");

        var caseEntity = await _dbContext.Cases
            .FirstOrDefaultAsync(c => c.Id == request.CaseId, cancellationToken);

        if (caseEntity == null)
        {
            throw new NotFoundException("القضية غير موجودة.");
        }

        if (caseEntity.ClientId != currentUserId)
        {
            throw new ForbiddenAccessException("ليس لديك صلاحية لإتمام هذه القضية.");
        }

        // Idempotency: if case is already Matched, return existing recommendations
        if (caseEntity.Status == CaseStatus.Matched)
        {
            var existingResult = await _matchingService.GetRecommendationsAsync(request.CaseId, currentUserId, cancellationToken);
            return ApiResponse<FinalizeResultDto>.Ok(existingResult);
        }

        // Precondition: Case must be in Reviewed status
        if (caseEntity.Status != CaseStatus.Reviewed)
        {
            throw new BusinessException("لا يمكن إتمام القضية. يجب أن تكون القضية في حالة تم مراجعتها (Reviewed).");
        }

        // Execute orchestration pipeline within a transaction
        await using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);
        try
        {
            // Step 1: Transition to FinalSubmitted
            CaseStatusTransitionGuard.EnsureCanTransition(caseEntity.Status, CaseStatus.FinalSubmitted);
            caseEntity.Status = CaseStatus.FinalSubmitted;
            await _dbContext.SaveChangesAsync(cancellationToken);

            // Step 2: AI Analysis -> CaseProfile
            await _caseAnalysisService.AnalyzeCaseAsync(request.CaseId, cancellationToken);

            // Step 3: Transition to Analyzed
            CaseStatusTransitionGuard.EnsureCanTransition(caseEntity.Status, CaseStatus.Analyzed);
            caseEntity.Status = CaseStatus.Analyzed;
            await _dbContext.SaveChangesAsync(cancellationToken);

            // Step 4: Matching -> Scoring -> Explanation -> Recommendation Persistence
            var finalizeResult = await _matchingService.ProcessMatchingAndPersistAsync(request.CaseId, cancellationToken);

            // Step 5: Transition to Matched
            CaseStatusTransitionGuard.EnsureCanTransition(caseEntity.Status, CaseStatus.Matched);
            caseEntity.Status = CaseStatus.Matched;
            await _dbContext.SaveChangesAsync(cancellationToken);


            await transaction.CommitAsync(cancellationToken);
            return ApiResponse<FinalizeResultDto>.Ok(finalizeResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to finalize case {CaseId}. Rolling back transaction.", request.CaseId);
            await transaction.RollbackAsync(cancellationToken);

            // Detach entity so in-memory status changes don't persist across context reuse
            _dbContext.Entry(caseEntity).State = EntityState.Detached;
            throw;
        }
    }
}
