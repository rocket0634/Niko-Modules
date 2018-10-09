using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RuleGenerator
{
    public static class BlindMazeRuleGenerator
    {
        private static MonoRandom _rng;

        public static void GenerateRules(MonoRandom rng)
        {
            if (_rng != null && _rng.Seed == rng.Seed) return;
            _rng = rng;

            Mazes.Clear();
            for (int i = 0; i < 10; i++)
            {
                var maze = new Maze();
                Mazes.Add(maze);
                maze.BuildMaze(rng);
            }
        }

        public static List<Maze> Mazes = new List<Maze>();
    }

    public class Maze
    {
        private const int Size = 5;

        public List<List<MazeCell>> CellGrid;

        public Maze()
        {
            CellGrid = new List<List<MazeCell>>();
            for (var i = 0; i < Size; i++)
            {
                var list = new List<MazeCell>();
                CellGrid.Add(list);
                for (var j = 0; j < Size; j++)
                {
                    var mazeCell = new MazeCell(i, j);
                    list.Add(mazeCell);
                }
            }
        }

        public MazeCell GetCell(int x, int y)
        {
            if (x < 0 || x >= Size || y < 0 || y >= Size)
                return null;
            return CellGrid[x][y];
        }

        public void BuildMaze(MonoRandom rng)
        {

        }

        public void PopulateMaze(MonoRandom rng)
        {
            var cellStack = new Stack<MazeCell>();
            var x = rng.Next(0, Size);
            var y = rng.Next(0, Size);
            var cell = GetCell(x, y);
            VisitCell(cell, cellStack, rng);
        }

        public void VisitCell(MazeCell cell, Stack<MazeCell> cellStack, MonoRandom rng)
        {
            while (cell != null)
            {
                cell.Visited = true;
                var mazeCell = GetNextNeigbour(cell, rng);
                if (mazeCell != null)
                {
                    MazeCell.RemoveWalls(cell, mazeCell);
                    cellStack.Push(cell);
                }
                else if (cellStack.Count > 0)
                {
                    mazeCell = cellStack.Pop();
                }
                cell = mazeCell;
            }
        }

        public MazeCell GetNextNeigbour(MazeCell cell, MonoRandom rng)
        {
            var list = new List<MazeCell>();
            if (cell.X > 0 && !CellGrid[cell.X - 1][cell.Y].Visited) list.Add(CellGrid[cell.X - 1][cell.Y]);
            if (cell.X < CellGrid.Count - 1 && !CellGrid[cell.X + 1][cell.Y].Visited) list.Add(CellGrid[cell.X + 1][cell.Y]);
            if (cell.Y > 0 && !CellGrid[cell.X][cell.Y - 1].Visited) list.Add(CellGrid[cell.X][cell.Y - 1]);
            if (cell.Y < CellGrid[cell.X].Count - 1 && !CellGrid[cell.X][cell.Y + 1].Visited) list.Add(CellGrid[cell.X][cell.Y + 1]);
            return list.Count > 0
                ? list[rng.Next(0, list.Count)]
                : null;
        }
    }

    public class MazeCell
    {
        public bool Visited;
        public bool WallUp = true;
        public bool WallDown = true;
        public bool WallLeft = true;
        public bool WallRight = true;
        public int X;
        public int Y;

        public MazeCell(int x, int y)
        {
            X = x;
            Y = y;
        }
        
        public static void RemoveWalls(MazeCell m1, MazeCell m2)
        {
            if (m1.X - m2.X == 1)
            {
                m1.WallLeft = false;
                m2.WallRight = false;
            }
            if (m1.X - m2.X == -1)
            {
                m1.WallRight = false;
                m2.WallLeft = false;
            }
            if (m1.Y - m2.Y == 1)
            {
                m1.WallUp = false;
                m2.WallDown = false;
            }
            if (m1.Y - m2.Y == -1)
            {
                m1.WallDown = false;
                m2.WallUp = false;
            }
        }
    }
}