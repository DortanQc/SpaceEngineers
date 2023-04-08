using Sandbox.ModAPI.Ingame;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MyGridAssistant
{
    public interface IConfiguration
    {
        string GetConfig(string key);
        void SetConfig(string key, string value);
    }

    public class Configuration : IConfiguration
    {
        private readonly IMyTerminalBlock _block;
        private Dictionary<string, string> _settings;

        public Configuration(IMyTerminalBlock block)
        {
            _block = block;
        }

        string IConfiguration.GetConfig(string key)
        {
            Refresh(_block.CustomData);

            string value;

            return _settings.TryGetValue(key.ToLower().Trim(), out value)
                ? value
                : null;
        }

        void IConfiguration.SetConfig(string key, string value)
        {
            var sb = new StringBuilder(_block.CustomData);

            sb.AppendLine(string.IsNullOrEmpty(value)
                ? $"{key}"
                : $"{key}={value}");

            _block.CustomData = sb.ToString();
        }

        public static string GetBlockConfiguration(IMyTerminalBlock block, string key)
        {
            IConfiguration config = new Configuration(block);

            return config.GetConfig(key);
        }

        private void Refresh(string customData)
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
    }
}
