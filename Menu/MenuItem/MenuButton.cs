using Raylib_cs;
using System.Numerics;
namespace Labyrinth.Menu.MenuItem
{
    class MenuButton
    {
        private Texture2D texture;
        private Vector2 pos;
        private bool isPressed = false;
        public MenuButton(int posX, int posY, Image image)
        {
            pos = new(posX, posY);
            texture = Raylib.LoadTextureFromImage(image);
        }
        public MenuButton(int posX, int posY, string text)
        {
            pos = new(posX, posY);

            Image image = Raylib.LoadImage("../Assets/blank.png");
            Raylib.ImageResize(ref image, 256, 256);
            Raylib.ImageDrawText(ref image, text, 0, 0, 32, Color.Brown);
            texture = Raylib.LoadTextureFromImage(image);
            Raylib.UnloadImage(image);
        }

        public void Draw()
        {
            Raylib.DrawTexture(texture, (int)pos.X, (int)pos.Y, Color.White);
        }
        private bool HitTest(Vector2 location)
        {
            return Raylib.CheckCollisionPointRec(location, new Rectangle(pos, texture.Width, texture.Height));
        }
        public void HandleInput()
        {
            if (Raylib.IsMouseButtonDown(MouseButton.Left) && HitTest(Raylib.GetMousePosition()))
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