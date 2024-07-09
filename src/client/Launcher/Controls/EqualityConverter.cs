using Avalonia.Data.Converters;

namespace Arise.Client.Launcher.Controls;

internal sealed class EqualityConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        var comparer = EqualityComparer<object>.Default;
        return comparer.Equals(value, parameter);
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return null;
    }
}
