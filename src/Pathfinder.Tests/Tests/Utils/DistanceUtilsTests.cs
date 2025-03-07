using Pathfinder.Pathfinding;
using Pathfinder.Pathfinding.Utils;

namespace Pathfinder.Tests.Tests.Utils;

public class DistanceUtilsTests
{
    /// <summary>
    /// Testaa, että ManhattanDistance palauttaa oikean arvon
    /// </summary>
    [Fact]
    public void ManhattanDistance_ReturnsCorrectDistance()
    {
        var a = new Node(0, 0);
        var b = new Node(3, 4);

        double result = DistanceUtils.ManhattanDistance(a, b);

        Assert.Equal(7, result);
    }

    /// <summary>
    /// Testaa, että EuclideanDistance palauttaa 0 samalle solmulle
    /// </summary>
    [Fact]
    public void EuclideanDistance_NodeVersion_ReturnsZeroIfIdentical()
    {
        var a = new Node(5, 5);

        double result = DistanceUtils.EuclideanDistance(a, a);

        Assert.Equal(0, result);
    }

    /// <summary>
    /// Testaa, että EuclideanDistance palauttaa oikean arvon eri solmuille
    /// </summary>
    [Fact]
    public void EuclideanDistance_NodeVersion_ReturnsCorrectDistance()
    {
        var a = new Node(0, 0);
        var b = new Node(3, 4);

        double result = DistanceUtils.EuclideanDistance(a, b);

        Assert.Equal(5, result, precision: 5);
    }

    /// <summary>
    /// Testaa, että EuclideanDistance palauttaa oikean arvon, kun käytetään koordinaatteja
    /// </summary>
    [Fact]
    public void EuclideanDistance_IntVersion_ReturnsCorrectDistance()
    {
        int x1 = 0, y1 = 0, x2 = 6, y2 = 8;

        double result = DistanceUtils.EuclideanDistance(x1, y1, x2, y2);

        Assert.Equal(10, result, precision: 5);
    }
}
