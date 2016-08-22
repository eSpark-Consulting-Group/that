using Microsoft.Win32;
using System.IO;

namespace DDT.Application.Helpers
{
    public class DialogHelper
    {
        public static DialogHelper Instance { get; private set; }

        DialogHelper()
        {
        }

        static DialogHelper()
        {
            Instance = new DialogHelper();
        }

        public string OpenFileDialog(string filter, out string fileSize, bool checkFileExists = true)
        {
            fileSize = "0 bytes";
            OpenFileDialog dialog = new OpenFileDialog
            {
                AddExtension = true,
                CheckFileExists = checkFileExists,
                CheckPathExists = checkFileExists,
                Filter = filter,
                Multiselect = false
            };

            bool? nullable = dialog.ShowDialog();
            if (nullable.HasValue && nullable.Value)
            {
                string[] sizes = { "bytes", "Kbytes", "Mbytes" };
                double len = new FileInfo(dialog.FileName).Length;
                int order = 0;
                while (len >= 1024 && ++order < sizes.Length)
                {
                    len = len / 1024;
                }

                fileSize = string.Format("{0:0.##} {1}", len, sizes[order]);

                return dialog.FileName;
            }
            return null;
        }
    }
}
