using System;
using System.Collections.Generic;
using LiteDB;
using KakeboApp.Core.Models;
using KakeboApp.Core.Interfaces;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using KakeboApp.Core.Utils;

namespace KakeboApp.Core.Data;

public class LiteDbKakeboDatabase : IKakeboDatabase, IDisposable
{
    private LiteDatabase? _database;
    private bool _disposed;

    private const string TransactionsCollection = "transactions";
    private const string BudgetsCollection = "budgets";

    public async Task<Result<Unit>> ConnectAsync(DatabaseConfig config)
    {
        try
        {
            await Task.Run(() =>
            {
                _database?.Dispose();

                var connectionString = new ConnectionString(config.FilePath)
                {
                    Password = config.Password,
                    ReadOnly = config.ReadOnly
                };

                _database = new LiteDatabase(connectionString);

                // Configurar índices
                SetupIndexes();
            });

            return new Result<Unit>.Success(Unit.Value);
        }
        catch (Exception ex)
        {
            return new Result<Unit>.Error($"Connection failed: {ex.Message}");
        }
    }

    public async Task<Result<bool>> TestConnectionAsync()
    {
        if (_database == null)
            return new Result<bool>.Error("No database connection");

        try
        {
            await Task.Run(() =>
            {
                // Test básico de acceso
                var transactions = _database.GetCollection<TransactionDocument>(TransactionsCollection);
                _ = transactions.Count();
            });

            return new Result<bool>.Success(true);
        }
        catch (Exception ex)
        {
            return new Result<bool>.Error($"Connection test failed: {ex.Message}");
        }
    }

    public async Task<Result<IReadOnlyList<Transaction>>> GetAllTransactionsAsync()
    {
        if (_database == null)
            return new Result<IReadOnlyList<Transaction>>.Error("No database connection");

        try
        {
            var result = await Task.Run(() =>
            {
                var collection = _database.GetCollection<TransactionDocument>(TransactionsCollection);
                return collection.FindAll()
                    .Select(MapToTransaction)
                    .OrderByDescending(t => t.Date)
                    .ToList();
            });

            return new Result<IReadOnlyList<Transaction>>.Success(result);
        }
        catch (Exception ex)
        {
            return new Result<IReadOnlyList<Transaction>>.Error($"Failed to get transactions: {ex.Message}");
        }
    }

    public async Task<Result<Transaction>> AddTransactionAsync(Transaction transaction)
    {
        if (_database == null)
            return new Result<Transaction>.Error("No database connection");

        try
        {
            var validationResult = ValidateTransaction(transaction);
            if (!validationResult.IsSuccess)
                return new Result<Transaction>.Error(validationResult.GetError());

            var result = await Task.Run(() =>
            {
                var collection = _database.GetCollection<TransactionDocument>(TransactionsCollection);
                var document = MapToDocument(transaction);

                var id = collection.Insert(document);
                document.Id = id.AsInt32;

                return MapToTransaction(document);
            });

            return new Result<Transaction>.Success(result);
        }
        catch (Exception ex)
        {
            return new Result<Transaction>.Error($"Failed to add transaction: {ex.Message}");
        }
    }

    public async Task<Result<Transaction>> UpdateTransactionAsync(Transaction transaction)
    {
        if (_database == null)
            return new Result<Transaction>.Error("No database connection");

        if (!transaction.Id.HasValue)
            return new Result<Transaction>.Error("Transaction ID is required for update");

        try
        {
            var validationResult = ValidateTransaction(transaction);
            if (!validationResult.IsSuccess)
                return new Result<Transaction>.Error(validationResult.GetError());

            var result = await Task.Run(() =>
            {
                var collection = _database.GetCollection<TransactionDocument>(TransactionsCollection);
                var document = MapToDocument(transaction);

                var updated = collection.Update(document);
                if (!updated)
                    throw new InvalidOperationException($"Transaction with ID {transaction.Id} not found");

                return transaction;
            });

            return new Result<Transaction>.Success(result);
        }
        catch (Exception ex)
        {
            return new Result<Transaction>.Error($"Failed to update transaction: {ex.Message}");
        }
    }

    public async Task<Result<Unit>> DeleteTransactionAsync(int transactionId)
    {
        if (_database == null)
            return new Result<Unit>.Error("No database connection");

        try
        {
            await Task.Run(() =>
            {
                var collection = _database.GetCollection<TransactionDocument>(TransactionsCollection);
                var deleted = collection.Delete(transactionId);

                if (!deleted)
                    throw new InvalidOperationException($"Transaction with ID {transactionId} not found");
            });

            return new Result<Unit>.Success(Unit.Value);
        }
        catch (Exception ex)
        {
            return new Result<Unit>.Error($"Failed to delete transaction: {ex.Message}");
        }
    }

    public async Task<Result<Transaction?>> GetTransactionByIdAsync(int transactionId)
    {
        if (_database == null)
            return new Result<Transaction?>.Error("No database connection");

        try
        {
            var result = await Task.Run(() =>
            {
                var collection = _database.GetCollection<TransactionDocument>(TransactionsCollection);
                var document = collection.FindById(transactionId);

                return document != null ? MapToTransaction(document) : null;
            });

            return new Result<Transaction?>.Success(result);
        }
        catch (Exception ex)
        {
            return new Result<Transaction?>.Error($"Failed to get transaction: {ex.Message}");
        }
    }

    public async Task<Result<IReadOnlyList<Transaction>>> GetTransactionsByDateRangeAsync(DateTime from, DateTime to)
    {
        if (_database == null)
            return new Result<IReadOnlyList<Transaction>>.Error("No database connection");

        try
        {
            var result = await Task.Run(() =>
            {
                var collection = _database.GetCollection<TransactionDocument>(TransactionsCollection);
                return collection.Find(d => d.Date >= from && d.Date <= to)
                    .Select(MapToTransaction)
                    .OrderByDescending(t => t.Date)
                    .ToList();
            });

            return new Result<IReadOnlyList<Transaction>>.Success(result);
        }
        catch (Exception ex)
        {
            return new Result<IReadOnlyList<Transaction>>.Error($"Failed to get transactions by date range: {ex.Message}");
        }
    }

    public async Task<Result<MonthlyBudget?>> GetMonthlyBudgetAsync(int year, int month)
    {
        if (_database == null)
            return new Result<MonthlyBudget?>.Error("No database connection");

        try
        {
            var result = await Task.Run(() =>
            {
                var collection = _database.GetCollection<BudgetDocument>(BudgetsCollection);
                var document = collection.FindOne(b => b.Year == year && b.Month == month);

                return document != null ? MapToBudget(document) : null;
            });

            return new Result<MonthlyBudget?>.Success(result);
        }
        catch (Exception ex)
        {
            return new Result<MonthlyBudget?>.Error($"Failed to get budget: {ex.Message}");
        }
    }

    public async Task<Result<MonthlyBudget>> SaveMonthlyBudgetAsync(MonthlyBudget budget)
    {
        if (_database == null)
            return new Result<MonthlyBudget>.Error("No database connection");

        try
        {
            var validationResult = budget.Validate();
            if (!validationResult.IsSuccess)
                return new Result<MonthlyBudget>.Error(validationResult.GetError());

            var result = await Task.Run(() =>
            {
                var collection = _database.GetCollection<BudgetDocument>(BudgetsCollection);
                var document = MapToBudgetDocument(budget);

                if (budget.Id.HasValue)
                {
                    collection.Update(document);
                    return budget;
                }
                else
                {
                    var id = collection.Insert(document);
                    return budget with { Id = id.AsInt32 };
                }
            });

            return new Result<MonthlyBudget>.Success(result);
        }
        catch (Exception ex)
        {
            return new Result<MonthlyBudget>.Error($"Failed to save budget: {ex.Message}");
        }
    }

    public async Task<Result<IReadOnlyList<MonthlyBudget>>> GetAllBudgetsAsync()
    {
        if (_database == null)
            return new Result<IReadOnlyList<MonthlyBudget>>.Error("No database connection");

        try
        {
            var result = await Task.Run(() =>
            {
                var collection = _database.GetCollection<BudgetDocument>(BudgetsCollection);
                return collection.FindAll()
                    .Select(MapToBudget)
                    .OrderByDescending(b => b.Year)
                    .ThenByDescending(b => b.Month)
                    .ToList();
            });

            return new Result<IReadOnlyList<MonthlyBudget>>.Success(result);
        }
        catch (Exception ex)
        {
            return new Result<IReadOnlyList<MonthlyBudget>>.Error($"Failed to get budgets: {ex.Message}");
        }
    }

    private void SetupIndexes()
    {
        if (_database == null) return;

        var transactions = _database.GetCollection<TransactionDocument>(TransactionsCollection);
        transactions.EnsureIndex(x => x.Date);
        transactions.EnsureIndex(x => x.Category);
        transactions.EnsureIndex(x => x.Type);
        transactions.EnsureIndex(x => x.Subcategory);

        var budgets = _database.GetCollection<BudgetDocument>(BudgetsCollection);
        budgets.EnsureIndex(x => new { x.Year, x.Month }, true); // Unique index
    }

    private static Result<Unit> ValidateTransaction(Transaction transaction)
    {
        var context = new ValidationContext(transaction);
        var results = new List<ValidationResult>();

        if (!Validator.TryValidateObject(transaction, context, results, true))
        {
            var errors = string.Join("; ", results.Select(r => r.ErrorMessage));
            return new Result<Unit>.Error($"Validation failed: {errors}");
        }

        return new Result<Unit>.Success(Unit.Value);
    }

    private static TransactionDocument MapToDocument(Transaction transaction) => new()
    {
        Id = transaction.Id ?? 0,
        Description = transaction.Description,
        Amount = transaction.Amount,
        Date = transaction.Date,
        Type = transaction.Type.ToString(),
        Category = transaction.Category.ToString(),
        Subcategory = transaction.Subcategory,
        Notes = transaction.Notes
    };

    private static Transaction MapToTransaction(TransactionDocument document) => new()
    {
        Id = document.Id,
        Description = document.Description,
        Amount = document.Amount,
        Date = document.Date,
        Type = Enum.Parse<TransactionType>(document.Type),
        Category = Enum.Parse<Category>(document.Category),
        Subcategory = document.Subcategory,
        Notes = document.Notes
    };

    private static BudgetDocument MapToBudgetDocument(MonthlyBudget budget) => new()
    {
        Id = budget.Id ?? 0,
        Year = budget.Year,
        Month = budget.Month,
        PlannedIncome = budget.PlannedIncome,
        SurvivalBudget = budget.SurvivalBudget,
        OptionalBudget = budget.OptionalBudget,
        CultureBudget = budget.CultureBudget,
        UnexpectedBudget = budget.UnexpectedBudget,
        CreatedAt = budget.CreatedAt
    };

    private static MonthlyBudget MapToBudget(BudgetDocument document) => new()
    {
        Id = document.Id,
        Year = document.Year,
        Month = document.Month,
        PlannedIncome = document.PlannedIncome,
        SurvivalBudget = document.SurvivalBudget,
        OptionalBudget = document.OptionalBudget,
        CultureBudget = document.CultureBudget,
        UnexpectedBudget = document.UnexpectedBudget,
        CreatedAt = document.CreatedAt
    };

    public void Dispose()
    {
        if (!_disposed)
        {
            _database?.Dispose();
            _disposed = true;
        }
    }
}

// Documentos para LiteDB
internal class TransactionDocument
{
    public int Id { get; set; }
    public required string Description { get; set; }
    public decimal Amount { get; set; }
    public DateTime Date { get; set; }
    public required string Type { get; set; }
    public required string Category { get; set; }
    public string? Subcategory { get; set; }
    public string? Notes { get; set; }
}

internal class BudgetDocument
{
    public int Id { get; set; }
    public int Year { get; set; }
    public int Month { get; set; }
    public decimal PlannedIncome { get; set; }
    public decimal SurvivalBudget { get; set; }
    public decimal OptionalBudget { get; set; }
    public decimal CultureBudget { get; set; }
    public decimal UnexpectedBudget { get; set; }
    public DateTime CreatedAt { get; set; }
}
