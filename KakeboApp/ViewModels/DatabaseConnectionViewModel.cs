using System;
using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading.Tasks;
using KakeboApp.Core.Interfaces;
using KakeboApp.Core.Models;
using ReactiveUI;

namespace KakeboApp.ViewModels;

public class DatabaseConnectionViewModel : ViewModelBase
{
    private readonly IDatabaseService _databaseService;
    private readonly IPlatformService _platformService;
    private readonly Subject<Unit> _databaseConnected = new();

    private string _databasePath = string.Empty;
    private string? _password;
    private string? _errorMessage;
    private bool _isConnecting;

    public DatabaseConnectionViewModel(IDatabaseService databaseService, IPlatformService platformService)
    {
        _databaseService = databaseService;
        _platformService = platformService;

        // Comandos
        BrowseFileCommand = ReactiveCommand.CreateFromTask(BrowseFile);
        CreateNewCommand = ReactiveCommand.CreateFromTask(CreateNew);
        ConnectCommand = ReactiveCommand.CreateFromTask(Connect);

        // Configurar ruta inicial
        var defaultPath = System.IO.Path.Combine(_platformService.GetLocalDataPath(), "kakebo.db");
        DatabasePath = defaultPath;
    }

    public string DatabasePath
    {
        get => _databasePath;
        set => this.RaiseAndSetIfChanged(ref _databasePath, value);
    }

    public string? Password
    {
        get => _password;
        set => this.RaiseAndSetIfChanged(ref _password, value);
    }

    public string? ErrorMessage
    {
        get => _errorMessage;
        set => this.RaiseAndSetIfChanged(ref _errorMessage, value);
    }

    public bool IsConnecting
    {
        get => _isConnecting;
        set => this.RaiseAndSetIfChanged(ref _isConnecting, value);
    }

    public ReactiveCommand<Unit, Unit> BrowseFileCommand { get; }
    public ReactiveCommand<Unit, Unit> CreateNewCommand { get; }
    public ReactiveCommand<Unit, Unit> ConnectCommand { get; }

    public IObservable<Unit> DatabaseConnected => _databaseConnected.AsObservable();

    private async Task BrowseFile()
    {
        try
        {
            var file = await _platformService.PickFileAsync("Seleccionar Base de Datos", "db", "sqlite", "sqlite3");
            if (file != null)
            {
                DatabasePath = file;
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Error al seleccionar archivo: {ex.Message}";
        }
    }

    private async Task CreateNew()
    {
        try
        {
            var file = await _platformService.SaveFileAsync("Crear Nueva Base de Datos", "kakebo.db", "db");
            if (file != null)
            {
                DatabasePath = file;
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Error al crear archivo: {ex.Message}";
        }
    }

    private async Task Connect()
    {
        IsConnecting = true;
        ErrorMessage = null;

        try
        {
            var config = new DatabaseConfig
            {
                FilePath = DatabasePath,
                Password = string.IsNullOrWhiteSpace(Password) ? null : Password
            };

            var result = await _databaseService.ConnectAsync(config);
            
            if (result.IsSuccess)
            {
                _databaseConnected.OnNext(Unit.Default);
            }
            else
            {
                ErrorMessage = result.GetError();
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Error de conexión: {ex.Message}";
        }
        finally
        {
            IsConnecting = false;
        }
    }
}
