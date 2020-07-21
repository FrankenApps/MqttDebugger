using Avalonia.Data.Converters;
using Avalonia.Input;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace MqttDebugger.Views.ValueConverters
{
    public class BooleanToKeyboardShortcutConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool isShiftEnter = (bool)value;
            if (isShiftEnter)
            {
                return new KeyGesture(Key.Enter, KeyModifiers.Shift);
            }
            return new KeyGesture(Key.Enter);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
