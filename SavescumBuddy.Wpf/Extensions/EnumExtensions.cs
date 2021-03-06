﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows.Markup;

namespace SavescumBuddy.Wpf.Extensions
{
    public static class EnumExtensions
    {
        public static string ToDescriptionOrString(this Enum value)
            => value.GetType().GetField(value.ToString())
                        .GetCustomAttributes(typeof(DescriptionAttribute), false)
                        .Cast<DescriptionAttribute>()
                        .FirstOrDefault()?.Description ?? value.ToString();
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
}
