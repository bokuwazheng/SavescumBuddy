using System.Collections.Generic;

namespace SavescumBuddy.Wpf.Services
{
    public interface IOpenFileService
    {
        bool OpenFile();
        public string FileName { get; }
        IEnumerable<string> FileNames { get; }
        bool IsFolderPicker { get; set; }
        bool ShowHiddenItems { get; set; }
        bool Multiselect { get; set; }
    }
}
