using System;
using System.Collections.Generic;

namespace Pathfinder.Pathfinding;

public class Helpers
{
    private static readonly (int, int)[] directions = [(-1, 0), (1, 0), (0, -1), (0, 1)];

    public static List<Node> GetNeighbors(int[,] map, Node current)
    {
        var neighbors = new List<Node>();
        foreach (var (dx, dy) in directions)
        {
            int x = current.X + dx;
            int y = current.Y + dy;

            if (x >= 0 && x < map.GetLength(0) && y >= 0 && y < map.GetLength(1) && map[x, y] == 0)
            {
                neighbors.Add(new Node(x, y));
            }
        }

        return neighbors;
    }

    public static List<Node> ReconstructPath(Node?[,] cameFrom, Node current)
    {
        var totalPath = new List<Node> { current };
        while (cameFrom[current.X, current.Y] != null)
        {
            current = cameFrom[current.X, current.Y]!.Value;
            totalPath.Insert(0, current);
        }
        return totalPath;
    }
}
