using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
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

    public static int[,] ReadMapFromImage(string filePath)
    {
        using var image = Image.Load<L8>(filePath);

        int width = image.Width;
        int height = image.Height;

        var map = new int[width, height];

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                byte intensity = image[x, y].PackedValue;

                map[x, y] = intensity < 128 ? 1 : 0;
            }
        }

        return map;
    }
}
