// Services/IPlatformService.cs - Abstracción para funcionalidades específicas de plataforma
namespace KakeboApp.Core.Interfaces;

public interface IPlatformService
{
    Task<string?> PickFileAsync(string title, params string[] extensions);
    Task<string?> SaveFileAsync(string title, string defaultName, params string[] extensions);
    string GetLocalDataPath();
    bool IsMobile { get; }
}
