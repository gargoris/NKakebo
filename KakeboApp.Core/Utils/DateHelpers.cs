using System;

namespace KakeboApp.Core.Services;

// Helper para operaciones de fecha
public static class DateHelpers
{
    public static DateTime GetMonthStart(int year, int month) => new(year, month, 1);

    public static DateTime GetMonthEnd(int year, int month) => GetMonthStart(year, month).AddMonths(1).AddDays(-1);

    public static (int Year, int Month) GetPreviousMonth(int year, int month)
    {
        if (month == 1)
            return (year - 1, 12);

        return (year, month - 1);
    }

    public static (int Year, int Month) GetNextMonth(int year, int month)
    {
        if (month == 12)
            return (year + 1, 1);

        return (year, month + 1);
    }

    public static DateTime GetFirstDayOfMonth(int year, int month)
    {
        return new DateTime(year, month, 1);
    }

    public static DateTime GetLastDayOfMonth(int year, int month)
    {
        return new DateTime(year, month, DateTime.DaysInMonth(year, month));
    }

    public static bool IsCurrentMonth(int year, int month)
    {
        var now = DateTime.Now;
        return year == now.Year && month == now.Month;
    }
}
