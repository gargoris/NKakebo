using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Platform.Storage;
using KakeboApp.Core.Interfaces;

namespace KakeboApp.Platforms.Windows;

public class WindowsPlatformService : IPlatformService
{
    public bool IsMobile => false;

    public async Task<string?> PickFileAsync(string title, params string[] extensions)
    {
        var mainWindow = GetMainWindow();
        if (mainWindow?.StorageProvider is not { } provider) return null;

        var files = await provider.OpenFilePickerAsync(new FilePickerOpenOptions
        {
            Title = title,
            AllowMultiple = false,
            FileTypeFilter = extensions.Select(ext => new FilePickerFileType("Database")
            {
                Patterns = new[] { $"*.{ext}" }
            }).ToArray()
        });

        return files.FirstOrDefault()?.Path.LocalPath;
    }

    public async Task<string?> SaveFileAsync(string title, string defaultName, params string[] extensions)
    {
        var mainWindow = GetMainWindow();
        if (mainWindow?.StorageProvider is not { } provider) return null;

        var file = await provider.SaveFilePickerAsync(new FilePickerSaveOptions
        {
            Title = title,
            SuggestedFileName = defaultName,
            DefaultExtension = extensions.FirstOrDefault(),
            FileTypeChoices = extensions.Select(ext => new FilePickerFileType("Database")
            {
                Patterns = new[] { $"*.{ext}" }
            }).ToArray()
        });

        return file?.Path.LocalPath;
    }

    public string GetLocalDataPath() =>
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "KakeboApp");

    private static Window? GetMainWindow()
    {
        if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            return desktop.MainWindow;
        }
        return null;
    }
}
