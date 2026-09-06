using Jolisoft.Demo.WebAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace Jolisoft.Demo.WebAPI.Data;

public sealed class WorkflowDbContext(DbContextOptions<WorkflowDbContext> options) : DbContext(options)
{
    public DbSet<WorkflowRecord> WorkflowRecords => Set<WorkflowRecord>();
}