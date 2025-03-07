using Pathfinder.Pathfinding;
using Pathfinder.Pathfinding.Utils;

namespace Pathfinder.Tests.Tests.Utils;

public class PathUtilsTests
{
    /// <summary>
    /// Testaa, että ReconstructPath palauttaa oikean reitin
    /// </summary>
    [Fact]
    public void ReconstructPath_ArrayVersion_ReturnsCorrectPath()
    {
        var cameFrom = new Node?[3, 3];
        cameFrom[1, 1] = new Node(0, 1);
        cameFrom[2, 1] = new Node(1, 1);
        var end = new Node(2, 1);

        var result = PathUtils.ReconstructPath(cameFrom, end);

        Assert.NotNull(result);
        Assert.Equal(3, result.Count);
        Assert.Equal(new Node(0, 1), result[0]);
        Assert.Equal(new Node(1, 1), result[1]);
        Assert.Equal(new Node(2, 1), result[2]);
    }

    /// <summary>
    /// Testaa, että ReconstructPath (Dictionary-versio) palauttaa oikean reitin
    /// </summary>
    [Fact]
    public void ReconstructPath_DictVersion_ReturnsCorrectPath()
    {
        var cameFrom = new Dictionary<(int x, int y), (int x, int y)>();
        cameFrom[(0, 2)] = (0, 1);
        cameFrom[(0, 1)] = (0, 0);

        var start = (0, 0);
        var end = (0, 2);

        var result = PathUtils.ReconstructPath(cameFrom, start, end);

        Assert.NotNull(result);
        Assert.Equal(3, result.Count);
        Assert.Equal(new Node(0, 0), result[0]);
        Assert.Equal(new Node(0, 1), result[1]);
        Assert.Equal(new Node(0, 2), result[2]);
    }

    /// <summary>
    /// Testaa, että GetLineBetween palauttaa oikeat pisteet
    /// </summary>
    [Fact]
    public void GetLineBetween_ReturnsIntermediatePoints()
    {
        int startX = 0, startY = 0, endX = 2, endY = 2;

        var line = PathUtils.GetLineBetween(startX, startY, endX, endY);

        Assert.Single(line);
        Assert.Equal(new Node(1, 1), line[0]);
    }

    /// <summary>
    /// Testaa, että ExtractVisitedNodes (Dictionary-versio) palauttaa kaikki avaimet
    /// </summary>
    [Fact]
    public void ExtractVisitedNodes_DictVersion_ReturnsAllKeysAsNodes()
    {
        var gScore = new Dictionary<(int x, int y), double>
        {
            { (0,0), 10 },
            { (1,1), 20 },
            { (2,2), 30 }
        };

        var visited = PathUtils
            .ExtractVisitedNodes(gScore)
            .OrderBy(n => (n.X, n.Y))
            .ToList();

        Assert.Equal(3, visited.Count);
        Assert.Equal(new Node(0, 0), visited[0]);
        Assert.Equal(new Node(1, 1), visited[1]);
        Assert.Equal(new Node(2, 2), visited[2]);
    }

    /// <summary>
    /// Testaa, että ExtractVisitedNodes (double[,], PriorityQueue) suodattaa pois jonossa olevat solmut
    /// </summary>
    [Fact]
    public void ExtractVisitedNodes_ArrayVersion_FiltersOutOpenSet()
    {
        var scoreArray = new double[3, 3];
        for (int x = 0; x < 3; x++)
        {
            for (int y = 0; y < 3; y++)
            {
                scoreArray[x, y] = (x == 2 && y == 2) ? double.MaxValue : 1;
            }
        }

        var openSet = new PriorityQueue<Node, double>();
        openSet.Enqueue(new Node(1, 1), 1);

        var visited = PathUtils
            .ExtractVisitedNodes(scoreArray, openSet)
            .OrderBy(n => (n.X, n.Y))
            .ToList();

        Assert.Equal(7, visited.Count);
        Assert.DoesNotContain(new Node(1, 1), visited);
    }
}
