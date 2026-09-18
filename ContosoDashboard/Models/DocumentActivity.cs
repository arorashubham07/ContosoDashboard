using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ContosoDashboard.Models;

public class DocumentActivity
{
    [Key]
    public int DocumentActivityId { get; set; }

    public int DocumentId { get; set; }
    public int ActorUserId { get; set; }
    public DocumentActivityAction Action { get; set; }
    public DocumentActivityOutcome Outcome { get; set; }
    public DateTime OccurredAtUtc { get; set; } = DateTime.UtcNow;

    [MaxLength(500)]
    public string? RecipientSummary { get; set; }

    [MaxLength(2000)]
    public string? Details { get; set; }

    [ForeignKey(nameof(ActorUserId))]
    public virtual User ActorUser { get; set; } = null!;
}

public enum DocumentActivityAction { Upload, Replace, Download, Preview, Delete, ShareGrant, ShareRevoke }
public enum DocumentActivityOutcome { Succeeded, Failed, Blocked }