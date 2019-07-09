using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace SavescumBuddy.ValueConverters
{
    class StringToModifierConverter : System.Windows.Data.IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return ModifierToString((Keys)value);
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return StringToModifier((string)value);
        }

        private string ModifierToString(Keys key)
        {
            switch (key)
            {
                case (Keys.Shift):
                    return UserFriendlyModifiers.Shift;
                case (Keys.Control):
                    return UserFriendlyModifiers.Control;
                case (Keys.Alt):
                    return UserFriendlyModifiers.Alt;
                default:
                    return UserFriendlyModifiers.None;
            }
        }

        private Keys StringToModifier(string key)
        {
            switch (key)
            {
                case (UserFriendlyModifiers.Shift):
                    return Keys.Shift;
                case (UserFriendlyModifiers.Control):
                    return Keys.Control;
                case (UserFriendlyModifiers.Alt):
                    return Keys.Alt;
                default:
                    return Keys.None;
            }
        }
    }

    public static class UserFriendlyModifiers
    {
        public const string None = "None";
        public const string Shift = "Shift";
        public const string Control = "Ctrl";
        public const string Alt = "Alt";

        public static List<string> AsList => new List<string>()
        {
            None, Shift, Control, Alt
        };
    }
}
