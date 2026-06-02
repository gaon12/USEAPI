using System;
using System.IO;
using System.Text;

namespace USEAPI
{
    internal static class ErrorLog
    {
        private static readonly string LogPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "USEAPI",
            "error.log");

        public static void Write(Exception exception)
        {
            if (exception == null)
            {
                return;
            }

            try
            {
                Directory.CreateDirectory(Path.GetDirectoryName(LogPath));
                File.AppendAllText(
                    LogPath,
                    string.Format("[{0:yyyy-MM-dd HH:mm:ss zzz}] {1}\r\n\r\n", DateTimeOffset.Now, exception),
                    Encoding.UTF8);
            }
            catch
            {
                // Logging must not create a second user-facing failure.
            }
        }
    }
}
