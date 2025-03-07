using Pathfinder.Pathfinding;
using Pathfinder.Pathfinding.Algorithms;

namespace Pathfinder.Tests.Tests.Algorithms;

public class JumpPointSearchTests
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
    /// Testaa että JPS löytää oikean reitin
    /// </summary>
    [Fact]
    public void FindsShortestPath()
    {
        var jps = new JumpPointSearch(_simpleMap);
        var result = jps.Search(_start, _goal, allowDiagonal: true);

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
            {0,0,0,0},
            {1,1,1,1},
            {0,0,0,0},
            {0,0,0,0}
        };

        var jps = new JumpPointSearch(mapWithObstacle);
        var result = jps.Search(_start, _goal, allowDiagonal: true);

        Assert.Null(result.Path);
    }

    /// <summary>
    /// Testaa että JPS löytää oikeat Jump Pointit
    /// </summary>
    [Fact]
    public void ExploresCorrectJumpPoints()
    {
        var jps = new JumpPointSearch(_simpleMap);
        var result = jps.Search(_start, _goal, allowDiagonal: true);

        Assert.NotNull(result.VisitedNodes);

        var visited = new HashSet<Node>(result.VisitedNodes);

        int[,] expectedGrid =
        {
            {1,0,1,0},
            {1,0,0,1},
            {0,1,1,0},
            {0,0,1,1},
            {0,0,0,0}
        };

        var expectedSet = Pathfinder.Tests.Utils.BuildSetFromGrid(expectedGrid);
        Assert.Equal(expectedSet, visited);
    }

    /// <summary>
    /// Testaa että jos start=goal, niin JPS palauttaa heti tuloksen
    /// </summary>
    [Fact]
    public void StartEqualsGoal_ReturnsImmediateResult()
    {
        var jps = new JumpPointSearch(_simpleMap);
        var result = jps.Search(_start, _start, allowDiagonal: true);

        Assert.NotNull(result.Path);
        Assert.Single(result.Path);
        Assert.Equal(_start, result.Path.First());
    }
}
