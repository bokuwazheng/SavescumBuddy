using SavescumBuddy.Core.Extensions;
using System.Globalization;
using System.IO;
using System.Windows.Controls;

namespace SavescumBuddy.Wpf.Validation
{
    public class ProperPathRule : ValidationRule
    {
        public bool IsFilePath { get; set; }

        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            var input = value as string;

            if (string.IsNullOrWhiteSpace(input))
                return ValidationResult.ValidResult;

            string errorMessage;

            var exists = File.Exists(input) || Directory.Exists(input);

            if (!exists)
            {
                errorMessage = IsFilePath
                    ? "File doesn't exist"
                    : "Directory doesn't exists";

                return exists
                    ? new ValidationResult(true, null)
                    : new ValidationResult(false, errorMessage);
            }

            var isWritable = Path.GetDirectoryName(input).IsDirectoryWritable() || input.IsDirectoryWritable();

            if (!isWritable)
            {
                errorMessage = IsFilePath
                    ? "File directory is not writable"
                    : "Directory is not writable";

                return exists
                    ? new ValidationResult(true, null)
                    : new ValidationResult(false, errorMessage);
            }

            return ValidationResult.ValidResult;
        }
    }
}
