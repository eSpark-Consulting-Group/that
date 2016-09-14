using System.Diagnostics;

namespace DDT.Core.Helper
{
    public class ProcessHelper : IProcessHelper
    {
        public bool IsProcessExist(string path)
        {
            var result = true;
            Process processNmap = null;

            try
            {
                processNmap = new Process
                {
                    StartInfo =
                    {
                        CreateNoWindow = true,
                        WindowStyle = ProcessWindowStyle.Hidden,
                        UseShellExecute = false,
                        RedirectStandardOutput = true,
                        FileName = path,
                        Arguments = "-sP"
                    }
                };

                processNmap.Start();
            }
            catch
            {

                result = false;
            }
            finally
            {
                if (processNmap != null && result)
                {
                    processNmap.Close();
                }
            }

            return result;
        }
    }
}
