// Converters.cs - Value Converters for Avalonia

using System;
using Avalonia.Data.Converters;
using Avalonia;
using System.Globalization;
using System.Linq;
using System.Collections.Generic;
using Avalonia.Controls;
using KakeboApp.Core.Models;
using KakeboApp.Core.Utils;

namespace KakeboApp.Converters;

public class BooleanToStringConverter : IValueConverter
{
    public static BooleanToStringConverter Instance { get; } = new();
    
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is bool boolValue && parameter is string param)
        {
            var parts = param.Split('|');
            if (parts.Length == 2)
            {
                return boolValue ? parts[1] : parts[0];
            }
        }
        return value?.ToString();
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

public class BooleanToGridLengthConverter : IValueConverter
{
    public static BooleanToGridLengthConverter Instance { get; } = new();

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
                    return new GridLength(length, GridUnitType.Pixel);
            }
        }
        return GridLength.Auto;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

public class CategoryListConverter : IValueConverter
{
    public static CategoryListConverter Instance { get; } = new();

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        // Return all categories as a list
        return Enum.GetValues<Category>().ToList();
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
    public static TransactionTypeToColorConverter Instance { get; } = new();

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
    public static BalanceToColorConverter Instance { get; } = new();

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
    public static CategoryDisplayConverter Instance { get; } = new();

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
