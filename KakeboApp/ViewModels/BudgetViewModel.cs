using System;
using ReactiveUI;
using System.Collections.ObjectModel;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Avalonia.Threading;
using KakeboApp.Core.Interfaces;
using KakeboApp.Core.Models;
using KakeboApp.Core.Services;
using KakeboApp.Core.Utils;
using KakeboApp.Commands;
using Unit = System.Reactive.Unit;
using Serilog;
using ReactiveUI.Fody.Helpers;

namespace KakeboApp.ViewModels;

// ViewModel para presupuesto Kakebo
public partial class BudgetViewModel : ViewModelBase
{
    private readonly IBudgetService _budgetService;
    private readonly ITransactionService _transactionService;

    public BudgetViewModel(IBudgetService budgetService, ITransactionService transactionService)
    {
        _budgetService = budgetService;
        _transactionService = transactionService;

        // Comandos de navegación usando AsyncCommand para evitar threading issues
        PreviousMonthCommand = new AsyncCommand(PreviousMonth);
        NextMonthCommand = new AsyncCommand(NextMonth);

        // Comandos de presupuesto - usando Observable.CombineLatest que funciona mejor con Fody
        var canSaveObservable = Observable.CombineLatest(
            this.WhenAnyValue(x => x.PlannedIncome),
            this.WhenAnyValue(x => x.IsBusy),
            (income, busy) => 
            {
                var canExecute = income > 0 && !busy;
                Log.Debug("SaveBudget CanExecute: PlannedIncome={Income}, IsBusy={IsBusy}, CanSave={CanSave}", 
                    income, busy, canExecute);
                return canExecute;
            })
            .StartWith(false); // Comenzar deshabilitado hasta que se evalúe

        SaveBudgetCommand = ReactiveCommand.CreateFromTask(
            SaveBudget,
            canSaveObservable,
            RxApp.MainThreadScheduler
        );

        RefreshDataCommand = ReactiveCommand.CreateFromTask(LoadDataInternal);

        // Configurar observables para actualizar propiedades calculadas cuando cambien las propiedades base
        this.WhenAnyValue(
            x => x.SurvivalBudget,
            x => x.OptionalBudget, 
            x => x.CultureBudget,
            x => x.UnexpectedBudget,
            x => x.PlannedIncome)
            .Subscribe(_ => UpdateCalculatedProperties());

        this.WhenAnyValue(
            x => x.CurrentYear,
            x => x.CurrentMonth)
            .Subscribe(_ => this.RaisePropertyChanged(nameof(MonthYearDisplay)));
    }

    public int CurrentYear { get; set; } = DateTime.Now.Year;

    public int CurrentMonth { get; set; } = DateTime.Now.Month;

    public MonthlyBudget? CurrentBudget { get; set; }

    public MonthlyExpenses? ActualExpenses { get; set; }

    public BalanceInfo? BalanceInfo { get; set; }

    // Campos editables con Fody - auto-propiedades
    [Reactive]
    public decimal PlannedIncome { get; set; }

    public decimal SurvivalBudget { get; set; }

    public decimal OptionalBudget { get; set; }

    public decimal CultureBudget { get; set; }

    public decimal UnexpectedBudget { get; set; }

    // Propiedades calculadas
    public decimal TotalBudget => SurvivalBudget + OptionalBudget + CultureBudget + UnexpectedBudget;
    public decimal RemainingBudget => PlannedIncome - TotalBudget;
    public string MonthYearDisplay => $"{GetMonthName(CurrentMonth)} {CurrentYear}";

    // Estado de presupuesto
    public bool IsSurvivalOverBudget => ActualExpenses?.Survival > SurvivalBudget;
    public bool IsOptionalOverBudget => ActualExpenses?.Optional > OptionalBudget;
    public bool IsCultureOverBudget => ActualExpenses?.Culture > CultureBudget;
    public bool IsUnexpectedOverBudget => ActualExpenses?.Unexpected > UnexpectedBudget;

    public ICommand PreviousMonthCommand { get; }
    public ICommand NextMonthCommand { get; }
    public ReactiveCommand<Unit, Unit> SaveBudgetCommand { get; }
    public ReactiveCommand<Unit, Unit> RefreshDataCommand { get; }

    public async Task LoadData()
    {
        await LoadDataInternal();
    }

    // Renombra el método original a privado para uso interno
    private async Task LoadDataInternal()
    {
        await Avalonia.Threading.Dispatcher.UIThread.InvokeAsync(() => IsBusy = true);
        try
        {
            var budget = await _budgetService.GetMonthlyBudgetAsync(CurrentYear, CurrentMonth);
            var expenses = await _budgetService.CalculateActualExpensesAsync(CurrentYear, CurrentMonth);
            var balance = await _transactionService.GetBalanceAsync(CurrentYear, CurrentMonth);

            await Avalonia.Threading.Dispatcher.UIThread.InvokeAsync(() =>
            {
                CurrentBudget = budget;
                ActualExpenses = expenses;
                BalanceInfo = balance;
                LoadBudgetFields(budget);
                UpdateCalculatedProperties();
            });
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error loading budget data");
        }
        finally
        {
            await Avalonia.Threading.Dispatcher.UIThread.InvokeAsync(() => IsBusy = false);
        }
    }

    private void LoadBudgetFields(MonthlyBudget? budget)
    {
        if (budget != null)
        {
            PlannedIncome = budget.PlannedIncome;
            SurvivalBudget = budget.SurvivalBudget;
            OptionalBudget = budget.OptionalBudget;
            CultureBudget = budget.CultureBudget;
            UnexpectedBudget = budget.UnexpectedBudget;
        }
        else
        {
            // Valores por defecto
            PlannedIncome = 0;
            SurvivalBudget = 0;
            OptionalBudget = 0;
            CultureBudget = 0;
            UnexpectedBudget = 0;
        }
    }

    public string? ErrorMessage { get; set; }

    private async Task SaveBudget()
    {
        await Avalonia.Threading.Dispatcher.UIThread.InvokeAsync(() => { IsBusy = true; ErrorMessage = null; });
        try
        {
            var budget = new MonthlyBudget
            {
                Id = CurrentBudget?.Id,
                Year = CurrentYear,
                Month = CurrentMonth,
                PlannedIncome = PlannedIncome,
                SurvivalBudget = SurvivalBudget,
                OptionalBudget = OptionalBudget,
                CultureBudget = CultureBudget,
                UnexpectedBudget = UnexpectedBudget
            };

            var saved = await _budgetService.SaveMonthlyBudgetAsync(budget);
            if (saved.IsSuccess)
            {
                var result = saved.GetValue();
                await Avalonia.Threading.Dispatcher.UIThread.InvokeAsync(() =>
                {
                    CurrentBudget = result;
                    ErrorMessage = null;
                });
            }
            else
            {
                var error = saved.GetError();
                await Avalonia.Threading.Dispatcher.UIThread.InvokeAsync(() =>
                {
                    ErrorMessage = error;
                });
            }
            // Recargar todos los datos para asegurar que todo esté actualizado
            await ReloadDataAfterSave();
            Log.Information("Budget saved successfully for {Year}-{Month}", CurrentYear, CurrentMonth);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error saving budget");
            await Avalonia.Threading.Dispatcher.UIThread.InvokeAsync(() => ErrorMessage = ex.Message);
        }
        finally
        {
            await Avalonia.Threading.Dispatcher.UIThread.InvokeAsync(() => IsBusy = false);
        }
    }

    private async Task ReloadDataAfterSave()
    {
        try
        {
            // Recargar expenses y balance info después de guardar
            var expenses = await _budgetService.CalculateActualExpensesAsync(CurrentYear, CurrentMonth);
            var balance = await _transactionService.GetBalanceAsync(CurrentYear, CurrentMonth);

            await Avalonia.Threading.Dispatcher.UIThread.InvokeAsync(() =>
            {
                ActualExpenses = expenses;
                BalanceInfo = balance;
                UpdateCalculatedProperties();
            });
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error reloading data after save");
        }
    }

    private async Task PreviousMonth()
    {
        var (year, month) = DateHelpers.GetPreviousMonth(CurrentYear, CurrentMonth);
        
        // Asegurar que los cambios se hagan en el UI thread
        if (Avalonia.Threading.Dispatcher.UIThread.CheckAccess())
        {
            CurrentYear = year;
            CurrentMonth = month;
            await LoadData();
        }
        else
        {
            await Avalonia.Threading.Dispatcher.UIThread.InvokeAsync(async () =>
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
        if (Avalonia.Threading.Dispatcher.UIThread.CheckAccess())
        {
            CurrentYear = year;
            CurrentMonth = month;
            await LoadData();
        }
        else
        {
            await Avalonia.Threading.Dispatcher.UIThread.InvokeAsync(async () =>
            {
                CurrentYear = year;
                CurrentMonth = month;
                await LoadData();
            });
        }
    }

    private void UpdateCalculatedProperties()
    {
        // Notificar cambios en propiedades calculadas usando RaisePropertyChanged para mayor compatibilidad
        this.RaisePropertyChanged(nameof(TotalBudget));
        this.RaisePropertyChanged(nameof(RemainingBudget));
        this.RaisePropertyChanged(nameof(MonthYearDisplay));
        this.RaisePropertyChanged(nameof(IsSurvivalOverBudget));
        this.RaisePropertyChanged(nameof(IsOptionalOverBudget));
        this.RaisePropertyChanged(nameof(IsCultureOverBudget));
        this.RaisePropertyChanged(nameof(IsUnexpectedOverBudget));
    }

    private static string GetMonthName(int month) => month switch
    {
        1 => "Enero", 2 => "Febrero", 3 => "Marzo", 4 => "Abril",
        5 => "Mayo", 6 => "Junio", 7 => "Julio", 8 => "Agosto",
        9 => "Septiembre", 10 => "Octubre", 11 => "Noviembre", 12 => "Diciembre",
        _ => month.ToString()
    };
}

