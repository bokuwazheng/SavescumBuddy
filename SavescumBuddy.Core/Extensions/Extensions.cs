using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Windows.Markup;

namespace SavescumBuddy.Core.Extensions
{
    public static class StringExtensions
    {
        public static bool IsDirectoryWritable(this string value, bool throwIfFails = false)
        {
            try
            {
                using var fs = File.Create(Path.Combine(value, Path.GetRandomFileName()), 1, FileOptions.DeleteOnClose);
                return true;
            }
            catch
            {
                if (throwIfFails)
                    throw new Exception($"Selected directory ({ value }) is read-only. Please select other directory.");
                else
                    return false;
            }
        }

        public static bool EqualsEnumDescription(this string description, Enum value)
            => description == value.ToDescriptionOrString();
    }

    public static class EnumExtensions
    {
        public static string ToDescriptionOrString(this Enum value)
            => value.GetType().GetField(value.ToString())
                        .GetCustomAttributes(typeof(DescriptionAttribute), false)
                        .Cast<DescriptionAttribute>()
                        .FirstOrDefault()?.Description ?? value.ToString();
    }

    public static class DateTimeExtensions
    {
        public static string ToWindowsFriendlyFormat(this DateTime dateTime)
            => dateTime.ToString("dd.MM.yyyy--HH.mm.ss");

        public static string ToUserFriendlyFormat(this DateTime dateTime, IFormatProvider formatProvider)
            => dateTime.ToString("MMM dd, yyyy h:mm:ss tt", formatProvider);
    }

    public class EnumToCollectionExtension : MarkupExtension
    {
        public Type EnumType { get; set; }

        public override object ProvideValue(IServiceProvider serviceProvider)
            => EnumType is null
            ? throw new ArgumentNullException(nameof(EnumType))
            : Enum.GetValues(EnumType).Cast<Enum>().Select(x => x.ToDescriptionOrString());
    }

    public class EnumToValueDescriptionPairListExtension : MarkupExtension
    {
        public Type EnumType { get; set; }

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            if (EnumType is null)
                throw new ArgumentNullException(nameof(EnumType));

            var strings = Enum.GetValues(EnumType).Cast<Enum>().Select(x => x.ToDescriptionOrString()).ToArray();
            var values = Enum.GetValues(EnumType).Cast<int>().ToArray();

            var result = new List<ValueDescriptionPair>();

            for (var i = 0; i < values.Count(); i++)
                result.Add(new ValueDescriptionPair(values[i], strings[i]));

            return result;
        }
    }

    public class ValueDescriptionPair
    {
        public ValueDescriptionPair(int value, string description)
        {
            Value = value;
            Description = description;
        }

        public int Value { get; set; }
        public string Description { get; set; }
    }
}
