using System.Collections.Concurrent;
using Jolisoft.Demo.EFLayer.Data.Generated;
using Jolisoft.Demo.EFLayer.Models.Generated;
using Jolisoft.Demo.WebAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace Jolisoft.Demo.WebAPI.Services;

public interface IWorkflowStore
{
    Task<IReadOnlyCollection<WorkflowRecords>> ListAsync(CancellationToken cancellationToken);
    Task<WorkflowRecords> AddAsync(WorkflowRecords record, CancellationToken cancellationToken);
}

public sealed class InMemoryWorkflowStore : IWorkflowStore
{
    private readonly ConcurrentDictionary<Guid, WorkflowRecords> records = new();

    public Task<IReadOnlyCollection<WorkflowRecords>> ListAsync(CancellationToken cancellationToken)
    {
        IReadOnlyCollection<WorkflowRecords> result = records.Values.OrderByDescending(record => record.CreatedAt).ToArray();
        return Task.FromResult(result);
    }

    public Task<WorkflowRecords> AddAsync(WorkflowRecords record, CancellationToken cancellationToken)
    {
        records[record.Id] = record;
        return Task.FromResult(record);
    }
}

public sealed class EfWorkflowStore(JolisoftDemoDbContext db) : IWorkflowStore
{
    public async Task<IReadOnlyCollection<WorkflowRecords>> ListAsync(CancellationToken cancellationToken) =>
        await db.WorkflowRecords.AsNoTracking().OrderByDescending(record => record.CreatedAt).ToArrayAsync(cancellationToken);

    public async Task<WorkflowRecords> AddAsync(WorkflowRecords record, CancellationToken cancellationToken)
    {
        db.WorkflowRecords.Add(record);
        await db.SaveChangesAsync(cancellationToken);
        return record;
    }
}