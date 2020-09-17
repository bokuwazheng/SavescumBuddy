using SavescumBuddy.Core.Extensions;
using System.Globalization;
using System.IO;
using System.Windows.Controls;

namespace SavescumBuddy.CustomControls.Validation
{
    public class ProperPathRule : ValidationRule
    {
        public bool IsFilePath { get; set; }

        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            var input = value as string;

            if (input is null)
                return new ValidationResult(false, null);

            var exists = (File.Exists(input) && Path.GetDirectoryName(input).IsDirectoryWritable()) 
                || (Directory.Exists(input) && input.IsDirectoryWritable());

            return exists 
                ? new ValidationResult(true, null)
                : new ValidationResult(false, null);
        }
    }
}
