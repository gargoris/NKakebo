using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Platform.Storage;
using Avalonia.Controls.ApplicationLifetimes;
using KakeboApp.Core.Interfaces;

namespace KakeboApp.Services;

public class DesktopPlatformService : IPlatformService
{
    public bool IsMobile => false;

    public async Task<string?> PickFileAsync(string title, params string[] extensions)
    {
        if (Application.Current?.ApplicationLifetime is not IClassicDesktopStyleApplicationLifetime desktop)
            return null;

        // Build file type filters from extensions
        var fileTypeFilters = extensions.Length > 0
            ? new[] { new FilePickerFileType("Archivos") { Patterns = extensions.Select(ext => ext.StartsWith("*.") ? ext : $"*.{ext}").ToArray() } }
            : new[] { new FilePickerFileType("Todos los archivos") { Patterns = new[] { "*.*" } } };

        var files = await desktop.MainWindow!.StorageProvider.OpenFilePickerAsync(
            new FilePickerOpenOptions
            {
                Title = title,
                FileTypeFilter = fileTypeFilters
            });

        return files.FirstOrDefault()?.Path.LocalPath;
    }

    public async Task<string?> SaveFileAsync(string title, string defaultName, params string[] extensions)
    {
        if (Application.Current?.ApplicationLifetime is not IClassicDesktopStyleApplicationLifetime desktop)
            return null;

        // Determine default extension from extensions array or fallback to "db"
        var defaultExtension = extensions.Length > 0 
            ? extensions[0].TrimStart('*', '.') 
            : "db";

        var file = await desktop.MainWindow!.StorageProvider.SaveFilePickerAsync(
            new FilePickerSaveOptions
            {
                Title = title,
                SuggestedFileName = defaultName,
                DefaultExtension = defaultExtension
            });

        return file?.Path.LocalPath;
    }

    public string GetLocalDataPath() =>
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "KakeboApp");
}
