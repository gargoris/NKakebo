using ReactiveUI;
using System.Collections.ObjectModel;
using System.Reactive;
using KakeboApp.Core.Interfaces;
using KakeboApp.Core.Models;
using KakeboApp.Core.Utils;

namespace KakeboApp.ViewModels;

// ViewModel para presupuesto Kakebo
public class BudgetViewModel : ViewModelBase
{
    private readonly IBudgetService _budgetService;
    private readonly ITransactionService _transactionService;
    
    private int _currentYear = DateTime.Now.Year;
    private int _currentMonth = DateTime.Now.Month;
    private MonthlyBudget? _currentBudget;
    private MonthlyExpenses? _actualExpenses;
    private BalanceInfo? _balanceInfo;
    
    // Campos de presupuesto
    private decimal _plannedIncome;
    private decimal _survivalBudget;
    private decimal _optionalBudget;
    private decimal _cultureBudget;
    private decimal _unexpectedBudget;
    
    public BudgetViewModel(IBudgetService budgetService, ITransactionService transactionService)
    {
        _budgetService = budgetService;
        _transactionService = transactionService;
        
        // Comandos de navegación
        PreviousMonthCommand = ReactiveCommand.CreateFromTask(PreviousMonth);
        NextMonthCommand = ReactiveCommand.CreateFromTask(NextMonth);
        
        // Comandos de presupuesto
        var canSave = this.WhenAnyValue(x => x.PlannedIncome, income => income > 0);
        SaveBudgetCommand = ReactiveCommand.CreateFromTask(SaveBudget, canSave);
        
        RefreshDataCommand = ReactiveCommand.CreateFromTask(LoadData);
        
        // Cargar datos iniciales
        LoadData();
    }
    
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
    
    public MonthlyBudget? CurrentBudget
    {
        get => _currentBudget;
        set => this.RaiseAndSetIfChanged(ref _currentBudget, value);
    }
    
    public MonthlyExpenses? ActualExpenses
    {
        get => _actualExpenses;
        set => this.RaiseAndSetIfChanged(ref _actualExpenses, value);
    }
    
    public BalanceInfo? BalanceInfo
    {
        get => _balanceInfo;
        set => this.RaiseAndSetIfChanged(ref _balanceInfo, value);
    }
    
    // Campos editables
    public decimal PlannedIncome
    {
        get => _plannedIncome;
        set => this.RaiseAndSetIfChanged(ref _plannedIncome, value);
    }
    
    public decimal SurvivalBudget
    {
        get => _survivalBudget;
        set => this.RaiseAndSetIfChanged(ref _survivalBudget, value);
    }
    
    public decimal OptionalBudget
    {
        get => _optionalBudget;
        set => this.RaiseAndSetIfChanged(ref _optionalBudget, value);
    }
    
    public decimal CultureBudget
    {
        get => _cultureBudget;
        set => this.RaiseAndSetIfChanged(ref _cultureBudget, value);
    }
    
    public decimal UnexpectedBudget
    {
        get => _unexpectedBudget;
        set => this.RaiseAndSetIfChanged(ref _unexpectedBudget, value);
    }
    
    // Propiedades calculadas
    public decimal TotalBudget => SurvivalBudget + OptionalBudget + CultureBudget + UnexpectedBudget;
    public decimal RemainingBudget => PlannedIncome - TotalBudget;
    public string MonthYearDisplay => $"{GetMonthName(CurrentMonth)} {CurrentYear}";
    
    // Estado de presupuesto
    public bool IsSurvivalOverBudget => ActualExpenses?.Survival > SurvivalBudget;
    public bool IsOptionalOverBudget => ActualExpenses?.Optional > OptionalBudget;
    public bool IsCultureOverBudget => ActualExpenses?.Culture > CultureBudget;
    public bool IsUnexpectedOverBudget => ActualExpenses?.Unexpected > UnexpectedBudget;
    
    public ReactiveCommand<Unit, Unit> PreviousMonthCommand { get; }
    public ReactiveCommand<Unit, Unit> NextMonthCommand { get; }
    public ReactiveCommand<Unit, Unit> SaveBudgetCommand { get; }
    public ReactiveCommand<Unit, Unit> RefreshDataCommand { get; }
    
    private async Task LoadData()
    {
        IsBusy = true;
        try
        {
            var budget = await _budgetService.GetMonthlyBudgetAsync(CurrentYear, CurrentMonth);
            var expenses = await _budgetService.CalculateActualExpensesAsync(CurrentYear, CurrentMonth);
            var balance = await _transactionService.GetBalanceAsync(CurrentYear, CurrentMonth);
            
            CurrentBudget = budget;
            ActualExpenses = expenses;
            BalanceInfo = balance;
            
            LoadBudgetFields(budget);
            UpdateCalculatedProperties();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error loading budget data: {ex.Message}");
        }
        finally
        {
            IsBusy = false;
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
    
    private async Task SaveBudget()
    {
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
            CurrentBudget = saved;
            UpdateCalculatedProperties();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error saving budget: {ex.Message}");
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
    
    private void UpdateCalculatedProperties()
    {
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

