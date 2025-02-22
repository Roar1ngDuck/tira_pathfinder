using Pathfinder.Pathfinding;
using Pathfinder.Pathfinding.Algorithms;
using Pathfinder.Pathfinding.Utils;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pathfinder.Tests
{
    public class ComparisonTests
    {
        private const int MapSize = 128;

        private static int[,] GenerateRandomMap(int size, int seed, double obstacleProbability)
        {
            var random = new Random(seed);
            var map = new int[size, size];

            for (int x = 0; x < size; x++)
            {
                for (int y = 0; y < size; y++)
                {
                    map[x, y] = random.NextDouble() < obstacleProbability ? 1 : 0;
                }
            }

            return map;
        }

        [Fact]
        public void AlgorithmsPerformanceIsCorrect()
        {
            var totalMilliseconds = new double[3];
            var maxCount = 50;

            for (int i = 0; i < maxCount; i++)
            {
                int seed = i;
                double obstacleProbability = 0.1;
                int[,] map = GenerateRandomMap(MapSize, seed, obstacleProbability);
                var rnd = new Random(seed);

                var start = new Node(rnd.Next(0, MapSize), rnd.Next(0, MapSize));
                var goal = new Node(rnd.Next(0, MapSize), rnd.Next(0, MapSize));
                var distance = DistanceUtils.EuclideanDistance(start, goal);
                while (distance <= 50)
                {
                    start = new Node(rnd.Next(0, MapSize), rnd.Next(0, MapSize));
                    goal = new Node(rnd.Next(0, MapSize), rnd.Next(0, MapSize));
                    distance = DistanceUtils.EuclideanDistance(start, goal);
                }

                var algorithms = new IPathFindingAlgorithm[] { new Dijkstra(map), new AStar(map), new JumpPointSearch(map) };

                PathFindingResult? previous = null;
                var timingStopwatch = new Stopwatch();
                for (int ii = 0; ii < algorithms.Length; ii++)
                {
                    var algorithm = algorithms[ii];
                    timingStopwatch.Restart();
                    var result = algorithm.Search(start, goal, allowDiagonal: true);
                    timingStopwatch.Stop();

                    if (previous != null)
                    {
                        Assert.Equal(Math.Round(previous.PathLength, 5), Math.Round(result.PathLength, 5));
                    }

                    previous = result;

                    if (!result.PathFound)
                    {
                        maxCount++;
                        break;
                    }

                    var elapsed = timingStopwatch.Elapsed.TotalMilliseconds;
                    totalMilliseconds[ii] += elapsed;
                }
            }

            for (int i = 0; i < totalMilliseconds.Length - 1; i++)
            {
                Assert.True(totalMilliseconds[i] > totalMilliseconds[i + 1]);
            }
        }
    }
}
