using ContosoDashboard.Data;
using ContosoDashboard.Models;
using Microsoft.EntityFrameworkCore;

namespace ContosoDashboard.Services;

public sealed class DocumentRecoveryService(ApplicationDbContext context, IFileStorageService storage, IClock clock, ILogger<DocumentRecoveryService> logger) : IDocumentRecoveryService
{
    public async Task ReconcileAsync(CancellationToken cancellationToken = default)
    {
        var records = await context.DocumentRecoveryRecords.Where(record => record.State != DocumentRecoveryState.Resolved).ToListAsync(cancellationToken);
        foreach (var record in records)
        {
            try
            {
                if (!string.IsNullOrEmpty(record.StagingPath)) await storage.DeleteAsync(record.StagingPath, cancellationToken);
                if (record.State != DocumentRecoveryState.MetadataCommitted && !string.IsNullOrEmpty(record.FinalPath)) await storage.DeleteAsync(record.FinalPath, cancellationToken);
                record.State = DocumentRecoveryState.Resolved;
                record.ResolvedAtUtc = clock.UtcNow;
            }
            catch (Exception exception)
            {
                record.State = DocumentRecoveryState.CleanupRequired;
                record.FailureDetail = exception.Message;
                logger.LogWarning(exception, "Unable to reconcile document recovery operation {OperationId}.", record.OperationId);
            }
        }
        await context.SaveChangesAsync(cancellationToken);
    }
}