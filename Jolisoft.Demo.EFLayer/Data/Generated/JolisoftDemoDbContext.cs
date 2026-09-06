using System;
using System.Collections.Generic;
using Jolisoft.Demo.EFLayer.Models.Generated;
using Microsoft.EntityFrameworkCore;

namespace Jolisoft.Demo.EFLayer.Data.Generated;

public partial class JolisoftDemoDbContext : DbContext
{
    public JolisoftDemoDbContext(DbContextOptions<JolisoftDemoDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<WorkflowRecords> WorkflowRecords { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<WorkflowRecords>(entity =>
        {
            entity.Property(e => e.Id).ValueGeneratedNever();
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
