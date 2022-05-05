using Sandbox.ModAPI.Ingame;
using System.Collections.Generic;
using System.Text;

namespace IngameScript
{
    public class MenuNavigationSystem
    {
        public enum ScriptActions
        {
            NavigationUp,
            NavigationDown,
            NavigationSelect,
            Normal
        }

        private readonly MenuItem[] _menuItems;
        private int _selectedMenuIndex;

        public MenuNavigationSystem()
        {
            _menuItems = new[]
            {
                new MenuItem("Menu Item 1"),
                new MenuItem("Menu Item 2"),
                new MenuItem("Menu Item 3")
            };
        }

        private int TotalItemInMenu => _menuItems.Length;

        public void RunAction(ScriptActions action)
        {
            switch (action)
            {
                case ScriptActions.NavigationUp:
                    MoveSelectedIndex(-1);

                    break;
                case ScriptActions.NavigationDown:
                    MoveSelectedIndex(1);

                    break;
                case ScriptActions.NavigationSelect:
                    SetSelectedItem();

                    break;
                case ScriptActions.Normal: break;
            }
        }

        private void SetSelectedItem()
        {
            var newIsActiveValue = !_menuItems[_selectedMenuIndex].IsActive;
            for (var i = 0; i <= _menuItems.Length - 1; i++)
                _menuItems[i].IsActive = false;

            _menuItems[_selectedMenuIndex].IsActive = newIsActiveValue;
        }

        private void MoveSelectedIndex(int direction)
        {
            var newPosition = _selectedMenuIndex + direction;

            if (newPosition > TotalItemInMenu - 1 || newPosition < 0)
                return;

            _selectedMenuIndex = newPosition;
        }

        public void DisplayMenu(IEnumerable<IMyTerminalBlock> blocks)
        {
            var displayBlocks = GetDisplayBlocks(blocks, "grid-manager-menu");

            var textBuilder = new StringBuilder();

            for (var i = 0; i <= _menuItems.Length - 1; i++)
            {
                if (i == _selectedMenuIndex)
                    textBuilder.Append(_menuItems[i].IsActive
                        ? "> [X] "
                        : ">   ");
                else
                    textBuilder.Append(_menuItems[i].IsActive
                        ? " [X] "
                        : "   ");

                textBuilder.AppendLine(_menuItems[i].Text());
            }

            displayBlocks.ForEach(block => block.WriteText(textBuilder));
        }

        private static List<IMyTextSurface> GetDisplayBlocks(
            IEnumerable<IMyTerminalBlock> blocks,
            string customDataKeyToLookup)
        {
            var result = new List<IMyTextSurface>();

            foreach (var block in blocks)
            {
                var customDataManager = new CustomDataManager(block.CustomData);
                var customData = customDataManager.GetPropertyValue(customDataKeyToLookup);

                if (customData == null) continue;

                var index = 0;
                if (customData.Length > 0)
                    int.TryParse(customData, out index);

                var textSurface = block as IMyTextSurfaceProvider;

                if (textSurface == null || textSurface.SurfaceCount <= index) continue;

                result.Add(textSurface.GetSurface(index));
            }

            return result;
        }
    }

    internal class MenuItem
    {
        private readonly string _text;

        public MenuItem(string text)
        {
            _text = text;
        }

        public bool IsActive { get; set; }

        public string Text() =>
            _text;
    }
}
