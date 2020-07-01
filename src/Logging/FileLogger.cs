using System;
using System.IO;

namespace LiteLoader.Logging
{
    public class FileLogger : ThreadedLogger, IFileLogger
    {
        public long MaxFileSize { get; }

        public TimeSpan DeleteAfter { get; }

        public string FilePath { get; protected set; }

        private StreamWriter _writer;

        public FileLogger(string fileName, long maxFileSize, TimeSpan deleteAfter)
        {
            MaxFileSize = maxFileSize;
            DeleteAfter = deleteAfter;

            if (string.IsNullOrEmpty(fileName))
            {
                throw new ArgumentNullException(nameof(fileName));
            }

            if (!fileName.StartsWith(Interface.CoreModule.FrameworkDirectory, StringComparison.OrdinalIgnoreCase))
            {
                throw new ArgumentException("Log files must be located under the Framework directory", nameof(fileName));
            }

            FilePath = Path.ChangeExtension(fileName, ".log");
        }

        protected override void BeginBatchProcess()
        {
            RotateLogs();
            if (!File.Exists(FilePath))
            {
                _writer = new StreamWriter(File.Open(FilePath, FileMode.Create, FileAccess.Write));
            }
            else
            {
                _writer = new StreamWriter(File.Open(FilePath, FileMode.Append, FileAccess.Write));
            }
        }

        protected override void EndBatchProcess()
        {
            _writer.Flush();
            _writer.Close();
            _writer.Dispose();
            _writer = null;
        }

        protected override void Write(LogMessage logMessage)
        {
            string message = $"({logMessage.Timestamp.Hour}:{logMessage.Timestamp.Minute}:{logMessage.Timestamp.Second})[{logMessage.Level.ToString().ToUpper()}] {logMessage.Message}";
            _writer.WriteLine(message);
        }

        private void RotateLogs()
        {
            DirectoryInfo directory = new DirectoryInfo(Path.GetDirectoryName(FilePath));
            if (!directory.Exists)
            {
                return;
            }

            FileInfo[] files = directory.GetFiles($"{Path.GetFileNameWithoutExtension(FilePath)}_*.log");
            DateTime now = DateTime.Now;
            for (int i = 0; i < files.Length; i++)
            {
                FileInfo f = files[i];
                if ((now - f.CreationTime) >= DeleteAfter)
                {
                    try
                    {
                        f.Delete();
                    }
                    catch (Exception)
                    {
                    }
                }
            }

            FileInfo file = new FileInfo(FilePath);
            if (!file.Exists)
            {
                return;
            }

            if ((now - file.CreationTime) >= DeleteAfter)
            {
                try
                {
                    file.Delete();
                }
                catch (Exception)
                {
                }
            }

            if (file.Length >= MaxFileSize)
            {
                string newName = Path.Combine(file.Directory.FullName, Path.GetFileNameWithoutExtension(file.FullName) + $"_{file.CreationTime.Year}{file.CreationTime.Month}{file.CreationTime.Day}{file.CreationTime.Hour}{file.CreationTime.Minute}{file.CreationTime.Second}.log");
                try
                {
                    file.MoveTo(newName);
                }
                catch (Exception)
                {
                }
            }
        }
    }
}
