using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Jolisoft.Demo.EFLayer.Models.Generated;

public partial class WorkflowRecords
{
    [Key]
    public Guid Id { get; set; }

    [StringLength(160)]
    public string Title { get; set; } = null!;

    [StringLength(40)]
    public string Status { get; set; } = null!;

    [StringLength(160)]
    public string CreatedBy { get; set; } = null!;

    public DateTimeOffset CreatedAt { get; set; }
}
