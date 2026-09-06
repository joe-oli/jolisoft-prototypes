using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Jolisoft.Demo.WebAPI.Models;

[Table("WorkflowRecords")]
public sealed class WorkflowRecord
{
    [Key]
    public Guid Id { get; set; }

    [Required]
    [MaxLength(160)]
    public string Title { get; set; } = string.Empty;

    [Required]
    [MaxLength(40)]
    public string Status { get; set; } = "Draft";

    [Required]
    [MaxLength(160)]
    public string CreatedBy { get; set; } = string.Empty;

    public DateTimeOffset CreatedAt { get; set; }
}

public sealed record CreateWorkflowRequest(string Title, string CreatedBy);

public sealed record WorkflowResult(Guid Id, string Title, string Status, string CreatedBy, DateTimeOffset CreatedAt, string PlatformReference);