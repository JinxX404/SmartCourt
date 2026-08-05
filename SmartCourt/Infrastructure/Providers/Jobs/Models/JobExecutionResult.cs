namespace SmartCourt.Infrastructure.Providers.Jobs;

public enum JobExecutionOutcome
{
    Completed = 0,
    NoOp = 1
}

public sealed record JobExecutionResult(
    JobExecutionOutcome Outcome,
    string Reason,
    int AffectedCount = 0)
{
    public static JobExecutionResult Completed(
        string reason,
        int affectedCount = 1)
    {
        return new JobExecutionResult(
            JobExecutionOutcome.Completed,
            reason,
            affectedCount);
    }

    public static JobExecutionResult NoOp(string reason)
    {
        return new JobExecutionResult(
            JobExecutionOutcome.NoOp,
            reason);
    }
}
