using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;

namespace Psicho_Support.Helpers
{
    public class BooleanToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == DependencyProperty.UnsetValue || value == null)
                return Visibility.Collapsed;

            if (value is bool)
            {
                bool boolValue = (bool)value;
                bool inverse = parameter != null && parameter.ToString() == "Inverse";
                if (inverse)
                    return boolValue ? Visibility.Collapsed : Visibility.Visible;
                else
                    return boolValue ? Visibility.Visible : Visibility.Collapsed;
            }
            return Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == DependencyProperty.UnsetValue || value == null)
                return false;

            if (value is Visibility)
            {
                Visibility visibility = (Visibility)value;
                bool inverse = parameter != null && parameter.ToString() == "Inverse";
                if (inverse)
                    return visibility != Visibility.Visible;
                else
                    return visibility == Visibility.Visible;
            }
            return false;
        }
    }

    public class InverseBooleanConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool)
            {
                return !(bool)value;
            }
            return false;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool)
            {
                return !(bool)value;
            }
            return false;
        }
    }

    public class InverseBooleanToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool boolValue)
            {
                return boolValue ? Visibility.Collapsed : Visibility.Visible;
            }
            return Visibility.Visible;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class NullToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool isVisible = value != null;

            if (parameter != null && parameter.ToString() == "Inverse")
                isVisible = !isVisible;

            return isVisible ? Visibility.Visible : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class StateToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is int)
            {
                int stateValue = (int)value;

                if (stateValue <= 20)
                    return "#F44336";
                else if (stateValue <= 40)
                    return "#FF9800";
                else if (stateValue <= 60)
                    return "#FFC107";
                else if (stateValue <= 80)
                    return "#8BC34A";
                else
                    return "#4CAF50";
            }

            return "#AAAAAA";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class StateToEmojiConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is int)
            {
                int stateValue = (int)value;

                if (stateValue >= 80)
                    return "😊";
                else if (stateValue >= 60)
                    return "🙂";
                else if (stateValue >= 40)
                    return "😐";
                else if (stateValue >= 20)
                    return "😟";
                else
                    return "😢";
            }

            return "😐";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class TimeSpanToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is TimeSpan)
            {
                TimeSpan timeSpan = (TimeSpan)value;

                if (parameter != null && parameter.ToString() == "Short")
                {
                    if (timeSpan.TotalHours >= 24)
                        return $"{timeSpan.Days}д {timeSpan.Hours}ч";
                    if (timeSpan.TotalHours >= 1)
                        return $"{timeSpan.Hours}ч {timeSpan.Minutes}м";
                    return $"{timeSpan.Minutes}м";
                }

                if (timeSpan.TotalHours >= 1)
                    return $"{(int)timeSpan.TotalHours}ч {timeSpan.Minutes}м {timeSpan.Seconds}с";
                if (timeSpan.TotalMinutes >= 1)
                    return $"{timeSpan.Minutes}м {timeSpan.Seconds}с";
                return $"{timeSpan.Seconds}с";
            }

            if (parameter != null && parameter.ToString() == "Short")
                return "0м";
            return "0м 0с";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class StringToBrushConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string colorString && !string.IsNullOrEmpty(colorString))
            {
                try
                {
                    var color = (Color)ColorConverter.ConvertFromString(colorString);
                    return new SolidColorBrush(color);
                }
                catch
                {
                    return new SolidColorBrush(Colors.Gray);
                }
            }
            return new SolidColorBrush(Colors.Gray);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    // ========== НОВЫЕ КОНВЕРТЕРЫ ДЛЯ ЗАМЕТОК ==========

    public class StressToCardColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is int stressLevel)
            {
                if (stressLevel <= 20)
                    return "#2E7D32";
                else if (stressLevel <= 40)
                    return "#558B2F";
                else if (stressLevel <= 60)
                    return "#F9A825";
                else if (stressLevel <= 80)
                    return "#F57C00";
                else
                    return "#C62828";
            }
            return "#2A2A3D";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class StressToTextColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is int stressLevel)
            {
                if (stressLevel <= 40)
                    return "#2E7D32";
                else if (stressLevel <= 60)
                    return "#F57C00";
                else
                    return "#C62828";
            }
            return "#AAAAAA";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class StressToEmojiConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is int stressLevel)
            {
                if (stressLevel <= 20)
                    return "😊";
                else if (stressLevel <= 40)
                    return "🙂";
                else if (stressLevel <= 60)
                    return "😐";
                else if (stressLevel <= 80)
                    return "😟";
                else
                    return "😢";
            }  
            return "📝";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class ExpandCollapseIconConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool isExpanded && isExpanded)
                return "▼";  // Свернуто
            return "▶";      // Развернуто
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    //КОНВЕРТЕРЫ ДЛЯ ОНБОРДИНГА
    public class BooleanToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool boolValue && boolValue)
            {
                return (SolidColorBrush)Application.Current.FindResource("AccentBrush");
            }
            return new SolidColorBrush(Color.FromRgb(100, 100, 100));
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}