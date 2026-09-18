using ContosoDashboard.Models;
using ContosoDashboard.Services;

namespace ContosoDashboard.Tests;

public sealed class DocumentAuthorizationTests
{
    [Fact]
    public async Task ScopedRolesCanReadProjectDocumentButOnlyOwnerCanShare()
    {
        using var host = DocumentTestHost.Create();
        var document = new Document { Title = "Plan", Category = "Project Documents", OriginalFileName = "plan.pdf", FilePath = "accepted/plan.pdf", FileSizeBytes = 1, ContentType = "application/pdf", UploaderUserId = 4, ProjectId = 1, AvailabilityState = DocumentAvailabilityState.Accepted };
        host.Context.Documents.Add(document);
        await host.Context.SaveChangesAsync();
        var authorization = new DocumentAuthorizationService(host.Context);

        Assert.True(await authorization.CanReadAsync(3, document));
        Assert.True(await authorization.CanReadAsync(2, document));
        Assert.False(await authorization.CanShareAsync(3, document));
        Assert.True(await authorization.CanShareAsync(4, document));
    }

    [Fact]
    public async Task FormerUploaderLosesProjectDocumentAccess()
    {
        using var host = DocumentTestHost.Create();
        var document = new Document { Title = "Plan", Category = "Project Documents", OriginalFileName = "plan.pdf", FilePath = "accepted/plan.pdf", FileSizeBytes = 1, ContentType = "application/pdf", UploaderUserId = 4, ProjectId = 1, AvailabilityState = DocumentAvailabilityState.Accepted };
        host.Context.Documents.Add(document);
        host.Context.ProjectMembers.Remove(host.Context.ProjectMembers.Single(member => member.ProjectId == 1 && member.UserId == 4));
        await host.Context.SaveChangesAsync();
        var authorization = new DocumentAuthorizationService(host.Context);

        Assert.False(await authorization.CanReadAsync(4, document));
        Assert.True(await authorization.CanReadAsync(2, document));
    }
}