using Pathfinder.Pathfinding;
using Pathfinder.Pathfinding.Algorithms;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pathfinder.Tests
{
    public class AStarTests
    {
        private readonly int[,] _simpleMap = {
            { 0, 0, 0, 0 },
            { 0, 1, 1, 0 },
            { 0, 0, 0, 0 },
            { 0, 0, 0, 0 }
        };

        private readonly Node _start = new Node(0, 0);
        private readonly Node _goal = new Node(3, 3);

        [Fact]
        public void FindsShortestPath_NonDiagonal()
        {
            var astar = new AStar(_simpleMap);
            var result = astar.Search(_start, _goal, allowDiagonal: false);

            Assert.NotNull(result.Path);
            Assert.Equal(7, result.Path.Count);
            Assert.Equal(_goal, result.Path.Last());
        }

        [Fact]
        public void FindsShortestPath_Diagonal()
        {
            var astar = new AStar(_simpleMap);
            var result = astar.Search(_start, _goal, allowDiagonal: true);

            Assert.NotNull(result.Path);
            Assert.Equal(5, result.Path.Count);
            Assert.Equal(_goal, result.Path.Last());
        }

        [Fact]
        public void ReturnsNoPath_WhenBlocked()
        {
            var mapWithObstacle = new[,] {
                { 0, 0, 0, 0 },
                { 1, 1, 1, 1 },
                { 0, 0, 0, 0 },
                { 0, 0, 0, 0 }
            };

            var astar = new AStar(mapWithObstacle);
            var result = astar.Search(_start, _goal, allowDiagonal: false);

            Assert.Null(result.Path);
        }

        [Fact]
        public void ExploresExactNodes_NonDiagonal()
        {
            var astar = new AStar(_simpleMap);
            var result = astar.Search(_start, _goal, allowDiagonal: false);
            var visited = new HashSet<Node>(result.VisitedNodes);

            var expectedVisited = new HashSet<Node>
            {
                new Node(0, 0),
                new Node(0, 1), new Node(1, 0),
                new Node(2, 0), new Node(0, 2),
                new Node(2, 1), new Node(0, 3),
                new Node(2, 2), new Node(1, 3),
                new Node(3, 2), new Node(2, 3),
                new Node(3, 3)
            };

            Assert.Equal(expectedVisited, visited);
        }

        [Fact]
        public void ExploresExactNodes_Diagonal()
        {
            var astar = new AStar(_simpleMap);
            var result = astar.Search(_start, _goal, allowDiagonal: true);
            var visited = new HashSet<Node>(result.VisitedNodes);

            var expectedVisited = new HashSet<Node>
            {
                new Node(0, 0), new Node(0, 1), new Node(1, 0), new Node(2, 1), new Node(2, 2), new Node(3, 2), new Node(3, 3)
            };

            Assert.Equal(expectedVisited, visited);
        }

        [Fact]
        public void StartEqualsGoal_ReturnsImmediateResult()
        {
            var astar = new AStar(_simpleMap);
            var result = astar.Search(_start, _start, allowDiagonal: false);

            Assert.NotNull(result.Path);
            Assert.Single(result.Path);
            Assert.Equal(_start, result.Path.First());
        }
    }
}
