using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using KakeboApp.Core.Interfaces;
using KakeboApp.Core.Models;
using KakeboApp.Core.Utils;

namespace KakeboApp.Core.Services;

// Implementación del servicio de transacciones
public class TransactionService : ITransactionService
{
    private readonly IKakeboDatabase _database;

    public TransactionService(IKakeboDatabase database)
    {
        _database = database;
    }

    public async Task<IReadOnlyList<Transaction>> GetAllTransactionsAsync()
    {
        var result = await _database.GetAllTransactionsAsync();
        return result.Match(
            transactions => transactions,
            error => throw new InvalidOperationException($"Database error: {error}")
        );
    }

    public async Task<IReadOnlyList<Transaction>> GetTransactionsByMonthAsync(int year, int month)
    {
        var from = new DateTime(year, month, 1);
        var to = from.AddMonths(1).AddDays(-1);
        return await GetTransactionsByDateRangeAsync(from, to);
    }

    public async Task<IReadOnlyList<Transaction>> GetTransactionsByDateRangeAsync(DateTime from, DateTime to)
    {
        var result = await _database.GetTransactionsByDateRangeAsync(from, to);
        return result.Match(
            transactions => transactions,
            error => throw new InvalidOperationException($"Database error: {error}")
        );
    }

    public async Task<IReadOnlyList<ExpenseByCategory>> GetExpensesByCategoryAsync(int year, int month)
    {
        var transactions = await GetTransactionsByMonthAsync(year, month);
        var expenses = transactions.Where(t => t.Type == TransactionType.Expense);

        var totalExpenses = expenses.Sum(e => e.Amount);
        if (totalExpenses == 0) return Array.Empty<ExpenseByCategory>();

        return expenses
            .GroupBy(e => e.Category)
            .Select(g => new ExpenseByCategory(
                g.Key,
                g.Sum(e => e.Amount),
                (double)(g.Sum(e => e.Amount) / totalExpenses * 100)
            ))
            .OrderByDescending(e => e.Amount)
            .ToList();
    }

    public async Task<IReadOnlyList<ExpenseBySubcategory>> GetExpensesBySubcategoryAsync(int year, int month)
    {
        var transactions = await GetTransactionsByMonthAsync(year, month);
        var expenses = transactions.Where(t => t.Type == TransactionType.Expense);

        var totalExpenses = expenses.Sum(e => e.Amount);
        if (totalExpenses == 0) return Array.Empty<ExpenseBySubcategory>();

        return expenses
            .GroupBy(e => new { e.Category, e.Subcategory })
            .Select(g => new ExpenseBySubcategory(
                g.Key.Category,
                g.Key.Subcategory,
                g.Sum(e => e.Amount),
                (double)(g.Sum(e => e.Amount) / totalExpenses * 100)
            ))
            .OrderByDescending(e => e.Amount)
            .ToList();
    }

    public async Task<BalanceInfo> GetBalanceAsync(int year, int month)
    {
        var transactions = await GetTransactionsByMonthAsync(year, month);

        var totalIncome = transactions
            .Where(t => t.Type == TransactionType.Income)
            .Sum(t => t.Amount);

        var totalExpenses = transactions
            .Where(t => t.Type == TransactionType.Expense)
            .Sum(t => t.Amount);

        var balance = totalIncome - totalExpenses;
        var savingsRate = totalIncome > 0 ? (double)(balance / totalIncome * 100) : 0;

        return new BalanceInfo(totalIncome, totalExpenses, balance, savingsRate);
    }

    public async Task<BalanceInfo> GetTotalBalanceAsync()
    {
        var transactions = await GetAllTransactionsAsync();

        var totalIncome = transactions
            .Where(t => t.Type == TransactionType.Income)
            .Sum(t => t.Amount);

        var totalExpenses = transactions
            .Where(t => t.Type == TransactionType.Expense)
            .Sum(t => t.Amount);

        var balance = totalIncome - totalExpenses;
        var savingsRate = totalIncome > 0 ? (double)(balance / totalIncome * 100) : 0;

        return new BalanceInfo(totalIncome, totalExpenses, balance, savingsRate);
    }

    public async Task<Transaction> AddTransactionAsync(Transaction transaction)
    {
        var result = await _database.AddTransactionAsync(transaction);
        return result.Match(
            addedTransaction => addedTransaction,
            error => throw new InvalidOperationException($"Database error: {error}")
        );
    }

    public async Task<Transaction> UpdateTransactionAsync(Transaction transaction)
    {
        var result = await _database.UpdateTransactionAsync(transaction);
        return result.Match(
            updatedTransaction => updatedTransaction,
            error => throw new InvalidOperationException($"Database error: {error}")
        );
    }

    public async Task DeleteTransactionAsync(int transactionId)
    {
        var result = await _database.DeleteTransactionAsync(transactionId);
        result.Match<Unit, string>(
            _ => { return null; },
            error => throw new InvalidOperationException($"Database error: {error}")
        );
    }

    public async Task<Transaction?> GetTransactionByIdAsync(int transactionId)
    {
        var result = await _database.GetTransactionByIdAsync(transactionId);
        return result.Match(
            transaction => transaction,
            error => throw new InvalidOperationException($"Database error: {error}")
        );
    }
}
