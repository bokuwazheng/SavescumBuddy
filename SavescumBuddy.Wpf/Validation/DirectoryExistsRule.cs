using System.Globalization;
using System.IO;
using System.Windows.Controls;

namespace SavescumBuddy.Wpf.Validation
{
    public class DirectoryExistsRule : ValidationRule
    {
        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            if (value is string input && !string.IsNullOrWhiteSpace(input))
            {
                var exists = File.Exists(input) || Directory.Exists(input);

                return exists
                    ? ValidationResult.ValidResult
                    : new ValidationResult(false, "Path doesn't refer to an existing directory or file");
            }
            else
                return ValidationResult.ValidResult;
        }
    }
}
