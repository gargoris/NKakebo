// ViewModel para reportes y análisis

using System;
using System.Collections.ObjectModel;
using System.Reactive;
using System.Threading.Tasks;
using System.Windows.Input;
using Avalonia.Threading;
using KakeboApp.Core.Interfaces;
using KakeboApp.Core.Models;
using KakeboApp.Core.Services;
using KakeboApp.ViewModels;
using KakeboApp.Commands;
using ReactiveUI;
using Serilog;

namespace KakeboApp.ViewModels;

public class ReportsViewModel : ViewModelBase
{
    private readonly ITransactionService _transactionService;
    private readonly IBudgetService _budgetService;

    public ReportsViewModel(ITransactionService transactionService, IBudgetService budgetService)
    {
        _transactionService = transactionService;
        _budgetService = budgetService;

        ExpensesByCategory = new ObservableCollection<ExpenseByCategory>();
        ExpensesBySubcategory = new ObservableCollection<ExpenseBySubcategory>();

        // Comandos - usando AsyncCommand para navegación de mes para evitar threading issues
        PreviousMonthCommand = new AsyncCommand(PreviousMonth);
        NextMonthCommand = new AsyncCommand(NextMonth);
        ToggleViewCommand = ReactiveCommand.Create(ToggleView);
        RefreshDataCommand = ReactiveCommand.CreateFromTask(LoadData);

        // Cargar datos iniciales de forma asíncrona
        // _ = Task.Run(LoadData);
    }

    public ObservableCollection<ExpenseByCategory> ExpensesByCategory { get; }
    public ObservableCollection<ExpenseBySubcategory> ExpensesBySubcategory { get; }

    public int CurrentYear { get; set; } = DateTime.Now.Year;

    public int CurrentMonth { get; set; } = DateTime.Now.Month;

    public bool ShowDetailedView { get; set; }

    public BalanceInfo? BalanceInfo { get; set; }

    public string MonthYearDisplay => $"{GetMonthName(CurrentMonth)} {CurrentYear}";
    public string ViewToggleText => ShowDetailedView ? "Vista por Categorías" : "Vista Detallada";

    public ICommand PreviousMonthCommand { get; }
    public ICommand NextMonthCommand { get; }
    public ReactiveCommand<Unit, Unit> ToggleViewCommand { get; }
    public ReactiveCommand<Unit, Unit> RefreshDataCommand { get; }

    private async Task LoadData()
    {
        await Avalonia.Threading.Dispatcher.UIThread.InvokeAsync(() => IsBusy = true);
        try
        {
            var balanceTask = _transactionService.GetBalanceAsync(CurrentYear, CurrentMonth);
            var categoryTask = _transactionService.GetExpensesByCategoryAsync(CurrentYear, CurrentMonth);
            var subcategoryTask = _transactionService.GetExpensesBySubcategoryAsync(CurrentYear, CurrentMonth);

            await Task.WhenAll(balanceTask, categoryTask, subcategoryTask);

            var balance = await balanceTask;
            var categories = await categoryTask;
            var subcategories = await subcategoryTask;

            await Avalonia.Threading.Dispatcher.UIThread.InvokeAsync(() =>
            {
                BalanceInfo = balance;
                ExpensesByCategory.Clear();
                foreach (var expense in categories)
                    ExpensesByCategory.Add(expense);
                ExpensesBySubcategory.Clear();
                foreach (var expense in subcategories)
                    ExpensesBySubcategory.Add(expense);
                UpdateCalculatedProperties();
            });
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error loading reports");
        }
        finally
        {
            await Avalonia.Threading.Dispatcher.UIThread.InvokeAsync(() => IsBusy = false);
        }
    }

    private async Task PreviousMonth()
    {
        var (year, month) = DateHelpers.GetPreviousMonth(CurrentYear, CurrentMonth);
        
        // Asegurar que los cambios se hagan en el UI thread
        if (Dispatcher.UIThread.CheckAccess())
        {
            CurrentYear = year;
            CurrentMonth = month;
            await LoadData();
        }
        else
        {
            await Dispatcher.UIThread.InvokeAsync(async () =>
            {
                CurrentYear = year;
                CurrentMonth = month;
                await LoadData();
            });
        }
    }

    private async Task NextMonth()
    {
        var (year, month) = DateHelpers.GetNextMonth(CurrentYear, CurrentMonth);
        
        // Asegurar que los cambios se hagan en el UI thread
        if (Dispatcher.UIThread.CheckAccess())
        {
            CurrentYear = year;
            CurrentMonth = month;
            await LoadData();
        }
        else
        {
            await Dispatcher.UIThread.InvokeAsync(async () =>
            {
                CurrentYear = year;
                CurrentMonth = month;
                await LoadData();
            });
        }
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
