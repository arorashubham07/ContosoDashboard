namespace ContosoDashboard.Services;

public sealed class LocalFileStorageService(IConfiguration configuration) : IFileStorageService
{
    private readonly string rootPath = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, configuration["DocumentStorage:RootPath"] ?? "../App_Data/ContosoDocuments"));
    public string CreateFinalPath(string extension) => Path.Combine("accepted", $"{Guid.NewGuid():N}{extension.ToLowerInvariant()}");
    public async Task<string> StageAsync(Stream content, CancellationToken cancellationToken = default) { var path = Path.Combine("staging", $"{Guid.NewGuid():N}.tmp"); var fullPath = Resolve(path); Directory.CreateDirectory(Path.GetDirectoryName(fullPath)!); await using var output = File.Create(fullPath); await content.CopyToAsync(output, cancellationToken); return path; }
    public Task MoveToFinalAsync(string stagingPath, string finalPath, CancellationToken cancellationToken = default) { var final = Resolve(finalPath); Directory.CreateDirectory(Path.GetDirectoryName(final)!); File.Move(Resolve(stagingPath), final); return Task.CompletedTask; }
    public Task<Stream> OpenReadAsync(string path, CancellationToken cancellationToken = default) => Task.FromResult<Stream>(File.OpenRead(Resolve(path)));
    public Task DeleteAsync(string path, CancellationToken cancellationToken = default) { var fullPath = Resolve(path); if (File.Exists(fullPath)) File.Delete(fullPath); return Task.CompletedTask; }
    public string GetFullPath(string path) => Resolve(path);
    private string Resolve(string path) { var fullPath = Path.GetFullPath(Path.Combine(rootPath, path)); if (!fullPath.StartsWith(rootPath + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase)) throw new InvalidOperationException("Invalid private storage path."); return fullPath; }
}