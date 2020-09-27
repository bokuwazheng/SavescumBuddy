using SavescumBuddy.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace SavescumBuddy.Services
{
    public class OpenFileService : IOpenFileService
    {
        private bool disposedValue;

        private string _fileName;
        private IEnumerable<string> _fileNames;

        public string FileName => _fileName;
        public IEnumerable<string> FileNames => _fileNames;

        public bool IsFolderPicker { get; set; }
        public bool ShowHiddenItems { get; set; }
        public bool Multiselect { get; set; }

        public bool OpenFile()
        {
            if (IsFolderPicker)
            {
                var fbd = new FolderBrowserDialog();
                var result = fbd.ShowDialog();
                if (result == DialogResult.OK)
                {
                    _fileName = fbd.SelectedPath;
                    return true;
                }
                else
                    return false;
            }
            else
            {
                var ofd = new OpenFileDialog() { Multiselect = Multiselect };
                var result = ofd.ShowDialog();
                if (result == DialogResult.OK)
                {
                    if (ofd.Multiselect)
                        _fileNames = ofd.FileNames;
                    else
                        _fileName = ofd.FileName;

                    return true;
                }
                else
                    return false;
            }
        }
    }
}
