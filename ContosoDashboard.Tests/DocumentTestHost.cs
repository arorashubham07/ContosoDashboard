using ContosoDashboard.Data;
using Microsoft.EntityFrameworkCore;

namespace ContosoDashboard.Tests;

public sealed class DocumentTestHost : IDisposable
{
    private DocumentTestHost(string rootPath)
    {
        RootPath = rootPath;
        DatabasePath = Path.Combine(rootPath, "document-tests.db");
        StorageRootPath = Path.Combine(rootPath, "private-storage");
        Directory.CreateDirectory(StorageRootPath);

        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseSqlite($"Data Source={DatabasePath}")
            .Options;

        Context = new ApplicationDbContext(options);
        Context.Database.EnsureCreated();
    }

    public string RootPath { get; }
    public string DatabasePath { get; }
    public string StorageRootPath { get; }
    public ApplicationDbContext Context { get; }

    public static DocumentTestHost Create()
    {
        var rootPath = Path.Combine(Path.GetTempPath(), "ContosoDashboard.Tests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(rootPath);
        return new DocumentTestHost(rootPath);
    }

    public void Dispose()
    {
        Context.Dispose();
        if (Directory.Exists(RootPath))
        {
            Directory.Delete(RootPath, recursive: true);
        }
    }
}