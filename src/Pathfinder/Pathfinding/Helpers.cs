using System;
using System.Collections.Generic;

namespace Pathfinder.Pathfinding;

public class Helpers
{
    private static readonly (int, int, double)[] directionsStraight = [(-1, 0, 1), (1, 0, 1), (0, -1, 1), (0, 1, 1)];

    private static readonly(int, int, double)[] directionsDiagonal = [
        (-1, 0, 1), (1, 0, 1), (0, -1, 1), (0, 1, 1), // Suoraan
        (-1, -1, Math.Sqrt(2)), (-1, 1, Math.Sqrt(2)), (1, -1, Math.Sqrt(2)), (1, 1, Math.Sqrt(2)) // Vinottain
    ];

    public static List<(Node neighbor, double cost)> GetNeighbors(int[,] grid, Node node, bool allowDiagonal)
    {
        var directions = allowDiagonal ? directionsDiagonal : directionsStraight;

        var neighbors = new List<(Node, double)>();
        foreach (var (dr, dc, cost) in directions)
        {
            int newRow = node.X + dr;
            int newCol = node.Y + dc;

            if (newRow >= 0 && newRow < grid.GetLength(0) &&
                newCol >= 0 && newCol < grid.GetLength(1) &&
                grid[newRow, newCol] == 0)
            {
                neighbors.Add((new Node(newRow, newCol), cost));
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
