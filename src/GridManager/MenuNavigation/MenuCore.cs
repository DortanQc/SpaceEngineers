using Sandbox.ModAPI.Ingame;
using System.Linq;
using System.Text;
using VRage.Game.GUI.TextPanel;

namespace IngameScript
{
    public abstract class MenuCore
    {
        private int _selectedMenuIndex;

        public MenuItem[] MenuItems { get; set; }

        public MenuItem SelectedMenuItem => MenuItems[_selectedMenuIndex];

        public void MoveSelection(int direction)
        {
            MenuItems.ToList().ForEach(i => i.ResetRendering());

            var totalNavigatableItem = MenuItems.Count(i => i.Type == MenuItem.MenuItemTypes.Navigatable);
            var newPosition = _selectedMenuIndex + direction;

            if (newPosition > totalNavigatableItem - 1 || newPosition < 0)
                return;

            _selectedMenuIndex = newPosition;
        }

        public void Render(IMyTextSurface block)
        {
            if (MenuItems.Any(m => m.Type == MenuItem.MenuItemTypes.Splash))
            {
                DisplaySplash(block);

                return;
            }

            var textBuilder = new StringBuilder();
            block.Alignment = TextAlignment.LEFT;
            block.ContentType = ContentType.TEXT_AND_IMAGE;
            block.FontSize = 1;
            block.TextPadding = 4;

            var index = -1;

            MenuItems
                .ToList()
                .ForEach(menuItem =>
                {
                    switch (menuItem.Type)
                    {
                        case MenuItem.MenuItemTypes.Text:
                            RenderText(menuItem, textBuilder);

                            break;
                        case MenuItem.MenuItemTypes.Navigatable:
                            index++;
                            RenderNavigatableText(index, menuItem, textBuilder);

                            break;
                    }
                });

            block.WriteText(textBuilder);
        }

        public void Reset()
        {
            MenuItems.ToList().ForEach(i => i.ResetRendering());
            _selectedMenuIndex = 0;
        }

        private static void RenderText(MenuItem menuItem, StringBuilder sb)
        {
            sb.AppendLine(menuItem.GetNextTextFrame(false));
        }

        private void RenderNavigatableText(int itemIndex, MenuItem menuItem, StringBuilder sb)
        {
            sb.AppendLine(menuItem.GetNextTextFrame(itemIndex == _selectedMenuIndex));
        }

        private void DisplaySplash(IMyTextSurface block)
        {
            var splash = MenuItems.First(m => m.Type == MenuItem.MenuItemTypes.Splash);

            block.Alignment = TextAlignment.CENTER;
            block.ContentType = ContentType.TEXT_AND_IMAGE;
            block.FontSize = 1;
            block.TextPadding = 50;

            block.WriteText(splash.GetNextTextFrame(false));
        }
    }
}
