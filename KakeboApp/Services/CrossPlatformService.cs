using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Platform.Storage;
using Avalonia.Controls.ApplicationLifetimes;
using KakeboApp.Core.Interfaces;
using System.Runtime.InteropServices;

namespace KakeboApp.Services;

public class CrossPlatformService : IPlatformService
{
    public bool IsMobile => false; // Desktop siempre es false

    public async Task<string?> PickFileAsync(string title, params string[] extensions)
    {
        if (Application.Current?.ApplicationLifetime is not IClassicDesktopStyleApplicationLifetime desktop)
            return null;

        var mainWindow = desktop.MainWindow;
        if (mainWindow?.StorageProvider is not { } storageProvider)
            return null;

        // Construir filtros de tipo de archivo
        var fileTypeFilters = extensions.Length > 0
            ? new[] 
            { 
                new FilePickerFileType("Archivos de Base de Datos") 
                { 
                    Patterns = extensions.Select(ext => ext.StartsWith("*.") ? ext : $"*.{ext}").ToArray(),
                    AppleUniformTypeIdentifiers = new[] { "public.database" },
                    MimeTypes = new[] { "application/octet-stream", "application/x-sqlite3" }
                } 
            }
            : new[] 
            { 
                new FilePickerFileType("Todos los archivos") 
                { 
                    Patterns = new[] { "*.*" } 
                } 
            };

        var files = await storageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
        {
            Title = title,
            FileTypeFilter = fileTypeFilters,
            AllowMultiple = false
        });

        return files.FirstOrDefault()?.Path.LocalPath;
    }

    public async Task<string?> SaveFileAsync(string title, string defaultName, params string[] extensions)
    {
        if (Application.Current?.ApplicationLifetime is not IClassicDesktopStyleApplicationLifetime desktop)
            return null;

        var mainWindow = desktop.MainWindow;
        if (mainWindow?.StorageProvider is not { } storageProvider)
            return null;

        // Determinar extensión predeterminada
        var defaultExtension = extensions.Length > 0 
            ? extensions[0].TrimStart('*', '.') 
            : "db";

        var fileTypeChoices = extensions.Length > 0
            ? new[]
            {
                new FilePickerFileType("Archivos de Base de Datos")
                {
                    Patterns = extensions.Select(ext => ext.StartsWith("*.") ? ext : $"*.{ext}").ToArray()
                }
            }
            : null;

        var file = await storageProvider.SaveFilePickerAsync(new FilePickerSaveOptions
        {
            Title = title,
            SuggestedFileName = defaultName,
            DefaultExtension = defaultExtension,
            FileTypeChoices = fileTypeChoices
        });

        return file?.Path.LocalPath;
    }

    public string GetLocalDataPath()
    {
        // Usar rutas específicas de cada sistema operativo
        var appName = "KakeboApp";
        
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            // Windows: %APPDATA%\KakeboApp
            return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), appName);
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            // Linux: ~/.local/share/KakeboApp
            var homeDir = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            return Path.Combine(homeDir, ".local", "share", appName);
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            // macOS: ~/Library/Application Support/KakeboApp
            var homeDir = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            return Path.Combine(homeDir, "Library", "Application Support", appName);
        }
        else
        {
            // Fallback genérico
            return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), appName);
        }
    }
}
