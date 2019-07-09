﻿using System.Globalization;
using System.IO;
using System.Windows.Controls;

namespace SavescumBuddy
{
    public class ProperPathRule : ValidationRule
    {
        public bool IsFilePath { get; set; }

        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            var input = "";

            if (((string)value).Length > 0)
            {
                input = (string)value;
            }

            if (IsFilePath)
            {
                if (File.Exists(input) && Util.IsDirectoryWritable(Path.GetDirectoryName(input)))
                {
                    return new ValidationResult(true, null);
                }
                else
                {
                    return new ValidationResult(false, null);
                }
            }
            else
            {
                if (Directory.Exists(input) && Util.IsDirectoryWritable(input))
                {
                    return new ValidationResult(true, null);
                }
                else
                {
                    return new ValidationResult(false, null);
                }
            }
        }
    }
}
