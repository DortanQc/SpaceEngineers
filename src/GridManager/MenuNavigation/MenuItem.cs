using System;

namespace IngameScript
{
    public class MenuItem
    {
        public enum MenuItemTypes
        {
            Splash
        }

        private DateTime _lastRenderTimer = DateTime.MinValue;
        private int _lastRenderedIndex = 0;

        public MenuItemTypes Type { get; set; }

        public MenuItem[] SubMenu { get; set; }

        public int Id { get; set; }

        public string[] Text { get; set; }

        public string GetNextAnimationFrame()
        {
            var text = Text[_lastRenderedIndex];

            if (DateTime.Now.Subtract(_lastRenderTimer).TotalMilliseconds > 500)
            {
                var newIndex = _lastRenderedIndex + 1;

                _lastRenderedIndex = newIndex > Text.Length - 1
                    ? 0
                    : newIndex;

                _lastRenderTimer = DateTime.Now;
            }

            return text;
        }
    }
}
