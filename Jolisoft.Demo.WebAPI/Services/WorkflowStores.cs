using System.Collections.Concurrent;
using Jolisoft.Demo.WebAPI.Data;
using Jolisoft.Demo.WebAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace Jolisoft.Demo.WebAPI.Services;

public interface IWorkflowStore
{
    Task<IReadOnlyCollection<WorkflowRecord>> ListAsync(CancellationToken cancellationToken);
    Task<WorkflowRecord> AddAsync(WorkflowRecord record, CancellationToken cancellationToken);
}

public sealed class InMemoryWorkflowStore : IWorkflowStore
{
    private readonly ConcurrentDictionary<Guid, WorkflowRecord> records = new();

    public Task<IReadOnlyCollection<WorkflowRecord>> ListAsync(CancellationToken cancellationToken)
    {
        IReadOnlyCollection<WorkflowRecord> result = records.Values.OrderByDescending(record => record.CreatedAt).ToArray();
        return Task.FromResult(result);
    }

    public Task<WorkflowRecord> AddAsync(WorkflowRecord record, CancellationToken cancellationToken)
    {
        records[record.Id] = record;
        return Task.FromResult(record);
    }
}

public sealed class EfWorkflowStore(WorkflowDbContext db) : IWorkflowStore
{
    public async Task<IReadOnlyCollection<WorkflowRecord>> ListAsync(CancellationToken cancellationToken) =>
        await db.WorkflowRecords.AsNoTracking().OrderByDescending(record => record.CreatedAt).ToArrayAsync(cancellationToken);

    public async Task<WorkflowRecord> AddAsync(WorkflowRecord record, CancellationToken cancellationToken)
    {
        db.WorkflowRecords.Add(record);
        await db.SaveChangesAsync(cancellationToken);
        return record;
    }
}