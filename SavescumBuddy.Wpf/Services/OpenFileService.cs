using System.Collections.Generic;
using System.Windows.Forms;
using DialogResult = System.Windows.Forms.DialogResult;

namespace SavescumBuddy.Wpf.Services
{
    public class OpenFileService : IOpenFileService
    {
        public string FileName { get; private set; }
        public IEnumerable<string> FileNames { get; private set; }

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
                    FileName = fbd.SelectedPath;
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
                        FileNames = ofd.FileNames;
                    else
                        FileName = ofd.FileName;

                    return true;
                }
                else
                    return false;
            }
        }
    }
}
