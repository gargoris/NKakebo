using System.Threading.Tasks;
using KakeboApp.Core.Models;
using KakeboApp.Core.Utils;

namespace KakeboApp.Core.Interfaces;

// Servicio de configuración de base de datos
public interface IDatabaseService
{
    Task<Result<Unit>> ConnectAsync(DatabaseConfig config);
    Task<Result<bool>> TestConnectionAsync(DatabaseConfig config);
    Task<Result<Unit>> CreateDatabaseAsync(string filePath, string? password);
    bool IsConnected { get; }
    DatabaseConfig? CurrentConfig { get; }
}
