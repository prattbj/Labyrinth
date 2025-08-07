using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using Labyrinth.Menu.MenuItem;

namespace Labyrinth.Menu.MenuType
{
    public class DeathMenu : IMenu
    {
        private readonly MenuButton playButton;
        public DeathMenu()
        {
            playButton = new((int)Globals.GetScreenSize().X / 2, (int)Globals.GetScreenSize().Y / 2, "You Died. Play Again?");
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