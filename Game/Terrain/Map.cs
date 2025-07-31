using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Numerics;
using MIConvexHull;
//using System.Drawing;
using Raylib_cs;
using Labyrinth.Game.Entities.Players;
using System.CodeDom.Compiler;
namespace Labyrinth.Game.Terrain
{

    public class Map
    {
        int spawnCellKey;
        Dictionary<int, Cell> cells = [];
        List<(Vector2, Vector2)> path;
        Rectangle bounds;
        public Map()
        {

            int width = 1 << 16;
            int height = 1 << 16; //change last number to make it bigger or smaller
            bounds = new(0, 0, width, height);
            (List<Generation.CellInitializer>, Generation.CellInitializer, List<(Vector2, Vector2)>) generated = Generation.MapGenerator.GenerateMap(new(0, 0, width, height), Generation.MapGenerator.Algorithm.Prim);
            path = generated.Item3;
            foreach (Generation.CellInitializer cell in generated.Item1)
            {
                cells.Add(cell.Index, new(cell, cells));
            }
            foreach (var c in cells)
            {
                c.Value.InitializeLines();
                c.Value.CreateEdges();
            }
            spawnCellKey = generated.Item2.Index;

        }
        // public List<(List<Vector2> polygon, Color color)> GetWallEdges()
        // {
        //     return grid.GetWallEdges();
        // } 
        public Cell GetCell(int index)
        {
            return cells[index];
        }
        public Cell[] GetCells()
        {
            return [.. cells.Values];
        }
        public int GetSpawnCellKey()
        {
            return spawnCellKey;
        }
        public Rectangle GetBounds()
        {
            return bounds;
        }
        public List<(Vector2, Vector2)> GetPath()
        {
            return path;
        }
        // public void Debug(Camera2D camera)
        // {
        //     grid.Debug(camera);
        // }
        public void Draw()
        {
            int? current = Globals.GetGame()?.GetPlayer()?.GetCurrentCellKey();
            if (current != null)
            {
                Cell currentCell = cells[current.Value];
                //Raylib.DrawText(currentCell.GetSection().ToString(), (int?)Globals.GetGame()?.GetPlayer()?.GetPos().X ?? 0, (int?)Globals.GetGame()?.GetPlayer()?.GetPos().Y ?? 0, 24, Color.Beige);
                currentCell.Draw();
            }
            // foreach (var p in path)
            // {
            //     Raylib.DrawLineV(p.Item1, p.Item2, Color.Red);
            // }
        }
    }

    
}

