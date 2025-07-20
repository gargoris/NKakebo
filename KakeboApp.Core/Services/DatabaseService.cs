using System;
using System.IO;
using System.Threading.Tasks;
using KakeboApp.Core.Interfaces;
using KakeboApp.Core.Models;
using KakeboApp.Core.Utils;

namespace KakeboApp.Core.Services;

// Implementación del servicio de configuración de base de datos
public class DatabaseService : IDatabaseService
{
    private readonly IKakeboDatabase _database;

    public DatabaseService(IKakeboDatabase database)
    {
        _database = database;
    }

    public bool IsConnected { get; private set; }
    public DatabaseConfig? CurrentConfig { get; private set; }

    public async Task<Result<Unit>> ConnectAsync(DatabaseConfig config)
    {
        var result = await _database.ConnectAsync(config);

        if (result.IsSuccess)
        {
            IsConnected = true;
            CurrentConfig = config;
        }

        return result;
    }

    public async Task<Result<bool>> TestConnectionAsync(DatabaseConfig config)
    {
        // Guardamos el estado actual
        var wasConnected = IsConnected;
        var previousConfig = CurrentConfig;

        try
        {
            // Intentamos conectar temporalmente
            var result = await _database.ConnectAsync(config);

            if (result.IsSuccess)
            {
                var testResult = await _database.TestConnectionAsync();

                // Restauramos el estado anterior si era diferente
                if (previousConfig != null && !previousConfig.Equals(config))
                {
                    await _database.ConnectAsync(previousConfig);
                    IsConnected = wasConnected;
                    CurrentConfig = previousConfig;
                }

                return testResult;
            }

            return new Result<bool>.Error(result.GetError());
        }
        catch (Exception ex)
        {
            // Restauramos el estado anterior en caso de excepción
            if (previousConfig != null)
            {
                await _database.ConnectAsync(previousConfig);
                IsConnected = wasConnected;
                CurrentConfig = previousConfig;
            }

            return new Result<bool>.Error($"Connection test failed: {ex.Message}");
        }
    }

    public async Task<Result<Unit>> CreateDatabaseAsync(string filePath, string? password)
    {
        try
        {
            // Verificamos que el directorio existe
            var directory = Path.GetDirectoryName(filePath);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            // Creamos la configuración para la nueva base de datos
            var config = new DatabaseConfig
            {
                FilePath = filePath,
                Password = password
            };

            // Intentamos conectar (esto creará el archivo si no existe)
            var result = await ConnectAsync(config);

            if (result.IsSuccess)
            {
                return new Result<Unit>.Success(Unit.Value);
            }

            return new Result<Unit>.Error(result.GetError());
        }
        catch (Exception ex)
        {
            return new Result<Unit>.Error($"Failed to create database: {ex.Message}");
        }
    }
}
