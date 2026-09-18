using ContosoDashboard.Models;

namespace ContosoDashboard.Services;

public sealed record DocumentUploadRequest(int CallerUserId, string Title, string Category, string? Description, string? Tags, int? ProjectId, string FileName, string ContentType, Stream Content);
public sealed record DocumentMetadataUpdateRequest(int CallerUserId, int DocumentId, string Title, string? Description, string Category, string? Tags, string ConcurrencyToken);
public sealed record DocumentOperationResult(bool Succeeded, string? Error = null, int? DocumentId = null, bool Retryable = false);
public sealed record DocumentStreamResult(Stream Stream, string ContentType, string FileName, bool Inline);
public sealed record MacroInspectionResult(bool IsSafe, string? Error = null);
public enum MalwareScanResult { Clean, ThreatDetected, Indeterminate }

public interface IClock { DateTime UtcNow { get; } }
public sealed class SystemClock : IClock { public DateTime UtcNow => DateTime.UtcNow; }
public interface IFileStorageService { Task<string> StageAsync(Stream content, CancellationToken cancellationToken = default); Task MoveToFinalAsync(string stagingPath, string finalPath, CancellationToken cancellationToken = default); Task<Stream> OpenReadAsync(string path, CancellationToken cancellationToken = default); Task DeleteAsync(string path, CancellationToken cancellationToken = default); string CreateFinalPath(string extension); string GetFullPath(string path); }
public interface IMalwareScanner { Task<MalwareScanResult> ScanAsync(string path, CancellationToken cancellationToken = default); }
public interface IOfficeMacroInspector { Task<MacroInspectionResult> InspectAsync(string path, string extension, CancellationToken cancellationToken = default); }
public interface IDocumentAuthorizationService { Task<bool> CanReadAsync(int callerUserId, Document document); Task<bool> CanManageAsync(int callerUserId, Document document); Task<bool> CanShareAsync(int callerUserId, Document document); Task<bool> CanUploadToProjectAsync(int callerUserId, int? projectId); }
public interface IDocumentAuditService { Task RecordAsync(int documentId, int actorUserId, DocumentActivityAction action, DocumentActivityOutcome outcome, string? details = null, CancellationToken cancellationToken = default); }
public interface IDocumentRecoveryService { Task ReconcileAsync(CancellationToken cancellationToken = default); }
public interface IDocumentService { Task<DocumentOperationResult> UploadAsync(DocumentUploadRequest request, CancellationToken cancellationToken = default); Task<DocumentStreamResult?> PrepareDownloadAsync(int callerUserId, int documentId, bool preview, CancellationToken cancellationToken = default); Task<DocumentOperationResult> UpdateMetadataAsync(DocumentMetadataUpdateRequest request, CancellationToken cancellationToken = default); Task<IReadOnlyList<Document>> GetMyDocumentsAsync(int callerUserId, CancellationToken cancellationToken = default); }