using Sandbox.ModAPI.Ingame;
using System;
using System.Collections.Generic;
using System.Linq;

namespace IngameScript
{
    public class MenuNavigationSystem
    {
        private readonly Action<string> _logAction;

        public enum ScriptActions
        {
            NavigationUp,
            NavigationDown,
            NavigationSelect,
            Normal
        }

        private readonly List<Menu> _menus = new List<Menu>();
        private readonly Menu _selectedMenu;

        public MenuNavigationSystem(Action<string> logAction)
        {
            _logAction = logAction;
            _menus.Add(new Menu(1)
            {
                MenuItems = new[]
                {
                    new MenuItem
                    {
                        Id = 1,
                        Text = new [] {"Please Login    ", "Please Login .  ", "Please Login .. ", "Please Login ..."},
                        Type = MenuItem.MenuItemTypes.Splash
                    }
                }
            });

            _selectedMenu = _menus.First(m => m.Id == 1);
        }

        public void RunAction(ScriptActions action)
        {
            switch (action)
            {
                case ScriptActions.NavigationUp:
                    _selectedMenu.MoveSelection(-1);

                    break;
                case ScriptActions.NavigationDown:
                    _selectedMenu.MoveSelection(-1);

                    break;
                case ScriptActions.NavigationSelect:
                    _selectedMenu.Select();

                    break;
                case ScriptActions.Normal: break;
            }
        }

        public void RenderMenu(IEnumerable<IMyTerminalBlock> blocks, MonitoringData monitoringMonitoringData)
        {
            var displayBlocks = GetDisplayBlocks(blocks, "grid-manager-menu");

            displayBlocks.ForEach(block => _selectedMenu.Render(block));
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
}
