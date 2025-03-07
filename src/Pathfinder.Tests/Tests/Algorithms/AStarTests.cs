using Pathfinder.Pathfinding;
using Pathfinder.Pathfinding.Algorithms;

namespace Pathfinder.Tests.Tests.Algorithms;

public class AStarTests
{
    private readonly int[,] _simpleMap =
    {
        {0, 0, 0, 0},
        {0, 1, 1, 0},
        {0, 0, 0, 0},
        {0, 0, 0, 0}
    };

    private readonly Node _start = new Node(0, 0);
    private readonly Node _goal = new Node(3, 3);

    /// <summary>
    /// Testaa että A* löytää oikean reitin ei-diagonaalisesti
    /// </summary>
    [Fact]
    public void FindsShortestPath_NonDiagonal()
    {
        var astar = new AStar(_simpleMap);
        var result = astar.Search(_start, _goal, allowDiagonal: false);

        Assert.NotNull(result.Path);
        Assert.Equal(7, result.Path.Count);
        Assert.Equal(_goal, result.Path.Last());
    }

    /// <summary>
    /// Testaa että A* löytää lyhimmän reitin diagonaalisesti
    /// </summary>
    [Fact]
    public void FindsShortestPath_Diagonal()
    {
        var astar = new AStar(_simpleMap);
        var result = astar.Search(_start, _goal, allowDiagonal: true);

        Assert.NotNull(result.Path);
        Assert.Equal(5, result.Path.Count);
        Assert.Equal(_goal, result.Path.Last());
    }

    /// <summary>
    /// Testaa ettei reittiä löydy, kun sitä ei ole
    /// </summary>
    [Fact]
    public void ReturnsNoPath_WhenBlocked()
    {
        var mapWithObstacle = new[,]
        {
            {0, 0, 0, 0},
            {1, 1, 1, 1},
            {0, 0, 0, 0},
            {0, 0, 0, 0}
        };

        var astar = new AStar(mapWithObstacle);
        var result = astar.Search(_start, _goal, allowDiagonal: false);

        Assert.Null(result.Path);
    }

    /// <summary>
    /// Testaa että A* käy läpi oikeat solmut ei-diagonaalisesti
    /// </summary>
    [Fact]
    public void ExploresExactNodes_NonDiagonal()
    {
        var astar = new AStar(_simpleMap);
        var result = astar.Search(_start, _goal, allowDiagonal: false);

        Assert.NotNull(result.VisitedNodes);

        var visited = new HashSet<Node>(result.VisitedNodes);

        int[,] expectedGrid =
        {
            {1,1,1,1},
            {1,0,0,1},
            {1,1,1,1},
            {0,0,1,1}
        };

        var expectedSet = Pathfinder.Tests.Utils.BuildSetFromGrid(expectedGrid);
        Assert.Equal(expectedSet, visited);
    }

    /// <summary>
    /// Testaa että A* käy läpi oikeat solmut diagonaalisesti
    /// </summary>
    [Fact]
    public void ExploresExactNodes_Diagonal()
    {
        var astar = new AStar(_simpleMap);
        var result = astar.Search(_start, _goal, allowDiagonal: true);

        Assert.NotNull(result.VisitedNodes);

        var visited = new HashSet<Node>(result.VisitedNodes);

        int[,] expectedGrid =
        {
            {1,1,0,0},
            {1,0,0,0},
            {0,1,1,0},
            {0,0,1,1}
        };

        var expectedSet = Pathfinder.Tests.Utils.BuildSetFromGrid(expectedGrid);
        Assert.Equal(expectedSet, visited);
    }

    /// <summary>
    /// Testaa että jos start=goal, niin A* palauttaa heti tuloksen
    /// </summary>
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
