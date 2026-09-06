using Jolisoft.Demo.WebAPI.Models;

namespace Jolisoft.Demo.WebAPI.Services;

public interface IPlatformGateway
{
    Task<string> RegisterAsync(WorkflowRecord record, CancellationToken cancellationToken);
}

public sealed class FakePlatformGateway : IPlatformGateway
{
    public Task<string> RegisterAsync(WorkflowRecord record, CancellationToken cancellationToken) =>
        Task.FromResult($"ACME-LOCAL-{record.Id.ToString()[..8].ToUpperInvariant()}");
}