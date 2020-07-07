using System;
using System.ComponentModel;
using System.Linq;
using System.Windows.Markup;

namespace SavescumBuddy.MarkupExtensions
{
    public class EnumToCollectionExtension : MarkupExtension
    {
        public Type EnumType { get; set; }

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            if (EnumType is null) 
                throw new ArgumentNullException(nameof(EnumType));

            return Enum.GetValues(EnumType).Cast<Enum>().Select(EnumToDescriptionOrString);
        }

        public static string EnumToDescriptionOrString(Enum value)
        {
            return value.GetType().GetField(value.ToString())
                        .GetCustomAttributes(typeof(DescriptionAttribute), false)
                        .Cast<DescriptionAttribute>()
                        .FirstOrDefault()?.Description ?? value.ToString();
        }
    }

    public static class StringExtensions
    {
        public static bool EqualsEnumDescription(this string description, Enum value)
        {
            return description == EnumToCollectionExtension.EnumToDescriptionOrString(value);
        }
    }
}
