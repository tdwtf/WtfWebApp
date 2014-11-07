using System;
using System.IO;
using Inedo.Diagnostics;

namespace TheDailyWtf.Logs
{
    internal sealed class FileSystemMessenger : IMessenger
    {
        private string logDirectory;
        private MessageLevel minimumLevel;

        public FileSystemMessenger(string baseDirectory, MessageLevel minimumLevel)
        {
            if (baseDirectory == null)
                throw new ArgumentNullException("baseDirectory");

            this.logDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, baseDirectory);
            this.minimumLevel = minimumLevel;
        }

        public void Message(IMessage message)
        {
            if (message.Level < this.minimumLevel)
                return;

            string path = this.GetLogFilePath();
            string text = string.Format("{0}\t- {1}\t - {2}\r\n", message.Level, DateTime.Now.ToString("s"), message.Message);

            File.AppendAllText(path, text);
        }

        private string GetLogFilePath()
        {
            string fileName = string.Format("{0}.log", DateTime.Now.ToString("yyyy-MM-dd"));
            return Path.Combine(this.logDirectory, fileName);
        }

        void IMessenger.Initialize() { }
        void IMessenger.Terminate() { }
    }
}