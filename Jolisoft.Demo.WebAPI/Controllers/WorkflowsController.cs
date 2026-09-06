using Jolisoft.Demo.WebAPI.Models;
using Jolisoft.Demo.WebAPI.Services;
using Microsoft.AspNetCore.Mvc;

namespace Jolisoft.Demo.WebAPI.Controllers;

[ApiController]
[Route("api/workflows")]
public sealed class WorkflowsController(IWorkflowStore store, IPlatformGateway platformGateway) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyCollection<WorkflowResult>>> List(CancellationToken cancellationToken)
    {
        var records = await store.ListAsync(cancellationToken);
        return Ok(records.Select(record => ToResult(record, $"ACME-LOCAL-{record.Id.ToString()[..8].ToUpperInvariant()}")));
    }

    [HttpPost]
    public async Task<ActionResult<WorkflowResult>> Create(CreateWorkflowRequest request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Title) || string.IsNullOrWhiteSpace(request.CreatedBy))
        {
            return BadRequest("Title and CreatedBy are required.");
        }

        var record = new WorkflowRecord
        {
            Id = Guid.NewGuid(),
            Title = request.Title.Trim(),
            CreatedBy = request.CreatedBy.Trim(),
            CreatedAt = DateTimeOffset.UtcNow,
        };

        await store.AddAsync(record, cancellationToken);
        var platformReference = await platformGateway.RegisterAsync(record, cancellationToken);
        return CreatedAtAction(nameof(List), new { id = record.Id }, ToResult(record, platformReference));
    }

    private static WorkflowResult ToResult(WorkflowRecord record, string platformReference) =>
        new(record.Id, record.Title, record.Status, record.CreatedBy, record.CreatedAt, platformReference);
}