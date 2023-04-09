using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MyGridAssistant
{
    public interface IMyGridAssistantLogger
    {
        void LogDebug(string message, int expireInSecond = 5);
        void LogInfo(string logId, string message);
        void ShowLogs();
    }

    public class MyGridAssistantLogger : IMyGridAssistantLogger
    {
        private readonly Action<string> _log;
        private readonly Dictionary<string, string> _logDictionary;
        private List<LogEntry> _timedLogs;

        public MyGridAssistantLogger(Action<string> log)
        {
            _log = log;
            _timedLogs = new List<LogEntry>();
            _logDictionary = new Dictionary<string, string>();
        }

        public void LogDebug(string message, int expireInSecond = 5)
        {
            var logEntry = new LogEntry
            {
                DateCreated = DateTime.Now,
                Message = message,
                Expiry = expireInSecond
            };

            _timedLogs.Insert(0, logEntry);
        }

        public void LogInfo(string logId, string message)
        {
            if (_logDictionary.ContainsKey(logId))
                _logDictionary[logId] = message;
            else
                _logDictionary.Add(logId, message);
        }

        public void ShowLogs()
        {
            var now = DateTime.Now;

            _timedLogs = _timedLogs.Where(log => now.Subtract(log.DateCreated).Seconds <= log.Expiry).ToList();

            var debugLogs = _timedLogs.Select(log => log.Message);
            var logs = _logDictionary.Select(log => log.Value);

            var result = new StringBuilder(string.Join(Environment.NewLine, debugLogs))
                .AppendLine()
                .AppendLine("-----------")
                .Append(string.Join(Environment.NewLine, logs))
                .ToString();

            _log(result);
        }
    }
}
