#if ANDROID
using System.IO;
using System.Threading.Tasks;
using Android.Content;
using KakeboApp.Core.Interfaces;

namespace KakeboApp.Services;

public class AndroidPlatformService : IPlatformService
{
    public bool IsMobile => true;

    public Task<string?> PickFileAsync(string title, params string[] extensions)
    {
        // En Android, usar archivo fijo en directorio interno
        var dbPath = Path.Combine(GetLocalDataPath(), "kakebo.db");
        return Task.FromResult<string?>(dbPath);
    }

    public Task<string?> SaveFileAsync(string title, string defaultName, params string[] extensions)
    {
        var dbPath = Path.Combine(GetLocalDataPath(), defaultName);
        return Task.FromResult<string?>(dbPath);
    }

    public string GetLocalDataPath()
    {
        var context = Platform.CurrentActivity ?? global::Android.App.Application.Context;
        var path = context?.FilesDir?.AbsolutePath ?? "/data/data/com.kakeboapp.manager/files";
        Directory.CreateDirectory(path);
        return path;
    }
}
#endif
