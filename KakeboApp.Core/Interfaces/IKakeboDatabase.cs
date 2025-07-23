using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using KakeboApp.Core.Models;
using KakeboApp.Core.Utils;

namespace KakeboApp.Core.Interfaces;

// Interfaz principal de base de datos
public interface IKakeboDatabase
{
    Task<Result<Unit>> ConnectAsync(DatabaseConfig config);
    Task<Result<bool>> TestConnectionAsync();
    Task<Result<IReadOnlyList<Transaction>>> GetAllTransactionsAsync();
    Task<Result<Transaction>> AddTransactionAsync(Transaction transaction);
    Task<Result<Transaction>> UpdateTransactionAsync(Transaction transaction);
    Task<Result<Unit>> DeleteTransactionAsync(int transactionId);
    Task<Result<Transaction?>> GetTransactionByIdAsync(int transactionId);
    Task<Result<IReadOnlyList<Transaction>>> GetTransactionsByDateRangeAsync(DateTime from, DateTime to);
    Task<Result<MonthlyBudget?>> GetMonthlyBudgetAsync(int year, int month);
    Task<Result<MonthlyBudget>> SaveMonthlyBudgetAsync(MonthlyBudget budget);
    Task<Result<IReadOnlyList<MonthlyBudget>>> GetAllBudgetsAsync();
}
