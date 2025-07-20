namespace KakeboApp.Core.Services;

// Implementación del servicio de presupuesto
public class BudgetService : IBudgetService
{
    private readonly IKakeboDatabase _database;
    private readonly ITransactionService _transactionService;

    public BudgetService(IKakeboDatabase database, ITransactionService transactionService)
    {
        _database = database;
        _transactionService = transactionService;
    }

    public async Task<MonthlyExpenses> CalculateActualExpensesAsync(int year, int month)
    {
        var transactions = await _transactionService.GetTransactionsByMonthAsync(year, month);
        var expenses = transactions.Where(t => t.Type == TransactionType.Expense);

        var survival = expenses
            .Where(e => e.KakeboCategory == KakeboCategory.Survival)
            .Sum(e => e.Amount);

        var optional = expenses
            .Where(e => e.KakeboCategory == KakeboCategory.Optional)
            .Sum(e => e.Amount);

        var culture = expenses
            .Where(e => e.KakeboCategory == KakeboCategory.Culture)
            .Sum(e => e.Amount);

        var unexpected = expenses
            .Where(e => e.KakeboCategory == KakeboCategory.Unexpected)
            .Sum(e => e.Amount);

        return new MonthlyExpenses(year, month, survival, optional, culture, unexpected);
    }

    public async Task<MonthlyBudget?> GetMonthlyBudgetAsync(int year, int month)
    {
        var result = await _database.GetMonthlyBudgetAsync(year, month);
        return result.Match(
            budget => budget,
            error => throw new InvalidOperationException($"Database error: {error}")
        );
    }

    public async Task<MonthlyBudget> SaveMonthlyBudgetAsync(MonthlyBudget budget)
    {
        var result = await _database.SaveMonthlyBudgetAsync(budget);
        return result.Match(
            savedBudget => savedBudget,
            error => throw new InvalidOperationException($"Database error: {error}")
        );
    }

    public async Task<IReadOnlyList<MonthlyBudget>> GetAllBudgetsAsync()
    {
        var result = await _database.GetAllBudgetsAsync();
        return result.Match(
            budgets => budgets,
            error => throw new InvalidOperationException($"Database error: {error}")
        );
    }

    public async Task<decimal> GetSavingsRateAsync(int year, int month)
    {
        var balance = await _transactionService.GetBalanceAsync(year, month);
        return (decimal)balance.SavingsRate;
    }

    public async Task<bool> IsOverBudgetAsync(int year, int month, KakeboCategory category)
    {
        var budget = await GetMonthlyBudgetAsync(year, month);
        if (budget == null) return false;

        var actual = await CalculateActualExpensesAsync(year, month);

        return category switch
        {
            KakeboCategory.Survival => actual.Survival > budget.SurvivalBudget,
            KakeboCategory.Optional => actual.Optional > budget.OptionalBudget,
            KakeboCategory.Culture => actual.Culture > budget.CultureBudget,
            KakeboCategory.Unexpected => actual.Unexpected > budget.UnexpectedBudget,
            _ => false
        };
    }
}



