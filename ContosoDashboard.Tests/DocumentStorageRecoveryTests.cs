using ContosoDashboard.Services;
using Microsoft.Extensions.Configuration;

namespace ContosoDashboard.Tests;

public sealed class DocumentStorageRecoveryTests : IDisposable
{
    private readonly string rootPath = Path.Combine(Path.GetTempPath(), "ContosoDashboard.StorageTests", Guid.NewGuid().ToString("N"));

    [Fact]
    public async Task StageAndMoveUsesPrivateGuidPaths()
    {
        var storage = CreateStorage();
        await using var content = new MemoryStream("document content"u8.ToArray());

        var stagingPath = await storage.StageAsync(content);
        var finalPath = storage.CreateFinalPath(".txt");
        await storage.MoveToFinalAsync(stagingPath, finalPath);

        await using var result = await storage.OpenReadAsync(finalPath);
        using var reader = new StreamReader(result);
        Assert.Equal("document content", await reader.ReadToEndAsync());
        Assert.StartsWith("accepted", finalPath, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void TraversalPathsAreRejected()
    {
        var storage = CreateStorage();
        Assert.Throws<InvalidOperationException>(() => storage.GetFullPath("../outside.txt"));
    }

    public void Dispose()
    {
        if (Directory.Exists(rootPath)) Directory.Delete(rootPath, recursive: true);
    }

    private LocalFileStorageService CreateStorage() => new(new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?> { ["DocumentStorage:RootPath"] = rootPath }).Build());
}