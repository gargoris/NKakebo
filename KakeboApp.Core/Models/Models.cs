using System;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using KakeboApp.Core.Utils;

namespace KakeboApp.Core.Models;

// Enums que reemplazan los discriminated unions de F#
public enum TransactionType
{
    Income,
    Expense
}

public enum Category
{
    // Ingresos
    Salary,
    Investment,
    Freelance,
    Business,
    Gifts,

    // Gastos de supervivencia
    Housing,
    Food,
    Transportation,
    Utilities,
    Healthcare,
    Insurance,

    // Gastos opcionales
    Entertainment,
    Dining,
    Shopping,
    Hobbies,
    Travel,
    Sports,

    // Cultura
    Books,
    Education,
    Courses,
    Subscriptions,

    // Inesperados
    Emergency,
    Repairs,
    Medical,
    Legal,
    Other
}

public enum KakeboCategory
{
    Survival,    // Supervivencia
    Optional,    // Opcional
    Culture,     // Cultura
    Unexpected   // Inesperado
}

// Record que reemplaza el tipo Transaction de F#
public record Transaction
{
    public int? Id { get; init; }

    [Required]
    [StringLength(200)]
    public required string Description { get; init; }

    [Range(0.01, double.MaxValue)]
    public required decimal Amount { get; init; }

    public required DateTime Date { get; init; }

    public required TransactionType Type { get; init; }

    public required Category Category { get; init; }

    [StringLength(100)]
    public string? Subcategory { get; init; }

    [StringLength(500)]
    public string? Notes { get; init; }

    // Propiedades calculadas
    public KakeboCategory KakeboCategory => CategoryUtils.GetKakeboCategory(Category);
    public string CategoryDisplayName => CategoryUtils.GetCategoryDisplayName(Category);
    public string FullCategoryName => CategoryUtils.GetFullCategoryName(Category, Subcategory);
}

// Record para configuración de presupuesto mensual
public record MonthlyBudget
{
    public int? Id { get; init; }
    public required int Year { get; init; }
    public required int Month { get; init; }
    public required decimal PlannedIncome { get; init; }
    public required decimal SurvivalBudget { get; init; }
    public required decimal OptionalBudget { get; init; }
    public required decimal CultureBudget { get; init; }
    public required decimal UnexpectedBudget { get; init; }
    public DateTime CreatedAt { get; init; } = DateTime.Now;
}

// Record para configuración de base de datos
public record DatabaseConfig
{
    public required string FilePath { get; init; }
    public string? Password { get; init; }
    public bool ReadOnly { get; init; } = false;
}

// Resultado que reemplaza DatabaseResult<T> de F#
public abstract record Result<T>
{
    public record Success(T Value) : Result<T>;
    public record Error(string Message) : Result<T>;

    public bool IsSuccess => this is Success;
    public bool IsError => this is Error;

    public T GetValue() => this is Success success ? success.Value :
        throw new InvalidOperationException("Cannot get value from error result");

    public string GetError() => this is Error error ? error.Message :
        throw new InvalidOperationException("Cannot get error from success result");
}

// Extension methods para trabajar con Result<T>
public static class ResultExtensions
{
    public static TResult Match<T, TResult>(this Result<T> result,
        Func<T, TResult> onSuccess,
        Func<string, TResult> onError)
    {
        return result switch
        {
            Result<T>.Success success => onSuccess(success.Value),
            Result<T>.Error error => onError(error.Message),
            _ => throw new ArgumentOutOfRangeException()
        };
    }

    public static async Task<Result<TResult>> MapAsync<T, TResult>(this Result<T> result,
        Func<T, Task<TResult>> mapper)
    {
        return result switch
        {
            Result<T>.Success success => new Result<TResult>.Success(await mapper(success.Value)),
            Result<T>.Error error => new Result<TResult>.Error(error.Message),
            _ => throw new ArgumentOutOfRangeException()
        };
    }

    public static Result<TResult> Map<T, TResult>(this Result<T> result,
        Func<T, TResult> mapper)
    {
        return result switch
        {
            Result<T>.Success success => new Result<TResult>.Success(mapper(success.Value)),
            Result<T>.Error error => new Result<TResult>.Error(error.Message),
            _ => throw new ArgumentOutOfRangeException()
        };
    }
}

// DTOs para reportes y análisis
public record ExpenseByCategory(Category Category, decimal Amount, double Percentage);
public record ExpenseBySubcategory(Category Category, string? Subcategory, decimal Amount, double Percentage);
public record MonthlyExpenses(int Year, int Month, decimal Survival, decimal Optional, decimal Culture, decimal Unexpected);
public record BalanceInfo(decimal TotalIncome, decimal TotalExpenses, decimal Balance, double SavingsRate);
