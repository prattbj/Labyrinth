using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using Labyrinth.Menu.MenuItem;

namespace Labyrinth.Menu.MenuType
{
    public class MainMenu : IMenu
    {
        private readonly MenuButton playButton;
        public MainMenu()
        {
            playButton = new((int)Globals.GetScreenSize().X / 2, (int)Globals.GetScreenSize().Y / 2, "Play");
        }
        public void Draw()
        {
            playButton.Draw();
        }

        public void HandleInput()
        {
            playButton.HandleInput();
        }

        public void PerformActions()
        {
            if (playButton.GetIsPressed())
            {
                Menu.SetCurrentMenu(null);
                Globals.SetGame(new Game.Game());
            }
        }
    }
}