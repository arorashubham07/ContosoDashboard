using ContosoDashboard.Data;
using ContosoDashboard.Models;
using Microsoft.EntityFrameworkCore;

namespace ContosoDashboard.Services;

public sealed class DocumentService(ApplicationDbContext context, IDocumentAuthorizationService authorization, IFileStorageService storage, IOfficeMacroInspector macroInspector, IMalwareScanner malwareScanner, IDocumentAuditService audit, IClock clock) : IDocumentService
{
    private static readonly HashSet<string> Categories = ["Project Documents", "Team Resources", "Personal Files", "Reports", "Presentations", "Other"];
    private static readonly Dictionary<string, string> ContentTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        [".pdf"] = "application/pdf", [".doc"] = "application/msword", [".docx"] = "application/vnd.openxmlformats-officedocument.wordprocessingml.document", [".xls"] = "application/vnd.ms-excel", [".xlsx"] = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", [".ppt"] = "application/vnd.ms-powerpoint", [".pptx"] = "application/vnd.openxmlformats-officedocument.presentationml.presentation", [".txt"] = "text/plain", [".jpg"] = "image/jpeg", [".jpeg"] = "image/jpeg", [".png"] = "image/png"
    };

    public async Task<DocumentOperationResult> UploadAsync(DocumentUploadRequest request, CancellationToken cancellationToken = default)
    {
        var extension = Path.GetExtension(request.FileName);
        if (string.IsNullOrWhiteSpace(request.Title) || request.Title.Trim().Length > 255 || !Categories.Contains(request.Category)) return new(false, "A title and valid category are required.");
        if (!ContentTypes.TryGetValue(extension, out var expectedContentType) || !string.Equals(expectedContentType, request.ContentType, StringComparison.OrdinalIgnoreCase)) return new(false, "The file type is not supported or does not match its content type.");
        if (!await authorization.CanUploadToProjectAsync(request.CallerUserId, request.ProjectId)) return new(false, "The project is unavailable.");

        string? stagingPath = null;
        try
        {
            stagingPath = await storage.StageAsync(request.Content, cancellationToken);
            var length = new FileInfo(storage.GetFullPath(stagingPath)).Length;
            if (length is <= 0 or > 25_000_000) return new(false, "Files must be between 1 byte and 25,000,000 bytes.");
            var inspection = await macroInspector.InspectAsync(storage.GetFullPath(stagingPath), extension, cancellationToken);
            if (!inspection.IsSafe) return new(false, inspection.Error);
            if (await malwareScanner.ScanAsync(storage.GetFullPath(stagingPath), cancellationToken) != MalwareScanResult.Clean) return new(false, "The file could not be verified as safe.");

            var finalPath = storage.CreateFinalPath(extension);
            var recovery = new DocumentRecoveryRecord { OperationKind = DocumentRecoveryOperationKind.Upload, StagingPath = stagingPath, FinalPath = finalPath, CreatedAtUtc = clock.UtcNow };
            context.DocumentRecoveryRecords.Add(recovery);
            await context.SaveChangesAsync(cancellationToken);
            await storage.MoveToFinalAsync(stagingPath, finalPath, cancellationToken);
            recovery.State = DocumentRecoveryState.FileMoved;

            var document = new Document { Title = request.Title.Trim(), Description = request.Description?.Trim(), Category = request.Category, Tags = request.Tags?.Trim().ToUpperInvariant(), ProjectId = request.ProjectId, UploaderUserId = request.CallerUserId, OriginalFileName = Path.GetFileName(request.FileName), FilePath = finalPath, FileSizeBytes = length, ContentType = expectedContentType, UploadedAtUtc = clock.UtcNow, AvailabilityState = DocumentAvailabilityState.Accepted };
            context.Documents.Add(document);
            await context.SaveChangesAsync(cancellationToken);
            await audit.RecordAsync(document.DocumentId, request.CallerUserId, DocumentActivityAction.Upload, DocumentActivityOutcome.Succeeded, cancellationToken: cancellationToken);
            recovery.DocumentId = document.DocumentId;
            recovery.State = DocumentRecoveryState.Resolved;
            recovery.ResolvedAtUtc = clock.UtcNow;
            await context.SaveChangesAsync(cancellationToken);
            return new(true, DocumentId: document.DocumentId);
        }
        catch (Exception)
        {
            return new(false, "The upload could not be completed. Please retry.", Retryable: true);
        }
        finally
        {
            if (stagingPath is not null) await storage.DeleteAsync(stagingPath, cancellationToken);
        }
    }

    public async Task<DocumentStreamResult?> PrepareDownloadAsync(int callerUserId, int documentId, bool preview, CancellationToken cancellationToken = default)
    {
        var document = await context.Documents.SingleOrDefaultAsync(item => item.DocumentId == documentId && item.AvailabilityState == DocumentAvailabilityState.Accepted, cancellationToken);
        if (document is null || !await authorization.CanReadAsync(callerUserId, document)) return null;
        if (preview && document.ContentType is not ("application/pdf" or "image/jpeg" or "image/png")) return null;
        await audit.RecordAsync(document.DocumentId, callerUserId, preview ? DocumentActivityAction.Preview : DocumentActivityAction.Download, DocumentActivityOutcome.Succeeded, cancellationToken: cancellationToken);
        return new(await storage.OpenReadAsync(document.FilePath, cancellationToken), document.ContentType, document.OriginalFileName, preview);
    }

    public async Task<DocumentOperationResult> UpdateMetadataAsync(DocumentMetadataUpdateRequest request, CancellationToken cancellationToken = default)
    {
        var document = await context.Documents.SingleOrDefaultAsync(item => item.DocumentId == request.DocumentId, cancellationToken);
        if (document is null || !await authorization.CanManageAsync(request.CallerUserId, document)) return new(false, "The document is unavailable.");
        if (document.ConcurrencyToken != request.ConcurrencyToken) return new(false, "The document changed. Reload and retry.");
        if (string.IsNullOrWhiteSpace(request.Title) || !Categories.Contains(request.Category)) return new(false, "A title and valid category are required.");
        document.Title = request.Title.Trim(); document.Description = request.Description?.Trim(); document.Category = request.Category; document.Tags = request.Tags?.Trim().ToUpperInvariant(); document.ConcurrencyToken = Guid.NewGuid().ToString("N");
        await context.SaveChangesAsync(cancellationToken);
        await audit.RecordAsync(document.DocumentId, request.CallerUserId, DocumentActivityAction.Replace, DocumentActivityOutcome.Succeeded, cancellationToken: cancellationToken);
        return new(true, DocumentId: document.DocumentId);
    }

    public async Task<IReadOnlyList<Document>> GetMyDocumentsAsync(int callerUserId, CancellationToken cancellationToken = default)
    {
        var documents = await context.Documents.Include(document => document.Project).Where(document => document.UploaderUserId == callerUserId && document.AvailabilityState == DocumentAvailabilityState.Accepted).OrderByDescending(document => document.UploadedAtUtc).ThenByDescending(document => document.DocumentId).ToListAsync(cancellationToken);
        var visible = new List<Document>();
        foreach (var document in documents)
        {
            if (await authorization.CanReadAsync(callerUserId, document)) visible.Add(document);
        }
        return visible;
    }
}