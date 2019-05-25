using System.Collections.Generic;
using System.Linq;

public static class MazeGeneration {
    public class Cell {
        public bool Visited { get; set; }
        public bool this[string direction] {
            get { return directions[direction]; }
            set { directions[direction] = value; }
        }

        private readonly Dictionary<string, bool> directions = new Dictionary<string, bool> { { "N", false }, { "E", false }, { "W", false }, { "S", false } };
    }

    public readonly static Cell[,,] Cells = new Cell[10, 5, 5];

    private readonly static string[] cardinal = { "N", "E", "W", "S" };
    private readonly static int[,] locations = { { -1, 0, 0, 1 }, { 0, 1, -1, 0 } };

    public static void InitializeGeneration() {
        for (var i = 0; i < 10; i++) {
            for (var j = 0; j < 5; j++) {
                for (var k = 0; k < 5; k++) {
                    Cells[i, j, k] = new Cell();
                }
            }
        }

        for (var i = 0; i < 10; i++)
            CheckCell(i, BlindMaze.rng.Next(4), BlindMaze.rng.Next(4));
    }

    private static bool CheckCell(int maze, int row, int col, int prevDir = -1) {
        if (!InRange(row, 0, 4) || !InRange(col, 0, 4) || Cells[maze, row, col].Visited) return false;

        Cells[maze, row, col].Visited = true;
        var start = BlindMaze.rng.Next(4);

        for (var i = 0; i < 4; i++) {
            var point = (start + i) % 4;
            Cells[maze, row, col][cardinal[point]] = CheckCell(maze, row + locations[0, point], col + locations[1, point], point);
        }

        if (row == 0 && col == 2) Cells[maze, row, col]["N"] = true;
        if (prevDir != -1) Cells[maze, row, col][cardinal[3 - prevDir]] = true;

        return true;
    }

    private static bool InRange(int value, int min, int max) { return (value >= min && value <= max); }

    public static string Join<T>(this IEnumerable<T> elements, string seperator = " ") {
        return string.Join(seperator, elements.OfType<object>().Select(x => x.ToString()).ToArray());
    }

    public static T[,] ToArray2D<T>(this T[] array1D, int rowLen, int colLen) {
        T[,] setArray2D = new T[rowLen, colLen];

        for (var i = 0; i < (rowLen * colLen); i++)
            setArray2D[i / colLen, i % colLen] = (i < array1D.Length) ? array1D[i] : default(T);

        return setArray2D;
    }
}