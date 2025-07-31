using Raylib_cs;
using System.Numerics;
namespace Labyrinth.Menu.MenuItem
{
    class MenuButton(int posX, int posY, string text)
    {
        //private Texture2D texture;
        private Vector2 pos = new(posX, posY);
        private bool isPressed = false;
        private readonly string text = text;

        public void Draw()
        {
            //Raylib.DrawTexture(texture, (int)pos.X, (int)pos.Y, Color.White);
            Raylib.DrawText(text, (int)pos.X, (int)pos.Y, 32, Color.Brown);
        }
        private bool HitTest(Vector2 location)
        {
            return Raylib.CheckCollisionPointRec(location, new Rectangle(pos, 32 * text.Length, 32));
        }
        public void HandleInput()
        {
            if (Raylib.IsMouseButtonReleased(MouseButton.Left) && HitTest(Raylib.GetMousePosition()))
            {
                isPressed = true;
            }
        }

        public bool GetIsPressed()
        {
            return isPressed;
        }
    }
}