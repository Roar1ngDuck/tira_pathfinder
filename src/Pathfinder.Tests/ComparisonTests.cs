﻿using Pathfinder.Pathfinding;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pathfinder.Tests
{
    public class ComparisonTests
    {
        private const int MapSize = 256;

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
        public void AlgorithmsPathLengthsMatch_Diagonal()
        {
            for (int i = 0; i < 100; i++)
            {
                int seed = i;
                double obstacleProbability = 0.3;
                int[,] map = GenerateRandomMap(MapSize, seed, obstacleProbability);

                var start = new Node(0, 0);
                var goal = new Node(MapSize - 1, MapSize - 1);

                IPathFindingAlgorithm aStar = new AStar(map);
                IPathFindingAlgorithm dijkstra = new Dijkstra(map);

                var aStarResult = aStar.Search(start, goal, allowDiagonal: true);
                var dijkstraResult = dijkstra.Search(start, goal, allowDiagonal: true);

                Assert.Equal(Math.Round(dijkstraResult.PathLength, 5), Math.Round(aStarResult.PathLength, 5));
            }
        }

        [Fact]
        public void AlgorithmsPathLengthsMatch_NonDiagonal()
        {
            for (int i = 0; i < 100; i++)
            {
                int seed = i;
                double obstacleProbability = 0.3;
                int[,] map = GenerateRandomMap(MapSize, seed, obstacleProbability);

                var start = new Node(0, 0);
                var goal = new Node(MapSize - 1, MapSize - 1);

                IPathFindingAlgorithm aStar = new AStar(map);
                IPathFindingAlgorithm dijkstra = new Dijkstra(map);

                var aStarResult = aStar.Search(start, goal, allowDiagonal: false);
                var dijkstraResult = dijkstra.Search(start, goal, allowDiagonal: false);

                Assert.Equal(dijkstraResult.PathLength, aStarResult.PathLength);
            }
        }
    }
}
