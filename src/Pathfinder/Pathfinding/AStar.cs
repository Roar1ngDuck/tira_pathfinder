using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;

namespace Pathfinder.Pathfinding;

public class AStar
{
    public static List<Node> Search(int[,] map, Node start, Node goal, Action<int[,], ICollection<Node>, ICollection<Node>, Node, ICollection<Node>?> callbackFunc, int callBackInterval, TimeSpan stepDelay)
    {
        var openSet = new PriorityQueue<Node, double>();
        openSet.Enqueue(start, 0);

        var cameFrom = new Dictionary<Node, Node>();

        var gScore = new Dictionary<Node, double>();
        gScore[start] = 0;

        var fScore = new Dictionary<Node, double>();
        fScore[start] = ManhattanDistance(start, goal);

        var timingStopwatch = Stopwatch.StartNew();
        long steps = 0;

        var counter = 0;

        while (openSet.Count > 0)
        {
            var current = openSet.Dequeue();

            if (current == goal)
            {
                var path = Helpers.ReconstructPath(cameFrom, current);

                callbackFunc(map, gScore.Keys.ToImmutableArray(), openSet.UnorderedItems.Select(item => item.Element).ToImmutableArray(), current, path);

                return path;
            }

            if (counter % callBackInterval == 0)
            {
                callbackFunc(map, gScore.Keys.ToImmutableArray(), openSet.UnorderedItems.Select(item => item.Element).ToImmutableArray(), current, null);
            }

            var neighbors = Helpers.GetNeighbors(map, current);
            foreach (var neighbor in neighbors)
            {
                var cost = 1;
                double tentative_gScore = gScore[current] + cost;

                if (!gScore.ContainsKey(neighbor) || tentative_gScore < gScore[neighbor])
                {
                    cameFrom[neighbor] = current;
                    gScore[neighbor] = tentative_gScore;
                    fScore[neighbor] = tentative_gScore + ManhattanDistance(neighbor, goal);

                    if (!openSet.UnorderedItems.Any(x => x.Element == neighbor))
                    {
                        openSet.Enqueue(neighbor, fScore[neighbor]);
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

    static double ManhattanDistance(Node a, Node b)
    {
        return Math.Abs(a.X - b.X) + Math.Abs(a.Y - b.Y);
    }
}
