
// ViewModel para configuración de conexión a base de datos
public class DatabaseConnectionViewModel : ViewModelBase
{
    private readonly IDatabaseService _databaseService;
    private string _databasePath = string.Empty;
    private string? _password;
    private bool _isConnecting;
    private string? _errorMessage;
    private readonly Subject<Unit> _databaseConnected = new();
    
    public DatabaseConnectionViewModel(IDatabaseService databaseService)
    {
        _databaseService = databaseService;
        
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
        // En una implementación real, aquí iría el diálogo de archivos
        // Para este ejemplo, simulamos la selección
        await Task.Delay(100);
        
        // Placeholder - en Avalonia se usaría StorageProvider
        // DatabasePath = selectedPath;
    }
    
    private async Task CreateNewDatabase()
    {
        // En una implementación real, aquí iría el diálogo para guardar archivo
        await Task.Delay(100);
        
        // Placeholder - crear nueva base de datos
        // var result = await _databaseService.CreateDatabaseAsync(newPath, Password);
    }
}