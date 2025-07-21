using System;
using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Platform.Storage;
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

        // Comandos sin scheduler específico para evitar problemas de threading
        BrowseFileCommand = ReactiveCommand.CreateFromTask<UserControl>(BrowseFile);
        CreateNewCommand = ReactiveCommand.CreateFromTask<UserControl>(CreateNew);
        ConnectCommand = ReactiveCommand.CreateFromTask(Connect);

        // Configurar ruta inicial
        var defaultPath = System.IO.Path.Combine(_platformService.GetLocalDataPath(), "kakebo.db");
        DatabasePath = defaultPath;
    }

    public string DatabasePath
    {
        get => _databasePath;
        set
        {
            if (Avalonia.Threading.Dispatcher.UIThread.CheckAccess())
            {
                this.RaiseAndSetIfChanged(ref _databasePath, value);
            }
            else
            {
                Avalonia.Threading.Dispatcher.UIThread.Post(() => this.RaiseAndSetIfChanged(ref _databasePath, value));
            }
        }
    }

    public string? Password
    {
        get => _password;
        set
        {
            if (Avalonia.Threading.Dispatcher.UIThread.CheckAccess())
            {
                this.RaiseAndSetIfChanged(ref _password, value);
            }
            else
            {
                Avalonia.Threading.Dispatcher.UIThread.Post(() => this.RaiseAndSetIfChanged(ref _password, value));
            }
        }
    }

    public string? ErrorMessage
    {
        get => _errorMessage;
        set
        {
            if (Avalonia.Threading.Dispatcher.UIThread.CheckAccess())
            {
                this.RaiseAndSetIfChanged(ref _errorMessage, value);
            }
            else
            {
                Avalonia.Threading.Dispatcher.UIThread.Post(() => this.RaiseAndSetIfChanged(ref _errorMessage, value));
            }
        }
    }

    public bool IsConnecting
    {
        get => _isConnecting;
        set
        {
            if (Avalonia.Threading.Dispatcher.UIThread.CheckAccess())
            {
                this.RaiseAndSetIfChanged(ref _isConnecting, value);
            }
            else
            {
                Avalonia.Threading.Dispatcher.UIThread.Post(() => this.RaiseAndSetIfChanged(ref _isConnecting, value));
            }
        }
    }

    public ReactiveCommand<UserControl, Unit> BrowseFileCommand { get; }
    public ReactiveCommand<UserControl, Unit> CreateNewCommand { get; }
    public ReactiveCommand<Unit, Unit> ConnectCommand { get; }

    public IObservable<Unit> DatabaseConnected => _databaseConnected.AsObservable();

    private async Task BrowseFile(UserControl control)
    {
        try
        {
            var topLevel = TopLevel.GetTopLevel(control);
            if (topLevel == null) return;
            var files = await topLevel.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
            {
                Title = "Seleccionar Base de Datos",
                AllowMultiple = false,
                FileTypeFilter = new[]
                {
                    new FilePickerFileType("Base de Datos") { Patterns = new[] { "*.db", "*.sqlite", "*.sqlite3" } },
                    new FilePickerFileType("Todos los Archivos") { Patterns = new[] { "*.*" } }
                }
            });
            if (files.Count > 0)
            {
                DatabasePath = files[0].Path.LocalPath;
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Error al seleccionar archivo: {ex.Message}";
        }
    }

    private async Task CreateNew(UserControl control)
    {
        try
        {
            var topLevel = TopLevel.GetTopLevel(control);
            if (topLevel == null) return;
            var file = await topLevel.StorageProvider.SaveFilePickerAsync(new FilePickerSaveOptions
            {
                Title = "Crear Nueva Base de Datos",
                DefaultExtension = "db",
                FileTypeChoices = new[]
                {
                    new FilePickerFileType("Base de Datos") { Patterns = new[] { "*.db" } }
                }
            });
            if (file != null)
            {
                DatabasePath = file.Path.LocalPath;
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
