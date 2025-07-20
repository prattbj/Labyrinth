using Raylib_cs;

namespace Labyrinth.Menu.MenuType
{
    interface IMenu
    {

        public void Draw();
        public void HandleInput();

        public void PerformActions();
    }
}