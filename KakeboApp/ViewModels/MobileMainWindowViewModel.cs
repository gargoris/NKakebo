using System;
using System.IO;
using ReactiveUI;
using System.Reactive;
using KakeboApp.Core.Interfaces;
using KakeboApp.Core.Models;
using Serilog;

namespace KakeboApp.ViewModels;

public class MobileMainWindowViewModel : ViewModelBase
{
    private readonly IPlatformService _platformService;
    private ViewModelBase _currentPage;
    private bool _isSidebarVisible;
    private string _pageTitle = "Kakebo";

    public MobileMainWindowViewModel(
        IPlatformService platformService,
        IDatabaseService databaseService,
        TransactionsViewModel transactionsViewModel,
        BudgetViewModel budgetViewModel,
        ReportsViewModel reportsViewModel)
    {
        _platformService = platformService;

        TransactionsViewModel = transactionsViewModel;
        BudgetViewModel = budgetViewModel;
        ReportsViewModel = reportsViewModel;

        // En móvil, iniciamos directamente en transacciones si hay DB
        _currentPage = TransactionsViewModel;

        // Comandos
        ToggleSidebarCommand = ReactiveCommand.Create(ToggleSidebar);
        ShowTransactionsCommand = ReactiveCommand.Create(() => NavigateTo(TransactionsViewModel, "Transacciones"));
        ShowBudgetCommand = ReactiveCommand.Create(() => NavigateTo(BudgetViewModel, "Presupuesto"));
        ShowReportsCommand = ReactiveCommand.Create(() => NavigateTo(ReportsViewModel, "Reportes"));

        // Auto-inicializar base de datos en móvil
        if (_platformService.IsMobile)
        {
            InitializeMobileDatabase();
        }
    }

    public ViewModelBase CurrentPage
    {
        get => _currentPage;
        private set => this.RaiseAndSetIfChanged(ref _currentPage, value);
    }

    public bool IsSidebarVisible
    {
        get => _isSidebarVisible;
        set => this.RaiseAndSetIfChanged(ref _isSidebarVisible, value);
    }

    public string PageTitle
    {
        get => _pageTitle;
        set => this.RaiseAndSetIfChanged(ref _pageTitle, value);
    }

    public bool IsMobile => _platformService.IsMobile;

    public TransactionsViewModel TransactionsViewModel { get; }
    public BudgetViewModel BudgetViewModel { get; }
    public ReportsViewModel ReportsViewModel { get; }

    public ReactiveCommand<Unit, Unit> ToggleSidebarCommand { get; }
    public ReactiveCommand<Unit, Unit> ShowTransactionsCommand { get; }
    public ReactiveCommand<Unit, Unit> ShowBudgetCommand { get; }
    public ReactiveCommand<Unit, Unit> ShowReportsCommand { get; }

    private void ToggleSidebar() => IsSidebarVisible = !IsSidebarVisible;

    private void NavigateTo(ViewModelBase page, string title)
    {
        CurrentPage = page;
        PageTitle = title;
        if (IsMobile) IsSidebarVisible = false;
    }

    private void InitializeMobileDatabase()
    {
        try
        {
            var dbPath = _platformService.GetLocalDataPath();
            Directory.CreateDirectory(dbPath);

            var config = new DatabaseConfig
            {
                FilePath = Path.Combine(dbPath, "kakebo.db")
            };

            // Conectar automáticamente en móvil
            // await _databaseService.ConnectAsync(config);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error initializing mobile database");
        }
    }
}
