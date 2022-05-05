using Sandbox.ModAPI.Ingame;
using System;
using System.Collections.Generic;

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

        private const int MAIN_MENU = 3;
        private const int AUTHENTICATING_MENU = 2;
        private const int LOGIN_MENU = 1;
        private const int LOGOUT_MENU = 4;
        private readonly Action<string> _logAction;
        private readonly List<Menu> _menus = new List<Menu>();
        private Menu _selectedMenu;

        public MenuNavigationSystem(Action<string> logAction)
        {
            _logAction = logAction;
            _menus.AddRange(new[]
            {
                new Menu(LOGIN_MENU)
                {
                    MenuItems = new[]
                    {
                        new MenuItem
                        {
                            Text = new[]
                                { "Please Login    ", "Please Login .  ", "Please Login .. ", "Please Login ..." },
                            Type = MenuItem.MenuItemTypes.Splash,
                            SelectAction = MenuItem.SelectActions.SubMenu,
                            SubMenuId = AUTHENTICATING_MENU
                        }
                    }
                },
                new Menu(AUTHENTICATING_MENU)
                {
                    MenuItems = new[]
                    {
                        new MenuItem
                        {
                            Text = new[]
                            {
                                ">",
                                "> _",
                                ">",
                                "> _",
                                ">",
                                "> _",
                                "> Authenticating",
                                "> Authenticating .",
                                "> Authenticating ..",
                                "> Authenticating ...",
                                "> Authenticating ....",
                                "> Authenticating .....",
                                "> Identity verified",
                                "> Identity verified _",
                                "> Identity verified",
                                "> Identity verified _",
                                "> Identity verified",
                                "> Identity verified _"
                            },
                            Type = MenuItem.MenuItemTypes.Text,
                            SelectAction = MenuItem.SelectActions.None,
                            AutoActionEndAnimation = () => GotoMenu(MAIN_MENU)
                        }
                    }
                },
                new Menu(LOGOUT_MENU)
                {
                    MenuItems = new[]
                    {
                        new MenuItem
                        {
                            Text = new[]
                            {
                                ">",
                                "> _",
                                ">",
                                "> _",
                                ">",
                                "> _",
                                "> Logging out",
                                "> Logging out .",
                                "> Logging out ..",
                                "> Logging out ...",
                                "> Logging out ....",
                                "> Logging out .....",
                                "",
                                "",
                                ""
                            },
                            Type = MenuItem.MenuItemTypes.Text,
                            SelectAction = MenuItem.SelectActions.None,
                            AutoActionEndAnimation = () => GotoMenu(LOGIN_MENU)
                        }
                    }
                },
                new Menu(MAIN_MENU)
                {
                    MenuItems = new[]
                    {
                        new MenuItem
                        {
                            Text = new[] { "Statistics" },
                            WhenSelectedText = new[] { "Statistics", "Statistics <" },
                            Type = MenuItem.MenuItemTypes.Navigatable,
                            SelectAction = MenuItem.SelectActions.None
                        },
                        new MenuItem
                        {
                            Text = new[] { "Inventory" },
                            WhenSelectedText = new[] { "Inventory", "Inventory <" },
                            Type = MenuItem.MenuItemTypes.Navigatable,
                            SelectAction = MenuItem.SelectActions.None
                        },
                        new MenuItem
                        {
                            Text = new[] { "Display" },
                            WhenSelectedText = new[] { "Display", "Display <" },
                            Type = MenuItem.MenuItemTypes.Navigatable,
                            SelectAction = MenuItem.SelectActions.None
                        },
                        new MenuItem
                        {
                            Text = new[] { "Logout" },
                            WhenSelectedText = new[] { "Logout", "Logout <" },
                            Type = MenuItem.MenuItemTypes.Navigatable,
                            SelectAction = MenuItem.SelectActions.SubMenu,
                            SubMenuId = LOGOUT_MENU
                        }
                    }
                }
            });

            GotoMenu(LOGIN_MENU);
        }

        private void GotoMenu(int menuId)
        {
            _selectedMenu?.Reset();
            _selectedMenu = _menus.Find(m => m.Id == menuId);
        }

        public void RunAction(ScriptActions action)
        {
            switch (action)
            {
                case ScriptActions.NavigationUp:
                    _selectedMenu.MoveSelection(-1);

                    break;
                case ScriptActions.NavigationDown:
                    _selectedMenu.MoveSelection(1);

                    break;
                case ScriptActions.NavigationSelect:
                    Select();

                    break;
                case ScriptActions.Normal: break;
            }
        }

        private void Select()
        {
            switch (_selectedMenu.SelectedMenuItem.SelectAction)
            {
                case MenuItem.SelectActions.SubMenu:
                    GotoMenu(_selectedMenu.SelectedMenuItem.SubMenuId);

                    break;
            }

            // var newIsActiveValue = !MenuItems[_selectedMenuIndex].IsActive;
            // for (var i = 0; i <= MenuItems.Length - 1; i++)
            //     MenuItems[i].IsActive = false;
            //
            // MenuItems[_selectedMenuIndex].IsActive = newIsActiveValue;
        }

        public void RenderMenu(IEnumerable<IMyTerminalBlock> blocks, MonitoringData monitoringMonitoringData)
        {
            var displayBlocks = GetDisplayBlocks(blocks, CustomDataSettings.GRID_MANAGER_MENU_ACCESS);

            displayBlocks.ForEach(block =>
            {
                _selectedMenu.Render(block);
            });
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
