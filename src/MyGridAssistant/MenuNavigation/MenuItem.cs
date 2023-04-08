using System;

namespace MyGridAssistant
{
    public class MenuItem
    {
        public enum MenuItemTypes
        {
            Splash,
            Text,
            Navigatable
        }

        public enum SelectActions
        {
            SubMenu,
            None
        }

        private int _lastRenderedIndex;
        private DateTime _lastRenderTimer = DateTime.MinValue;

        public MenuItemTypes Type { get; set; }

        public string[] Text { get; set; }

        public SelectActions SelectAction { get; set; }

        public int SubMenuId { get; set; }

        public Action AutoActionEndAnimation { get; set; }

        public string[] WhenSelectedText { get; set; }

        public string GetNextTextFrame(bool isSelected)
        {
            string[] textToRender;

            if ((WhenSelectedText?.Length ?? 0) > 0 && isSelected)
                textToRender = WhenSelectedText;
            else
                textToRender = Text;

            var text = textToRender[_lastRenderedIndex];

            if (DateTime.Now.Subtract(_lastRenderTimer).TotalMilliseconds >= 500)
            {
                var newIndex = _lastRenderedIndex + 1;

                _lastRenderedIndex = newIndex > textToRender.Length - 1
                    ? 0
                    : newIndex;

                _lastRenderTimer = DateTime.Now;

                if (_lastRenderedIndex == 0 && AutoActionEndAnimation != null)
                    AutoActionEndAnimation();
            }

            return text;
        }

        public void ResetRendering()
        {
            _lastRenderedIndex = 0;
        }
    }
}
