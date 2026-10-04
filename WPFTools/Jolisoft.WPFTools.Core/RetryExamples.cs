using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;

namespace Jolisoft.WPFTools.Core;

public sealed class SimulatedTransientException : Exception
{
    public SimulatedTransientException() : base("Simulated temporary service failure.") { }
}

public static class LocalRetryPolicy
{
    public static async Task<T> ExecuteAsync<T>(Func<T> operation, int maxAttempts,
        Func<TimeSpan, CancellationToken, Task> delay, Action<string> trace,
        CancellationToken cancellationToken = default)
    {
        if (maxAttempts < 1 || maxAttempts > 5) throw new ArgumentOutOfRangeException(nameof(maxAttempts));
        for (var attempt = 1; ; attempt++)
        {
            cancellationToken.ThrowIfCancellationRequested();
            trace($"Attempt {attempt}/{maxAttempts}");
            try { return operation(); }
            catch (SimulatedTransientException error) when (attempt < maxAttempts)
            {
                var wait = TimeSpan.FromMilliseconds(200 * Math.Pow(2, attempt - 1));
                trace($"{error.Message} Retry in {wait.TotalMilliseconds:0} ms.");
                await delay(wait, cancellationToken);
            }
        }
    }
}

public enum RetryScenario { RecoverAfterTwoFailures, ExhaustRetries, UnsupportedQuery }
public sealed record RetryOutcome(bool Succeeded, int Attempts, int Rows, string Message);

public static class RetryExamples
{
    public static async Task<RetryOutcome> RunAsync(RetryScenario scenario, Action<string> trace,
        Func<TimeSpan, CancellationToken, Task>? delay = null, CancellationToken cancellationToken = default)
    {
        if (!Enum.IsDefined(scenario)) throw new ArgumentOutOfRangeException(nameof(scenario));
        var service = new LocalOrganizationService(AssessmentExamples.JoinFixtures());
        var query = AssessmentExamples.JoinedQuery(true, true);
        if (scenario == RetryScenario.UnsupportedQuery)
            query.Criteria.AddCondition("acme_name", ConditionOperator.Like, "%");
        var attempts = 0;
        try
        {
            var result = await LocalRetryPolicy.ExecuteAsync(() =>
            {
                attempts++;
                if (scenario == RetryScenario.ExhaustRetries ||
                    scenario == RetryScenario.RecoverAfterTwoFailures && attempts <= 2)
                    throw new SimulatedTransientException();
                return service.RetrieveMultiple(query);
            }, 3, delay ?? ((wait, token) => Task.Delay(wait, token)), trace, cancellationToken);
            var message = $"Success after {attempts} attempts: {result.Entities.Count} rows returned.";
            trace(message);
            return new RetryOutcome(true, attempts, result.Entities.Count, message);
        }
        catch (Exception error) when (error is SimulatedTransientException or NotSupportedException)
        {
            var message = error is SimulatedTransientException
                ? $"Stopped after {attempts} attempts: retry limit reached."
                : $"Stopped after {attempts} attempt: unsupported query is not retried. {error.Message}";
            trace(message);
            return new RetryOutcome(false, attempts, 0, message);
        }
    }
}
