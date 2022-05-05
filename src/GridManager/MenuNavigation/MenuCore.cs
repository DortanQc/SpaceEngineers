using Sandbox.ModAPI.Ingame;
using System;
using System.Linq;
using System.Text;
using VRage.Game.GUI.TextPanel;

namespace IngameScript
{
    public abstract class MenuCore
    {
        private int _selectedMenuIndex;

        public MenuItem[] MenuItems { get; set; }

        private int TotalItemInMenu => MenuItems.Length;

        public void MoveSelection(int direction)
        {
            var newPosition = _selectedMenuIndex + direction;

            if (newPosition > TotalItemInMenu - 1 || newPosition < 0)
                return;

            _selectedMenuIndex = newPosition;
        }

        public void Select()
        {
            // var newIsActiveValue = !MenuItems[_selectedMenuIndex].IsActive;
            // for (var i = 0; i <= MenuItems.Length - 1; i++)
            //     MenuItems[i].IsActive = false;
            //
            // MenuItems[_selectedMenuIndex].IsActive = newIsActiveValue;
        }

        public void Render(IMyTextSurface block)
        {
            if (MenuItems.Any(m => m.Type == MenuItem.MenuItemTypes.Splash))
            {
                DisplaySplash(block);
                return;
            }

            for (var i = 0; i <= TotalItemInMenu - 1; i++)
            {
                // if (i == _selectedMenuIndex)
                //     textBuilder.Append(MenuItems[i].IsActive
                //         ? "> [X] "
                //         : ">   ");
                // else
                //     textBuilder.Append(MenuItems[i].IsActive
                //         ? " [X] "
                //         : "   ");
                //
                // textBuilder.AppendLine(MenuItems[i].Text());
            }
        }

        private void DisplaySplash(IMyTextSurface block)
        {
            var splash = MenuItems.First(m => m.Type == MenuItem.MenuItemTypes.Splash);

            block.Alignment = TextAlignment.CENTER;
            block.ContentType = ContentType.TEXT_AND_IMAGE;
            block.FontSize = 1;
            block.TextPadding = 50;

            block.WriteText(splash.GetNextAnimationFrame());
        }
    }
}
