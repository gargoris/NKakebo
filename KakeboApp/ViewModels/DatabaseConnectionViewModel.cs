// ViewModel para configuración de conexión a base de datos

using System;
using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading.Tasks;
using KakeboApp.Core.Interfaces;
using KakeboApp.Core.Models;
using KakeboApp.ViewModels;
using ReactiveUI;
using CoreUnit = KakeboApp.Core.Utils.Unit;

public class DatabaseConnectionViewModel : ViewModelBase
{
    private readonly IDatabaseService _databaseService;
    private readonly IPlatformService _platformService;
    private string _databasePath = string.Empty;
    private string? _password;
    private bool _isConnecting;
    private string? _errorMessage;
    private readonly Subject<Unit> _databaseConnected = new();

    public DatabaseConnectionViewModel(IDatabaseService databaseService, IPlatformService platformService)
    {
        _databaseService = databaseService;
        _platformService = platformService;

        // Comandos con validación reactiva
        var canConnect = this.WhenAnyValue(
            x => x.DatabasePath,
            x => x.IsConnecting,
            (path, connecting) => !string.IsNullOrWhiteSpace(path) && !connecting);

        ConnectCommand = ReactiveCommand.CreateFromTask(ConnectToDatabase, canConnect);

        var canBrowse = this.WhenAnyValue(x => x.IsConnecting, connecting => !connecting);
        BrowseFileCommand = ReactiveCommand.CreateFromTask(BrowseForFile, canBrowse);

        var canCreateNew = this.WhenAnyValue(x => x.IsConnecting, connecting => !connecting);
        CreateNewCommand = ReactiveCommand.CreateFromTask(CreateNewDatabase, canCreateNew);

        // Limpiar error cuando cambia la ruta
        this.WhenAnyValue(x => x.DatabasePath)
            .Subscribe(_ => ErrorMessage = null);
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

    public bool IsConnecting
    {
        get => _isConnecting;
        set => this.RaiseAndSetIfChanged(ref _isConnecting, value);
    }

    public string? ErrorMessage
    {
        get => _errorMessage;
        set => this.RaiseAndSetIfChanged(ref _errorMessage, value);
    }

    public ReactiveCommand<Unit, Unit> ConnectCommand { get; }
    public ReactiveCommand<Unit, Unit> BrowseFileCommand { get; }
    public ReactiveCommand<Unit, Unit> CreateNewCommand { get; }

    public IObservable<Unit> DatabaseConnected => _databaseConnected.AsObservable();

    private async Task ConnectToDatabase()
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
            ErrorMessage = $"Connection failed: {ex.Message}";
        }
        finally
        {
            IsConnecting = false;
        }
    }

    private async Task BrowseForFile()
    {
        try
        {
            var selectedPath = await _platformService.PickFileAsync("Seleccionar Base de Datos", "db");
            if (!string.IsNullOrEmpty(selectedPath))
            {
                DatabasePath = selectedPath;
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Error al seleccionar archivo: {ex.Message}";
        }
    }

    private async Task CreateNewDatabase()
    {
        try
        {
            var selectedPath = await _platformService.SaveFileAsync("Crear Nueva Base de Datos", "kakebo.db", "db");
            if (!string.IsNullOrEmpty(selectedPath))
            {
                var result = await _databaseService.CreateDatabaseAsync(selectedPath, Password);
                if (result.IsSuccess)
                {
                    DatabasePath = selectedPath;
                }
                else
                {
                    ErrorMessage = result.GetError();
                }
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Error al crear base de datos: {ex.Message}";
        }
    }
}
