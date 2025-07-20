using System.Numerics;
using Labyrinth;
using Labyrinth.Menu;
using Labyrinth.Game;

using Raylib_cs;
using System.Diagnostics;
using System.Runtime.InteropServices.JavaScript;

namespace Labyrinth
{
    public static class Globals
    {
        private static Vector2 screenSize = new(1920, 1080);
        public static ref Vector2 GetScreenSize()
        {
            return ref screenSize;
        }
        private static Game.Game? game;

        public static void SetGame(Game.Game? newGame)
        {
            game = newGame;
        }
        public static Game.Game? GetGame()
        {
            return game;
        }

        private static readonly int tickRate = 240;
        public static int GetTickRate()
        {
            return tickRate;
        }
    }

    #if WASM
        partial class Program
        {
            static private float timeStep = 1.0f / (float)Globals.GetTickRate();
            static private float accumulator = 0.0f;
            
            // public static async Task Main(string[] args)
            // {
            //     Action<string[]> init = args => {
            //         Raylib.InitWindow(int.Parse(args[0]), int.Parse(args[1]), "Labyrinth");
            //         Raylib.SetTargetFPS(60);
            //         Globals.GetScreenSize().X = Raylib.GetScreenWidth();
            //         Globals.GetScreenSize().Y = Raylib.GetScreenHeight();
            //         Raylib.SetExitKey(KeyboardKey.Null);
            //         Console.WriteLine($"Initialized with size {int.Parse(args[0])}, {int.Parse(args[1])}");
            //     };
            //     while (!Raylib.IsWindowReady())
            //     {
            //         try
            //         {
            //             init(args);
            //         }
            //         catch (Exception ex)
            //         {
            //             Console.WriteLine("Failed to initialize window. Retrying.");
            //             throw;
            //         }
            //     }
            //     Console.WriteLine("Window Initialized.");
                
                
            //     await Task.Delay(-1);
            // }
            public static void Main(string[] args)
            {
                Action<string[]> init = args => {
                    Raylib.InitWindow(int.Parse(args[0]), int.Parse(args[1]), "Labyrinth");
                    Raylib.SetTargetFPS(60);
                    Globals.GetScreenSize().X = Raylib.GetScreenWidth();
                    Globals.GetScreenSize().Y = Raylib.GetScreenHeight();
                    Raylib.SetExitKey(KeyboardKey.Null);
                    Console.WriteLine($"Initialized with size {int.Parse(args[0])}, {int.Parse(args[1])}");
                };
                while (!Raylib.IsWindowReady())
                {
                    try
                    {
                        init(args);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("Failed to initialize window. Retrying.");
                        throw;
                    }
                }
                Console.WriteLine("Window Initialized.");
                
                
            }
            [JSExport]
            public static void RunGame()
            {
                if (Raylib.IsWindowReady())
                {
                    float frameTime = Raylib.GetFrameTime();
                    accumulator += frameTime;
                    while (accumulator >= frameTime)
                    {
                        Menu.Menu.HandleInput();
                        Menu.Menu.PerformActions();
                        if (!Menu.Menu.IsMenuActive())
                        {
                            Globals.GetGame()?.HandleInput();
                            Globals.GetGame()?.PerformActions();
                        }
                        accumulator -= timeStep;
                    }
                    Raylib.BeginDrawing();
                    Raylib.ClearBackground(Color.Black);

                    Globals.GetGame()?.Draw();
                    if (Menu.Menu.IsMenuActive())
                    {
                        Raylib.DrawRectangle(0, 0, (int)Globals.GetScreenSize().X, (int)Globals.GetScreenSize().Y, new Color(0, 0, 0, 180));
                    }
                    Menu.Menu.Draw();
                    
                    Raylib.DrawFPS(0, 0);
                    Raylib.EndDrawing();
                }
                else
                {
                    Console.WriteLine("Window Not Ready");
                }
            }
        }
    #else
        class Program
        {
            static private float timeStep = 1.0f / (float)Globals.GetTickRate();
            static private float accumulator = 0.0f;
            // STAThread is required if you deploy using NativeAOT on Windows - See https://github.com/raylib-cs/raylib-cs/issues/301
            [STAThread]
            public static void Main()
            {
                Raylib.SetConfigFlags(ConfigFlags.FullscreenMode);

                Raylib.InitWindow(0, 0, "Labyrinth");
                Raylib.SetTargetFPS(60);
                Globals.GetScreenSize().X = Raylib.GetScreenWidth();
                Globals.GetScreenSize().Y = Raylib.GetScreenHeight();
                Raylib.SetExitKey(KeyboardKey.Null);
                
                while (!Raylib.WindowShouldClose())
                {
                    float frameTime = Raylib.GetFrameTime();
                    accumulator += frameTime;
                    while (accumulator >= frameTime)
                    {
                        Menu.Menu.HandleInput();
                        Menu.Menu.PerformActions();
                        if (!Menu.Menu.IsMenuActive())
                        {
                            Globals.GetGame()?.HandleInput();
                            Globals.GetGame()?.PerformActions();
                        }
                        accumulator -= timeStep;
                    }
                    Raylib.BeginDrawing();
                    Raylib.ClearBackground(Color.Black);
                    Globals.GetGame()?.Draw();
                    if (Menu.Menu.IsMenuActive())
                    {
                        Raylib.DrawRectangle(0, 0, (int)Globals.GetScreenSize().X, (int)Globals.GetScreenSize().Y, new Color(0, 0, 0, 180));
                    }
                    Menu.Menu.Draw();
                    Raylib.DrawFPS(0, 0);
                    Raylib.EndDrawing();
                }

                Raylib.CloseWindow();
            }
        }
    #endif
}