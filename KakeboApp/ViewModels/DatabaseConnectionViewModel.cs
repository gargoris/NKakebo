using System;
using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading.Tasks;
using System.Windows.Input;
using Avalonia.Controls;
using Avalonia.Platform.Storage;
using KakeboApp.Commands;
using KakeboApp.Core.Interfaces;
using KakeboApp.Core.Models;
using ReactiveUI;

namespace KakeboApp.ViewModels;

public class DatabaseConnectionViewModel : ViewModelBase
{
    private readonly IDatabaseService _databaseService;
    private readonly IPlatformService _platformService;
    private readonly Subject<Unit> _databaseConnected = new();

    public DatabaseConnectionViewModel(IDatabaseService databaseService, IPlatformService platformService)
    {
        _databaseService = databaseService;
        _platformService = platformService;

        // Comandos usando ICommand simple para evitar problemas de threading con ReactiveCommand
        BrowseFileCommand = new AsyncCommand<UserControl>(BrowseFile, _ => !IsConnecting);
        CreateNewCommand = new AsyncCommand<UserControl>(CreateNew, _ => !IsConnecting);
        ConnectCommand = new AsyncCommand(Connect, () => !IsConnecting);

        // Configurar ruta inicial
        var defaultPath = System.IO.Path.Combine(_platformService.GetLocalDataPath(), "kakebo.db");
        DatabasePath = defaultPath;
        
        // Observar cambios en IsConnecting para actualizar comandos
        this.WhenAnyValue(x => x.IsConnecting)
            .Subscribe(_ => NotifyCanExecuteChanged());
    }

    public string DatabasePath { get; set; } = string.Empty;

    public string? Password { get; set; }

    public new string? ErrorMessage { get; set; }

    public bool IsConnecting { get; set; }

    public ICommand BrowseFileCommand { get; }
    public ICommand CreateNewCommand { get; }
    public ICommand ConnectCommand { get; }

    public IObservable<Unit> DatabaseConnected => _databaseConnected.AsObservable();

    private void NotifyCanExecuteChanged()
    {
        (BrowseFileCommand as AsyncCommand<UserControl>)?.RaiseCanExecuteChanged();
        (CreateNewCommand as AsyncCommand<UserControl>)?.RaiseCanExecuteChanged();
        (ConnectCommand as AsyncCommand)?.RaiseCanExecuteChanged();
    }

    private async Task BrowseFile(UserControl? control)
    {
        if (control == null) return;
        
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

    private async Task CreateNew(UserControl? control)
    {
        if (control == null) return;
        
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
                // Disparar evento en el hilo de UI para evitar errores de threading
                Avalonia.Threading.Dispatcher.UIThread.Post(() =>
                {
                    _databaseConnected.OnNext(Unit.Default);
                });
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
