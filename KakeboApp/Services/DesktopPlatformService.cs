using Avalonia.Platform.Storage;
using Avalonia.Controls.ApplicationLifetimes;
using KakeboApp.Core.Interfaces;

namespace KakeboApp.Services;

public class DesktopPlatformService : IPlatformService
{
    public bool IsMobile => false;

    public async Task<string?> PickFileAsync()
    {
        if (Application.Current?.ApplicationLifetime is not IClassicDesktopStyleApplicationLifetime desktop)
            return null;

        var files = await desktop.MainWindow!.StorageProvider.OpenFilePickerAsync(
            new FilePickerOpenOptions
            {
                Title = "Seleccionar Base de Datos",
                FileTypeFilter = new[]
                {
                    new FilePickerFileType("Base de Datos") { Patterns = new[] { "*.db" } }
                }
            });

        return files.FirstOrDefault()?.Path.LocalPath;
    }

    public async Task<string?> SaveFileAsync(string defaultName)
    {
        if (Application.Current?.ApplicationLifetime is not IClassicDesktopStyleApplicationLifetime desktop)
            return null;

        var file = await desktop.MainWindow!.StorageProvider.SaveFilePickerAsync(
            new FilePickerSaveOptions
            {
                Title = "Crear Base de Datos",
                SuggestedFileName = defaultName,
                DefaultExtension = "db"
            });

        return file?.Path.LocalPath;
    }

    public string GetDatabasePath() => 
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "KakeboApp");
}