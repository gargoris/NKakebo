// ViewModel para reportes y análisis

using System;
using System.Collections.ObjectModel;
using System.Reactive;
using System.Threading.Tasks;
using KakeboApp.Core.Interfaces;
using KakeboApp.Core.Models;
using KakeboApp.Core.Services;
using KakeboApp.ViewModels;
using ReactiveUI;

namespace KakeboApp.ViewModels;

public class ReportsViewModel : ViewModelBase
{
    private readonly ITransactionService _transactionService;
    private readonly IBudgetService _budgetService;

    private int _currentYear = DateTime.Now.Year;
    private int _currentMonth = DateTime.Now.Month;
    private bool _showDetailedView;
    private BalanceInfo? _balanceInfo;

    public ReportsViewModel(ITransactionService transactionService, IBudgetService budgetService)
    {
        _transactionService = transactionService;
        _budgetService = budgetService;

        ExpensesByCategory = new ObservableCollection<ExpenseByCategory>();
        ExpensesBySubcategory = new ObservableCollection<ExpenseBySubcategory>();

        // Comandos
        PreviousMonthCommand = ReactiveCommand.CreateFromTask(PreviousMonth);
        NextMonthCommand = ReactiveCommand.CreateFromTask(NextMonth);
        ToggleViewCommand = ReactiveCommand.Create(ToggleView);
        RefreshDataCommand = ReactiveCommand.CreateFromTask(LoadData);

        // Cargar datos de forma asíncrona sin bloquear el constructor
        _ = Task.Run(LoadData);
    }

    public ObservableCollection<ExpenseByCategory> ExpensesByCategory { get; }
    public ObservableCollection<ExpenseBySubcategory> ExpensesBySubcategory { get; }

    public int CurrentYear
    {
        get => _currentYear;
        set => this.RaiseAndSetIfChanged(ref _currentYear, value);
    }

    public int CurrentMonth
    {
        get => _currentMonth;
        set => this.RaiseAndSetIfChanged(ref _currentMonth, value);
    }

    public bool ShowDetailedView
    {
        get => _showDetailedView;
        set => this.RaiseAndSetIfChanged(ref _showDetailedView, value);
    }

    public BalanceInfo? BalanceInfo
    {
        get => _balanceInfo;
        set => this.RaiseAndSetIfChanged(ref _balanceInfo, value);
    }

    public string MonthYearDisplay => $"{GetMonthName(CurrentMonth)} {CurrentYear}";
    public string ViewToggleText => ShowDetailedView ? "Vista por Categorías" : "Vista Detallada";

    public ReactiveCommand<Unit, Unit> PreviousMonthCommand { get; }
    public ReactiveCommand<Unit, Unit> NextMonthCommand { get; }
    public ReactiveCommand<Unit, Unit> ToggleViewCommand { get; }
    public ReactiveCommand<Unit, Unit> RefreshDataCommand { get; }

    private async Task LoadData()
    {
        IsBusy = true;
        try
        {
            var balanceTask = _transactionService.GetBalanceAsync(CurrentYear, CurrentMonth);
            var categoryTask = _transactionService.GetExpensesByCategoryAsync(CurrentYear, CurrentMonth);
            var subcategoryTask = _transactionService.GetExpensesBySubcategoryAsync(CurrentYear, CurrentMonth);

            await Task.WhenAll(balanceTask, categoryTask, subcategoryTask);

            BalanceInfo = await balanceTask;

            ExpensesByCategory.Clear();
            foreach (var expense in await categoryTask)
            {
                ExpensesByCategory.Add(expense);
            }

            ExpensesBySubcategory.Clear();
            foreach (var expense in await subcategoryTask)
            {
                ExpensesBySubcategory.Add(expense);
            }

            UpdateCalculatedProperties();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error loading reports: {ex.Message}");
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task PreviousMonth()
    {
        var (year, month) = DateHelpers.GetPreviousMonth(CurrentYear, CurrentMonth);
        CurrentYear = year;
        CurrentMonth = month;
        await LoadData();
    }

    private async Task NextMonth()
    {
        var (year, month) = DateHelpers.GetNextMonth(CurrentYear, CurrentMonth);
        CurrentYear = year;
        CurrentMonth = month;
        await LoadData();
    }

    private void ToggleView()
    {
        ShowDetailedView = !ShowDetailedView;
        this.RaisePropertyChanged(nameof(ViewToggleText));
    }

    private void UpdateCalculatedProperties()
    {
        this.RaisePropertyChanged(nameof(MonthYearDisplay));
    }

    private static string GetMonthName(int month) => month switch
    {
        1 => "Enero", 2 => "Febrero", 3 => "Marzo", 4 => "Abril",
        5 => "Mayo", 6 => "Junio", 7 => "Julio", 8 => "Agosto",
        9 => "Septiembre", 10 => "Octubre", 11 => "Noviembre", 12 => "Diciembre",
        _ => month.ToString()
    };
}
