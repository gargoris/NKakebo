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
using ReactiveUI.Fody.Helpers;
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
        
        // ✅ AQUÍ ESTÁ LA CORRECCIÓN: Suscribirse al evento de conexión con UI thread
        ConnectionViewModel.DatabaseConnected
            .ObserveOn(RxApp.MainThreadScheduler)
            .Subscribe(_ => OnDatabaseConnected());
        
        // También suscribirse al evento del servicio para mayor seguridad
        // El evento ya viene del UI thread gracias al ThreadingHelper
        _databaseService.DatabaseConnected += OnDatabaseConnected;
    }

    [Reactive] public ViewModelBase CurrentPage { get; set; } = null!;
    [Reactive] public bool IsConnected { get; set; }

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
    private async Task ShowBudgetAsync()
    {
        // Cargar datos del presupuesto antes de mostrar la pantalla
        await BudgetViewModel.LoadData();
        CurrentPage = BudgetViewModel;
    }
    private async Task ShowReportsAsync() => await Task.Run(() => CurrentPage = ReportsViewModel);
    private async Task ShowConnectionAsync() => await Task.Run(() => CurrentPage = ConnectionViewModel);

    private void OnDatabaseConnected()
    {
        try
        {
            // Asegurar que los cambios se hagan en el UI thread
            if (Avalonia.Threading.Dispatcher.UIThread.CheckAccess())
            {
                IsConnected = true;
                CurrentPage = TransactionsViewModel;
            }
            else
            {
                Avalonia.Threading.Dispatcher.UIThread.Post(() =>
                {
                    IsConnected = true;
                    CurrentPage = TransactionsViewModel;
                });
            }
            
            Log.Information("Database connected successfully, navigated to transactions view");
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error during database connection handling");
        }
    }
}
