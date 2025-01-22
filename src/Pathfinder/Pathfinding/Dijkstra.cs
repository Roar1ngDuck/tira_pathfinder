using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Pathfinder.Pathfinding
{
    internal class Dijkstra : IPathFindingAlgorithm
    {
        private readonly int[,] _map;

        public Dijkstra(int[,] map)
        {
            _map = map;
        }

        public PathFindingResult Search(Node start, Node goal, bool allowDiagonal, Action<int[,], IEnumerable<Node>, IEnumerable<Node>, Node>? callbackFunc, TimeSpan stepDelay)
        {
            var openSet = new PriorityQueue<Node, double>();
            openSet.Enqueue(start, 0);

            var inOpenSet = new bool[_map.GetLength(0), _map.GetLength(1)];
            inOpenSet[start.X, start.Y] = true;

            var width = _map.GetLength(0);
            var height = _map.GetLength(1);
            var cameFrom = new Node?[width, height];
            var gScore = new double[width, height];

            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    gScore[x, y] = double.MaxValue;
                }
            }

            gScore[start.X, start.Y] = 0;

            var timingStopwatch = Stopwatch.StartNew();
            long counter = 0;

            while (openSet.Count > 0)
            {
                var current = openSet.Dequeue();
                inOpenSet[current.X, current.Y] = false;

                if (callbackFunc != null && MainWindow.ShouldCallCallback)
                {
                    var visited = ExtractVisitedNodes(gScore);
                    var queue = openSet.UnorderedItems.Select(item => item.Element).ToList();
                    callbackFunc(_map, visited, queue, current);
                }

                if (current == goal)
                {
                    var path = Helpers.ReconstructPath(cameFrom, current);
                    return new PathFindingResult(ExtractVisitedNodes(gScore), path);
                }

                var neighbors = Helpers.GetNeighbors(_map, current, allowDiagonal);
                foreach (var (neighbor, cost) in neighbors)
                {
                    double tentative_gScore = gScore[current.X, current.Y] + cost;

                    if (tentative_gScore < gScore[neighbor.X, neighbor.Y])
                    {
                        cameFrom[neighbor.X, neighbor.Y] = current;
                        gScore[neighbor.X, neighbor.Y] = tentative_gScore;

                        if (!inOpenSet[neighbor.X, neighbor.Y])
                        {
                            openSet.Enqueue(neighbor, tentative_gScore);
                            inOpenSet[neighbor.X, neighbor.Y] = true;
                        }
                    }
                }

                if (stepDelay.TotalMilliseconds > 0)
                {
                    double elapsedMs = timingStopwatch.Elapsed.TotalMilliseconds;
                    double targetDelay = counter * stepDelay.TotalMilliseconds;
                    if (elapsedMs < targetDelay)
                    {
                        Thread.Sleep((int)Math.Max(1, targetDelay - elapsedMs));
                    }
                }

                counter++;
            }

            return new PathFindingResult(ExtractVisitedNodes(gScore), null);
        }

        public PathFindingResult Search(Node start, Node goal, bool allowDiagonal)
        {
            return Search(start, goal, allowDiagonal, null, TimeSpan.Zero);
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
}
