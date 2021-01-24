using System.Globalization;
using System.IO;
using System.Windows.Controls;

namespace SavescumBuddy.Wpf.Validation
{
    public class DirectoryIsNotReadOnlyRule : ValidationRule
    {
        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            if (value is string input && !string.IsNullOrWhiteSpace(input))
            {
                var isFolder = File.GetAttributes(input).HasFlag(FileAttributes.Directory);

                var folderPath = isFolder
                    ? input
                    : Path.GetDirectoryName(input);

                var isReadOnly = new DirectoryInfo(folderPath).Attributes.HasFlag(FileAttributes.ReadOnly);

                if (isReadOnly)
                {
                    return new ValidationResult(false,
                        isFolder
                        ? "Directory is read-only"
                        : "File directory is read-only");
                }
                else
                    return ValidationResult.ValidResult;
            }
            else
                return ValidationResult.ValidResult;
        }
    }
}
