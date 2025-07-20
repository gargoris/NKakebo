
// ViewModel principal de la aplicación

using System;
using System.Reactive;
using KakeboApp.Core.Interfaces;
using KakeboApp.ViewModels;
using ReactiveUI;

public class MainWindowViewModel : ViewModelBase
{
    private readonly IDatabaseService _databaseService;
    private ViewModelBase _currentPage;
    private bool _isConnected;

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
        _currentPage = _databaseService.IsConnected ? TransactionsViewModel : ConnectionViewModel;

        // Comandos de navegación
        ShowTransactionsCommand = ReactiveCommand.Create(ShowTransactions);
        ShowBudgetCommand = ReactiveCommand.Create(ShowBudget);
        ShowReportsCommand = ReactiveCommand.Create(ShowReports);
        ShowConnectionCommand = ReactiveCommand.Create(ShowConnection);

        // Suscripción a eventos de conexión
        ConnectionViewModel.DatabaseConnected.Subscribe(_ => OnDatabaseConnected());

        // Estado inicial
        IsConnected = _databaseService.IsConnected;
    }

    public ViewModelBase CurrentPage
    {
        get => _currentPage;
        private set => this.RaiseAndSetIfChanged(ref _currentPage, value);
    }

    public bool IsConnected
    {
        get => _isConnected;
        private set => this.RaiseAndSetIfChanged(ref _isConnected, value);
    }

    // ViewModels de páginas
    public DatabaseConnectionViewModel ConnectionViewModel { get; }
    public TransactionsViewModel TransactionsViewModel { get; }
    public BudgetViewModel BudgetViewModel { get; }
    public ReportsViewModel ReportsViewModel { get; }

    // Comandos de navegación
    public ReactiveCommand<Unit, Unit> ShowTransactionsCommand { get; }
    public ReactiveCommand<Unit, Unit> ShowBudgetCommand { get; }
    public ReactiveCommand<Unit, Unit> ShowReportsCommand { get; }
    public ReactiveCommand<Unit, Unit> ShowConnectionCommand { get; }

    private void ShowTransactions() => CurrentPage = TransactionsViewModel;
    private void ShowBudget() => CurrentPage = BudgetViewModel;
    private void ShowReports() => CurrentPage = ReportsViewModel;
    private void ShowConnection() => CurrentPage = ConnectionViewModel;

    private void OnDatabaseConnected()
    {
        IsConnected = true;
        CurrentPage = TransactionsViewModel;

        // Actualizar datos en todos los ViewModels
        TransactionsViewModel.RefreshDataCommand.Execute().Subscribe();
        BudgetViewModel.RefreshDataCommand.Execute().Subscribe();
        ReportsViewModel.RefreshDataCommand.Execute().Subscribe();
    }
}
