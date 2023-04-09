using System;

namespace MyGridAssistant
{
    public class LogEntry
    {
        public DateTime DateCreated { get; set; }

        public string Message { get; set; }

        public int Expiry { get; set; }
    }
}
