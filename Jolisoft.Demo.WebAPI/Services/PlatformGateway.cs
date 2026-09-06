using Jolisoft.Demo.EFLayer.Models.Generated;

namespace Jolisoft.Demo.WebAPI.Services;

public interface IPlatformGateway
{
    Task<string> RegisterAsync(WorkflowRecords record, CancellationToken cancellationToken);
}

public sealed class FakePlatformGateway : IPlatformGateway
{
    public Task<string> RegisterAsync(WorkflowRecords record, CancellationToken cancellationToken) =>
        Task.FromResult($"ACME-LOCAL-{record.Id.ToString()[..8].ToUpperInvariant()}");
}