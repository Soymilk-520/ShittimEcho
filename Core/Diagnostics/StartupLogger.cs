using System;
using System.Diagnostics;
using System.IO;
using System.Security.Principal;

namespace ShittimEcho.Core.Diagnostics
{
    public sealed class StartupLogger
    {
        private readonly string _logFilePath;
        private readonly object _lock = new();

        public StartupLogger()
        {
            string logDirectory = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "ShittimEcho",
                "Logs");

            Directory.CreateDirectory(logDirectory);

            string fileName = $"{DateTime.Now:yyyy-MM-dd_HHmmss}.log";
            _logFilePath = Path.Combine(logDirectory, fileName);
        }

        public string LogFilePath => _logFilePath;

        public void Write(string message)
        {
            string line =
                $"[{DateTime.Now:HH:mm:ss.fff}] {message}";

            lock (_lock)
            {
                File.AppendAllText(
                    _logFilePath,
                    line + Environment.NewLine);
            }

            Debug.WriteLine(line);
        }

        public void WriteSystemInformation()
        {
            Process process = Process.GetCurrentProcess();

            Write("========================================");
            Write("Nyaruru Fishy Fight");
            Write("Version: 0.1.0 Alpha - Genesis");
            Write($"Process ID: {process.Id}");
            Write($"Session ID: {process.SessionId}");
            Write($"User: {WindowsIdentity.GetCurrent().Name}");
            Write($"OS: {Environment.OSVersion}");
            Write($"64-bit OS: {Environment.Is64BitOperatingSystem}");
            Write($"64-bit Process: {Environment.Is64BitProcess}");
            Write("System initialization started.");
            Write("========================================");
        }
    }
}