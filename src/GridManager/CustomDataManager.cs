﻿using System;
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

        public void AddValue(string key, string value)
        {
            if (_settings.ContainsKey(key))
                _settings[key] = value;
            else
                _settings.Add(key, value);
        }
    }
}