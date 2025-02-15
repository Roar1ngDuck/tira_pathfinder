using Xunit;
using Pathfinder.Pathfinding;
using Pathfinder.Pathfinding.Algorithms;
using System.Collections.Generic;
using System.Linq;

namespace Pathfinder.Tests
{
    public class DijkstraTests
    {
        private readonly int[,] _simpleMap =
        {
            { 0, 0, 0, 0 },
            { 0, 1, 1, 0 },
            { 0, 0, 0, 0 },
            { 0, 0, 0, 0 },
            { 0, 0, 0, 0 }
        };

        private readonly Node _start = new Node(0, 0);
        private readonly Node _goal = new Node(3, 3);

        /// <summary>
        /// Testaa että Dijkstra löytää oikean reitin ei-diagonaalisesti
        /// </summary>
        [Fact]
        public void FindsShortestPath_NonDiagonal()
        {
            var dijkstra = new Dijkstra(_simpleMap);
            var result = dijkstra.Search(_start, _goal, allowDiagonal: false);

            Assert.NotNull(result.Path);
            Assert.Equal(7, result.Path.Count);
            Assert.Equal(_goal, result.Path.Last());
        }

        /// <summary>
        /// Testaa että Dijkstra löytää oikean reitin diagonaalisesti
        /// </summary>
        [Fact]
        public void FindsShortestPath_Diagonal()
        {
            var dijkstra = new Dijkstra(_simpleMap);
            var result = dijkstra.Search(_start, _goal, allowDiagonal: true);

            Assert.NotNull(result.Path);
            Assert.Equal(5, result.Path.Count);
            Assert.Equal(_goal, result.Path.Last());
        }

        /// <summary>
        /// Testaa ettei reittiä löydy, kun sitä ei ole
        /// </summary>
        [Fact]
        public void NoPath_WhenBlocked()
        {
            var mapWithObstacle = new[,]
            {
                { 0, 0, 0, 0 },
                { 1, 1, 1, 1 },
                { 0, 0, 0, 0 },
                { 0, 0, 0, 0 }
            };

            var dijkstra = new Dijkstra(mapWithObstacle);
            var result = dijkstra.Search(_start, _goal, allowDiagonal: false);

            Assert.Null(result.Path);
        }

        /// <summary>
        /// Testaa että Dijkstra käy läpi oikeat solmut ei-diagonaalisesti
        /// </summary>
        [Fact]
        public void ExploresExactNodes_NonDiagonal()
        {
            var dijkstra = new Dijkstra(_simpleMap);
            var result = dijkstra.Search(_start, _goal, allowDiagonal: false);

            var visited = new HashSet<Node>(result.VisitedNodes);

            int[,] expectedGrid =
            {
                {1,1,1,1},
                {1,0,0,1},
                {1,1,1,1},
                {1,1,1,1},
                {1,1,1,0}
            };

            var expectedSet = Utils.BuildSetFromGrid(expectedGrid);
            Assert.Equal(expectedSet, visited);
        }

        /// <summary>
        /// Testaa että Dijkstra käy läpi oikeat solmut diagonaalisesti
        /// </summary>
        [Fact]
        public void ExploresExactNodes_Diagonal()
        {
            var dijkstra = new Dijkstra(_simpleMap);
            var result = dijkstra.Search(_start, _goal, allowDiagonal: true);

            var visited = new HashSet<Node>(result.VisitedNodes);

            int[,] expectedGrid =
            {
                {1,1,1,1},
                {1,0,0,1},
                {1,1,1,1},
                {1,1,1,1},
                {1,1,1,0}
            };

            var expectedSet = Utils.BuildSetFromGrid(expectedGrid);
            Assert.Equal(expectedSet, visited);
        }

        /// <summary>
        /// Testaa että jos start=goal, niin Dijkstra palauttaa heti tuloksen
        /// </summary>
        [Fact]
        public void StartEqualsGoal_ReturnsImmediateResult()
        {
            var dijkstra = new Dijkstra(_simpleMap);
            var result = dijkstra.Search(_start, _start, allowDiagonal: false);

            Assert.NotNull(result.Path);
            Assert.Single(result.Path);
            Assert.Equal(_start, result.Path.First());
        }
    }
}
