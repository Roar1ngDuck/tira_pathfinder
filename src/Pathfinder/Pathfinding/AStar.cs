using Avalonia.Controls.Shapes;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;

namespace Pathfinder.Pathfinding;

public class AStar : IPathFindingAlgorithm
{
    public List<Node> Search(int[,] map, Node start, Node goal, Action<int[,], ICollection<Node>, ICollection<Node>, Node, ICollection<Node>?>? callbackFunc, int callBackInterval, TimeSpan stepDelay)
    {
        var openSet = new PriorityQueue<Node, double>();
        openSet.Enqueue(start, 0);

        var width = map.GetLength(0);
        var height = map.GetLength(1);
        var cameFrom = new Node?[width, height];
        var gScore = new double[width, height];
        var fScore = new double[width, height];

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                gScore[x, y] = double.MaxValue;
                fScore[x, y] = double.MaxValue;
            }
        }

        gScore[start.X, start.Y] = 0;
        fScore[start.X, start.Y] = ManhattanDistance(start, goal);

        var timingStopwatch = Stopwatch.StartNew();
        long steps = 0;
        var counter = 0;

        while (openSet.Count > 0)
        {
            var current = openSet.Dequeue();

            if (current == goal)
            {
                var path = Helpers.ReconstructPath(cameFrom, current);

                if (callbackFunc != null)
                {
                    callbackFunc(map, ExtractVisitedNodes(gScore), openSet.UnorderedItems.Select(item => item.Element).ToArray(), current, path);
                }

                return path;
            }

            if (callbackFunc != null && counter % callBackInterval == 0)
            {
                callbackFunc(map, ExtractVisitedNodes(gScore), openSet.UnorderedItems.Select(item => item.Element).ToArray(), current, null);
            }

            var neighbors = Helpers.GetNeighbors(map, current);
            foreach (var neighbor in neighbors)
            {
                var cost = 1;
                double tentative_gScore = gScore[current.X, current.Y] + cost;

                if (tentative_gScore < gScore[neighbor.X, neighbor.Y])
                {
                    cameFrom[neighbor.X, neighbor.Y] = current;
                    gScore[neighbor.X, neighbor.Y] = tentative_gScore;
                    fScore[neighbor.X, neighbor.Y] = tentative_gScore + ManhattanDistance(neighbor, goal);

                    if (!openSet.UnorderedItems.Any(x => x.Element == neighbor))
                    {
                        openSet.Enqueue(neighbor, fScore[neighbor.X, neighbor.Y]);
                    }
                }
            }

            if (stepDelay.TotalMilliseconds > 0)
            {
                steps++;
                double elapsedMs = timingStopwatch.Elapsed.TotalMilliseconds;
                double targetDelay = steps * stepDelay.TotalMilliseconds;
                if (elapsedMs < targetDelay)
                {
                    Thread.Sleep((int)Math.Max(1, targetDelay - elapsedMs));
                }
            }

            counter++;
        }

        return new List<Node>();
    }

    public List<Node> Search(int[,] map, Node start, Node goal)
    {
        return Search(map, start, goal, null, 0, TimeSpan.Zero);
    }

    static double ManhattanDistance(Node a, Node b)
    {
        return Math.Abs(a.X - b.X) + Math.Abs(a.Y - b.Y);
    }

    private static ICollection<Node> ExtractVisitedNodes(double[,] gScore)
    {
        var visitedNodes = new List<Node>();
        for (int x = 0; x < gScore.GetLength(0); x++)
        {
            for (int y = 0; y < gScore.GetLength(1); y++)
            {
                if (gScore[x, y] != double.MaxValue)
                {
                    visitedNodes.Add(new Node(x, y));
                }
            }
        }
        return visitedNodes;
    }
}
