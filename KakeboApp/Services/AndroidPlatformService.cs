#if ANDROID
using Android.Content;
using KakeboApp.Core.Interfaces;

namespace KakeboApp.Services;

public class AndroidPlatformService : IPlatformService
{
    public bool IsMobile => true;

    public Task<string?> PickFileAsync()
    {
        // En Android, usar archivo fijo en directorio interno
        var dbPath = Path.Combine(GetDatabasePath(), "kakebo.db");
        return Task.FromResult<string?>(dbPath);
    }

    public Task<string?> SaveFileAsync(string defaultName)
    {
        var dbPath = Path.Combine(GetDatabasePath(), defaultName);
        return Task.FromResult<string?>(dbPath);
    }

    public string GetDatabasePath()
    {
        var context = Platform.CurrentActivity ?? global::Android.App.Application.Context;
        var path = context?.FilesDir?.AbsolutePath ?? "/data/data/com.kakeboapp.manager/files";
        Directory.CreateDirectory(path);
        return path;
    }
}
#endif