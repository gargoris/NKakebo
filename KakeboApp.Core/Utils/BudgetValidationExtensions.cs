using System.Collections.Generic;
using KakeboApp.Core.Models;

namespace KakeboApp.Core.Utils;

// Extensiones para validación de presupuestos
public static class BudgetValidationExtensions
{
    public static Result<MonthlyBudget> Validate(this MonthlyBudget budget)
    {
        var errors = new List<string>();

        if (budget.Year < 1900 || budget.Year > 2100)
            errors.Add("Year must be between 1900 and 2100");

        if (budget.Month < 1 || budget.Month > 12)
            errors.Add("Month must be between 1 and 12");

        if (budget.PlannedIncome < 0)
            errors.Add("Planned income cannot be negative");

        if (budget.SurvivalBudget < 0)
            errors.Add("Survival budget cannot be negative");

        if (budget.OptionalBudget < 0)
            errors.Add("Optional budget cannot be negative");

        if (budget.CultureBudget < 0)
            errors.Add("Culture budget cannot be negative");

        if (budget.UnexpectedBudget < 0)
            errors.Add("Unexpected budget cannot be negative");

        var totalBudget = budget.SurvivalBudget + budget.OptionalBudget +
                          budget.CultureBudget + budget.UnexpectedBudget;

        if (totalBudget > budget.PlannedIncome)
            errors.Add("Total budget cannot exceed planned income");

        return errors.Count == 0
            ? new Result<MonthlyBudget>.Success(budget)
            : new Result<MonthlyBudget>.Error(string.Join("; ", errors));
    }
}
