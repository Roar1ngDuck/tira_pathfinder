using Pathfinder.Pathfinding;
using Pathfinder.Pathfinding.Algorithms;
using Pathfinder.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace Pathfinder.Tests
{
    public class RealMapTests
    {
        [Fact]
        public void Berlin_PathLengthsMatch()
        {
            for (int i = 0; i < 25; i++)
            {
                int seed = i;
                var map = Input.ReadMapFromFile("Maps/Berlin_1_512.map");

                var MapSize = map.GetLength(0);

                var rnd = new Random(seed);

                var start = new Node(rnd.Next(0, MapSize), rnd.Next(0, MapSize));
                var goal = new Node(rnd.Next(0, MapSize), rnd.Next(0, MapSize));

                var algorithms = new PathFindingAlgorithm[] { new AStar(map), new Dijkstra(map), new JumpPointSearch(map) };

                PathFindingResult? previous = null;
                foreach (var algorithm in algorithms)
                {
                    var result = algorithm.Search(start, goal, allowDiagonal: true);

                    if (previous != null)
                    {
                        Assert.Equal(Math.Round(previous.PathLength, 5), Math.Round(result.PathLength, 5));
                    }

                    previous = result;
                }
            }
        }


        [Fact]
        public void RoomMap_PathLengthsMatch()
        {
            for (int i = 0; i < 25; i++)
            {
                int seed = i;
                var map = Input.ReadMapFromFile("Maps/16room_001.map");

                var MapSize = map.GetLength(0);

                var rnd = new Random(seed);

                var start = new Node(rnd.Next(0, MapSize), rnd.Next(0, MapSize));
                var goal = new Node(rnd.Next(0, MapSize), rnd.Next(0, MapSize));

                var algorithms = new PathFindingAlgorithm[] { new AStar(map), new Dijkstra(map), new JumpPointSearch(map) };

                PathFindingResult? previous = null;
                foreach (var algorithm in algorithms)
                {
                    var result = algorithm.Search(start, goal, allowDiagonal: true);

                    if (previous != null)
                    {
                        Assert.Equal(Math.Round(previous.PathLength, 5), Math.Round(result.PathLength, 5));
                    }

                    previous = result;
                }
            }
        }

        [Fact]
        public void WinterConquest_PathLengthsMatch()
        {
            for (int i = 0; i < 25; i++)
            {
                int seed = i;
                var map = Input.ReadMapFromFile("Maps/WinterConquest.map");

                var MapSize = map.GetLength(0);

                var rnd = new Random(seed);

                var start = new Node(rnd.Next(0, MapSize), rnd.Next(0, MapSize));
                var goal = new Node(rnd.Next(0, MapSize), rnd.Next(0, MapSize));

                var algorithms = new PathFindingAlgorithm[] { new AStar(map), new Dijkstra(map), new JumpPointSearch(map) };

                PathFindingResult? previous = null;
                foreach (var algorithm in algorithms)
                {
                    var result = algorithm.Search(start, goal, allowDiagonal: true);

                    if (previous != null)
                    {
                        Assert.Equal(Math.Round(previous.PathLength, 5), Math.Round(result.PathLength, 5));
                    }

                    previous = result;
                }
            }
        }
    }
}
