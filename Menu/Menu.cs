
using System.Collections;
using System.Numerics;
using System.Reflection;
using System.Security.Cryptography.X509Certificates;
using System.Configuration;
using Raylib_cs;
using Labyrinth.Menu.MenuType;
namespace Labyrinth.Menu
{
    static class Menu
    {
        private static IMenu? currentMenu = new MainMenu();
        public static void Draw()
        {
            currentMenu?.Draw();
        }

        public static void HandleInput()
        {
            currentMenu?.HandleInput();
        }

        public static void PerformActions()
        {
            currentMenu?.PerformActions();
        }
        public static void SetCurrentMenu(IMenu? menu)
        {
            currentMenu = menu;
        }

        public static bool IsMenuActive()
        {
            return currentMenu != null;
        }
    }
}