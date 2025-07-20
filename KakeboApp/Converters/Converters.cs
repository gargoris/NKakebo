// Converters.cs - Value Converters for Avalonia
using Avalonia.Data.Converters;
using Avalonia;
using System.Globalization;
using KakeboApp.Core.Models;
using KakeboApp.Core.Utils;

namespace KakeboApp.Converters;

public class BooleanToGridLengthConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is bool boolValue && parameter is string param)
        {
            var parts = param.Split('|');
            if (parts.Length == 2)
            {
                var falseValue = parts[0];
                var trueValue = parts[1];
                var selectedValue = boolValue ? trueValue : falseValue;
                
                if (selectedValue == "0" || selectedValue == "Auto")
                    return GridLength.Auto;
                
                if (double.TryParse(selectedValue, out var length))
                    return new GridLength(length);
            }
        }
        return GridLength.Auto;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

public class BooleanToIntConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is bool boolValue && parameter is string param)
        {
            var parts = param.Split('|');
            if (parts.Length == 2 && int.TryParse(parts[0], out var falseInt) && int.TryParse(parts[1], out var trueInt))
            {
                return boolValue ? trueInt : falseInt;
            }
        }
        return 0;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

public class TransactionTypeToColorConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is TransactionType type)
        {
            return type switch
            {
                TransactionType.Income => "Green",
                TransactionType.Expense => "Red",
                _ => "Black"
            };
        }
        return "Black";
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

public class BalanceToColorConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is decimal balance)
        {
            return balance >= 0 ? "Green" : "Red";
        }
        return "Black";
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

public class CategoryDisplayConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is Category category)
        {
            return CategoryUtils.GetCategoryDisplayName(category);
        }
        return value?.ToString();
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}