namespace Jolisoft.Demo.WebAPI.Models;

public sealed record CreateWorkflowRequest(string Title, string CreatedBy);

public sealed record WorkflowResult(Guid Id, string Title, string Status, string CreatedBy, DateTimeOffset CreatedAt, string PlatformReference);