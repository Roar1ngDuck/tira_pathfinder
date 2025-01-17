using System;
using System.Collections.Generic;

namespace Pathfinder.Pathfinding;

public class Helpers
{
    private static readonly (int, int)[] directions = [(-1, 0), (1, 0), (0, -1), (0, 1)];

    public static List<(int x, int y)> GetNeighbors(int[,] map, (int x, int y) current)
    {
        var neighbors = new List<(int x, int y)>();
        foreach (var (dx, dy) in directions)
        {
            int x = current.x + dx;
            int y = current.y + dy;

            if (x >= 0 && x < map.GetLength(0) && y >= 0 && y < map.GetLength(1) && map[x, y] == 0)
            {
                neighbors.Add((x, y));
            }
        }

        return neighbors;
    }
}
