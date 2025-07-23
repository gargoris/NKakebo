using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using KakeboApp.Core.Models;

namespace KakeboApp.Core.Interfaces;

// Servicio de transacciones
public interface ITransactionService
{
    Task<IReadOnlyList<Transaction>> GetAllTransactionsAsync();
    Task<IReadOnlyList<Transaction>> GetTransactionsByMonthAsync(int year, int month);
    Task<IReadOnlyList<Transaction>> GetTransactionsByDateRangeAsync(DateTime from, DateTime to);
    Task<IReadOnlyList<ExpenseByCategory>> GetExpensesByCategoryAsync(int year, int month);
    Task<IReadOnlyList<ExpenseBySubcategory>> GetExpensesBySubcategoryAsync(int year, int month);
    Task<BalanceInfo> GetBalanceAsync(int year, int month);
    Task<BalanceInfo> GetTotalBalanceAsync();
    Task<Transaction> AddTransactionAsync(Transaction transaction);
    Task<Transaction> UpdateTransactionAsync(Transaction transaction);
    Task DeleteTransactionAsync(int transactionId);
    Task<Transaction?> GetTransactionByIdAsync(int transactionId);
}
