using System;
using System.Threading.Tasks;
using KakeboApp.Core.Models;
using KakeboApp.Core.Utils;

namespace KakeboApp.Core.Interfaces;

/// <summary>
/// Servicio para gestionar la conexión y configuración de base de datos
/// </summary>
public interface IDatabaseService
{
    /// <summary>
    /// Indica si hay una conexión activa a la base de datos
    /// </summary>
    bool IsConnected { get; }

    /// <summary>
    /// Configuración actual de la base de datos (si está conectada)
    /// </summary>
    DatabaseConfig? CurrentConfig { get; }

    /// <summary>
    /// Evento que se dispara cuando se establece una conexión exitosa
    /// </summary>
    event Action? DatabaseConnected;

    /// <summary>
    /// Evento que se dispara cuando se desconecta de la base de datos
    /// </summary>
    event Action? DatabaseDisconnected;

    /// <summary>
    /// Conecta a una base de datos con la configuración especificada
    /// </summary>
    /// <param name="config">Configuración de conexión</param>
    /// <returns>Resultado de la operación</returns>
    Task<Result<Unit>> ConnectAsync(DatabaseConfig config);

    /// <summary>
    /// Prueba una conexión sin cambiar el estado actual
    /// </summary>
    /// <param name="config">Configuración de conexión a probar</param>
    /// <returns>True si la conexión es exitosa</returns>
    Task<Result<bool>> TestConnectionAsync(DatabaseConfig config);

    /// <summary>
    /// Crea una nueva base de datos en la ruta especificada
    /// </summary>
    /// <param name="filePath">Ruta del archivo de base de datos</param>
    /// <param name="password">Contraseña opcional</param>
    /// <returns>Resultado de la operación</returns>
    Task<Result<Unit>> CreateDatabaseAsync(string filePath, string? password);

    /// <summary>
    /// Desconecta de la base de datos actual
    /// </summary>
    /// <returns>Resultado de la operación</returns>
    Task<Result<Unit>> DisconnectAsync();
}