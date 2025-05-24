using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.IO;

namespace PoDebateRap.ServerApi.Logging
{
    public class FileLoggerProvider : ILoggerProvider
    {
        private readonly string _filePath;
        private readonly ConcurrentDictionary<string, FileLogger> _loggers = new ConcurrentDictionary<string, FileLogger>();

        public FileLoggerProvider(string filePath)
        {
            _filePath = filePath;
            // Ensure the directory exists
            var logDirectory = Path.GetDirectoryName(filePath);
            if (!string.IsNullOrEmpty(logDirectory) && !Directory.Exists(logDirectory))
            {
                Directory.CreateDirectory(logDirectory);
            }
            // Clear the log file at the start of the application run
            if (File.Exists(_filePath))
            {
                File.WriteAllText(_filePath, string.Empty);
            }
        }

        public ILogger CreateLogger(string categoryName)
        {
            return _loggers.GetOrAdd(categoryName, name => new FileLogger(name, _filePath));
        }

        public void Dispose()
        {
            _loggers.Clear();
        }
    }

    public class FileLogger : ILogger
    {
        private readonly string _name;
        private readonly string _filePath;
        private static readonly object _lock = new object();

        public FileLogger(string name, string filePath)
        {
            _name = name;
            _filePath = filePath;
        }

        public IDisposable BeginScope<TState>(TState state)
        {
            return null;
        }

        public bool IsEnabled(LogLevel logLevel)
        {
            return true; // Log all levels
        }

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
        {
            if (!IsEnabled(logLevel))
            {
                return;
            }

            var logRecord = string.Format("{0} [{1}] {2}: {3} {4}",
                DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                logLevel.ToString(),
                _name,
                formatter(state, exception),
                exception != null ? exception.ToString() : "");

            lock (_lock)
            {
                File.AppendAllText(_filePath, logRecord + Environment.NewLine);
            }
        }
    }
}
