using System;
using System.Collections.Generic;
using System.Linq;

namespace IngameScript
{
    public class CustomDataManager
    {
        private readonly Dictionary<string, string> _settings;

        public CustomDataManager()
        {
            _settings = new Dictionary<string, string>();
        }

        public CustomDataManager(string customData)
        {
            _settings = customData.Split(
                    new[] { "\r\n", "\r", "\n" },
                    StringSplitOptions.None)
                .ToDictionary(
                    s => s.Split('=')[0].Trim(),
                    s =>
                    {
                        var setting = s.Split('=');

                        return setting.Length > 1
                            ? setting[1].Trim()
                            : null;
                    });
        }

        public string GetStringProperty(string key)
        {
            string value;

            return _settings.TryGetValue(key, out value)
                ? value
                : null;
        }

        public void AddValue(string key, string value)
        {
            _settings.Add(key, value);
        }
    }
}
