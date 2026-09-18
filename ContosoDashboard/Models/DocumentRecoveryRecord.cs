using System.ComponentModel.DataAnnotations;

namespace ContosoDashboard.Models;

public class DocumentRecoveryRecord
{
    [Key]
    public int DocumentRecoveryRecordId { get; set; }

    [Required, MaxLength(32)]
    public string OperationId { get; set; } = Guid.NewGuid().ToString("N");

    public DocumentRecoveryOperationKind OperationKind { get; set; }

    [MaxLength(500)]
    public string? StagingPath { get; set; }

    [MaxLength(500)]
    public string? FinalPath { get; set; }

    public int? DocumentId { get; set; }
    public DocumentRecoveryState State { get; set; } = DocumentRecoveryState.IntentRecorded;
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    public DateTime? ResolvedAtUtc { get; set; }

    [MaxLength(2000)]
    public string? FailureDetail { get; set; }
}

public enum DocumentRecoveryOperationKind { Upload, Replace, Delete }
public enum DocumentRecoveryState { IntentRecorded, FileMoved, MetadataCommitted, CleanupRequired, Resolved }