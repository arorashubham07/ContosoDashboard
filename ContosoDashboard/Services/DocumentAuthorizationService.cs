using ContosoDashboard.Data;
using ContosoDashboard.Models;
using Microsoft.EntityFrameworkCore;

namespace ContosoDashboard.Services;

public sealed class DocumentAuthorizationService(ApplicationDbContext context) : IDocumentAuthorizationService
{
    public async Task<bool> CanReadAsync(int callerUserId, Document document)
    {
        var user = await context.Users.FindAsync(callerUserId);
        if (user is null || document.AvailabilityState != DocumentAvailabilityState.Accepted) return false;
        if (user.Role == UserRole.Administrator) return true;
        var member = !document.ProjectId.HasValue || await context.ProjectMembers.AnyAsync(member => member.ProjectId == document.ProjectId && member.UserId == callerUserId);
        if (document.UploaderUserId == callerUserId && member) return true;
        if (document.ProjectId.HasValue && member) return true;
        if (user.Role == UserRole.ProjectManager && document.ProjectId.HasValue && await context.Projects.AnyAsync(project => project.ProjectId == document.ProjectId && project.ProjectManagerId == callerUserId)) return true;
        if (user.Role == UserRole.TeamLead && await context.Users.AnyAsync(uploader => uploader.UserId == document.UploaderUserId && uploader.Department == user.Department)) return true;
        return await context.DocumentShares.AnyAsync(share => share.DocumentId == document.DocumentId && share.RevokedAtUtc == null && (share.RecipientUserId == callerUserId || (share.RecipientDepartment == user.Department && (!document.ProjectId.HasValue || member))));
    }

    public async Task<bool> CanManageAsync(int callerUserId, Document document)
    {
        var user = await context.Users.FindAsync(callerUserId);
        if (user is null) return false;
        if (user.Role == UserRole.Administrator) return true;
        if (document.UploaderUserId == callerUserId && (!document.ProjectId.HasValue || await context.ProjectMembers.AnyAsync(member => member.ProjectId == document.ProjectId && member.UserId == callerUserId))) return true;
        if (user.Role == UserRole.ProjectManager && document.ProjectId.HasValue) return await context.Projects.AnyAsync(project => project.ProjectId == document.ProjectId && project.ProjectManagerId == callerUserId);
        return user.Role == UserRole.TeamLead && await context.Users.AnyAsync(uploader => uploader.UserId == document.UploaderUserId && uploader.Department == user.Department);
    }
    public async Task<bool> CanShareAsync(int callerUserId, Document document)
    {
        var user = await context.Users.FindAsync(callerUserId);
        return user?.Role == UserRole.Administrator || (document.UploaderUserId == callerUserId && (!document.ProjectId.HasValue || await context.ProjectMembers.AnyAsync(member => member.ProjectId == document.ProjectId && member.UserId == callerUserId)));
    }
    public async Task<bool> CanUploadToProjectAsync(int callerUserId, int? projectId) => !projectId.HasValue || await context.ProjectMembers.AnyAsync(member => member.ProjectId == projectId && member.UserId == callerUserId) || await context.Projects.AnyAsync(project => project.ProjectId == projectId && project.ProjectManagerId == callerUserId) || await context.Users.AnyAsync(user => user.UserId == callerUserId && user.Role == UserRole.Administrator);
}