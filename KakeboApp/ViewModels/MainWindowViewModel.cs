// ViewModel principal de la aplicación

using System;
using System.Reactive;
using System.Linq;
using System.Threading.Tasks;
using System.Reactive.Linq;
using System.Windows.Input;
using Avalonia.Threading;
using KakeboApp.Core.Interfaces;
using KakeboApp.ViewModels;
using KakeboApp.Utils;
using KakeboApp.Commands;
using ReactiveUI;
using Serilog;

namespace KakeboApp.ViewModels;

public class MainWindowViewModel : ViewModelBase
{
    private readonly IDatabaseService _databaseService;

    public MainWindowViewModel(
        IDatabaseService databaseService,
        DatabaseConnectionViewModel connectionViewModel,
        TransactionsViewModel transactionsViewModel,
        BudgetViewModel budgetViewModel,
        ReportsViewModel reportsViewModel)
    {
        _databaseService = databaseService;

        // ViewModels de páginas
        ConnectionViewModel = connectionViewModel;
        TransactionsViewModel = transactionsViewModel;
        BudgetViewModel = budgetViewModel;
        ReportsViewModel = reportsViewModel;

        // Página inicial
        CurrentPage = _databaseService.IsConnected ? TransactionsViewModel : ConnectionViewModel;

        // Comandos de navegación usando AsyncCommand en lugar de ReactiveCommand para evitar threading issues
        ShowTransactionsCommand = new AsyncCommand(ShowTransactionsAsync);
        ShowBudgetCommand = new AsyncCommand(ShowBudgetAsync);
        ShowReportsCommand = new AsyncCommand(ShowReportsAsync);
        ShowConnectionCommand = new AsyncCommand(ShowConnectionAsync);

        // Estado inicial
        IsConnected = _databaseService.IsConnected;
        
        // ✅ AQUÍ ESTÁ LA CORRECCIÓN: Suscribirse al evento de conexión
        ConnectionViewModel.DatabaseConnected.Subscribe(_ => OnDatabaseConnected());
        
        // También suscribirse al evento del servicio para mayor seguridad
        // El evento ya viene del UI thread gracias al ThreadingHelper
        _databaseService.DatabaseConnected += OnDatabaseConnected;
    }

    public ViewModelBase CurrentPage { get; private set; }

    public bool IsConnected { get; private set; }

    // ViewModels de páginas
    public DatabaseConnectionViewModel ConnectionViewModel { get; }
    public TransactionsViewModel TransactionsViewModel { get; }
    public BudgetViewModel BudgetViewModel { get; }
    public ReportsViewModel ReportsViewModel { get; }

    // Comandos de navegación (usando ICommand en lugar de ReactiveCommand para evitar threading issues)
    public ICommand ShowTransactionsCommand { get; }
    public ICommand ShowBudgetCommand { get; }
    public ICommand ShowReportsCommand { get; }
    public ICommand ShowConnectionCommand { get; }

    private async Task ShowTransactionsAsync() => await Task.Run(() => CurrentPage = TransactionsViewModel);
    private async Task ShowBudgetAsync() => await Task.Run(() => CurrentPage = BudgetViewModel);
    private async Task ShowReportsAsync() => await Task.Run(() => CurrentPage = ReportsViewModel);
    private async Task ShowConnectionAsync() => await Task.Run(() => CurrentPage = ConnectionViewModel);

    private void OnDatabaseConnected()
    {
        try
        {
            IsConnected = true;
            
            // Navegar inmediatamente a transacciones
            CurrentPage = TransactionsViewModel;

            // TODO: Cargar datos de ViewModels de forma thread-safe después de solucionar problema ReactiveCommand
            // Por ahora omitimos la carga automática para evitar threading issues
            // Los ViewModels se cargarán cuando el usuario navegue a ellos
            
            Log.Information("Database connected successfully, navigated to transactions view");
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error during database connection handling");
        }
    }
}
