using ContosoDashboard.Data;
using ContosoDashboard.Models;

namespace ContosoDashboard.Services;

public sealed class DocumentAuditService(ApplicationDbContext context, IClock clock) : IDocumentAuditService
{
    public async Task RecordAsync(int documentId, int actorUserId, DocumentActivityAction action, DocumentActivityOutcome outcome, string? details = null, CancellationToken cancellationToken = default)
    {
        context.DocumentActivities.Add(new DocumentActivity { DocumentId = documentId, ActorUserId = actorUserId, Action = action, Outcome = outcome, Details = details, OccurredAtUtc = clock.UtcNow });
        await context.SaveChangesAsync(cancellationToken);
    }
}