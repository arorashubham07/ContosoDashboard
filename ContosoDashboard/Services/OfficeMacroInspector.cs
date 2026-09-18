using System.IO.Compression;

namespace ContosoDashboard.Services;

public sealed class OfficeMacroInspector : IOfficeMacroInspector
{
    public Task<MacroInspectionResult> InspectAsync(string path, string extension, CancellationToken cancellationToken = default)
    {
        try { if (extension is not ".doc" and not ".docx" and not ".xls" and not ".xlsx" and not ".ppt" and not ".pptx") return Task.FromResult(new MacroInspectionResult(true)); if (extension is ".doc" or ".xls" or ".ppt") return Task.FromResult(new MacroInspectionResult(false, "Legacy Office inspection is unavailable.")); using var archive = ZipFile.OpenRead(path); return Task.FromResult(archive.Entries.Any(entry => entry.FullName.Contains("vbaProject", StringComparison.OrdinalIgnoreCase)) ? new MacroInspectionResult(false, "Office macros are not allowed.") : new MacroInspectionResult(true)); } catch { return Task.FromResult(new MacroInspectionResult(false, "Office content could not be inspected.")); }
    }
}