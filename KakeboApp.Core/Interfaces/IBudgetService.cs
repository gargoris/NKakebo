using KakeboApp.Core.Models;

namespace KakeboApp.Core.Interfaces;

// Servicio de presupuesto
public interface IBudgetService
{
    Task<MonthlyExpenses> CalculateActualExpensesAsync(int year, int month);
    Task<MonthlyBudget?> GetMonthlyBudgetAsync(int year, int month);
    Task<MonthlyBudget> SaveMonthlyBudgetAsync(MonthlyBudget budget);
    Task<IReadOnlyList<MonthlyBudget>> GetAllBudgetsAsync();
    Task<decimal> GetSavingsRateAsync(int year, int month);
    Task<bool> IsOverBudgetAsync(int year, int month, KakeboCategory category);
}
