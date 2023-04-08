using Sandbox.ModAPI.Ingame;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MyGridAssistant
{
    public class CustomDataManager
    {
        private readonly Dictionary<string, string> _settings;

        public CustomDataManager(string customData)
        {
            _settings = customData
                .Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None)
                .Where(row => row.Trim() != string.Empty)
                .ToDictionary(
                    s => s.Split('=')[0].ToLower().Trim(),
                    s =>
                    {
                        var setting = s.Split('=');

                        return setting.Length > 1
                            ? setting[1].ToLower().Trim()
                            : "";
                    });
        }

        public string GetPropertyValue(string key)
        {
            string value;

            return _settings.TryGetValue(key.ToLower().Trim(), out value)
                ? value
                : null;
        }

        public static void AddValue(IMyTerminalBlock block, string key, string value)
        {
            var sb = new StringBuilder(block.CustomData);

            sb.AppendLine(string.IsNullOrEmpty(value)
                ? $"{key}"
                : $"{key}={value}");

            block.CustomData = sb.ToString();
        }
    }
}
