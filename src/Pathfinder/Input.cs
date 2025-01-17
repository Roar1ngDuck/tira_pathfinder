using System;
using System.IO;
using System.Linq;

namespace Pathfinder;

public class Input
{
    public static int[,] ReadMapFromFile(string file)
    {
        var lines = File.ReadAllLines(file).Where(line => line.Contains('.') || line.Contains('@')).ToArray();

        var width = lines[0].Length;
        var height = lines.Length;

        var map = new int[width, height];

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                var mapPiece = lines[y][x];

                map[x, y] = mapPiece == '.' ? 0 : 1;
            }
        }

        return map;
    }
}
