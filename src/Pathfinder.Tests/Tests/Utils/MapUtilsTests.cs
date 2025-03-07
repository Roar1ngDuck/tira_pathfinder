using Pathfinder.Pathfinding.Utils;

namespace Pathfinder.Tests.Tests.Utils;

public class MapUtilsTests
{
    /// <summary>
    /// Testaa, että IsBlocked palauttaa true, jos liike menee ulos kartalta
    /// </summary>
    [Fact]
    public void IsBlocked_ReturnsTrueWhenOutOfBounds()
    {
        int[,] map =
        {
            {0, 0},
            {0, 0}
        };

        bool blocked = MapUtils.IsBlocked(map, 0, 0, -1, 0);

        Assert.True(blocked);
    }

    /// <summary>
    /// Testaa, että IsBlocked palauttaa true, jos seuraava ruutu on este
    /// </summary>
    [Fact]
    public void IsBlocked_ReturnsTrueWhenNextIsObstacle()
    {
        int[,] map =
        {
            {0, 1},
            {0, 0}
        };

        bool blocked = MapUtils.IsBlocked(map, 0, 0, 0, 1);

        Assert.True(blocked);
    }

    /// <summary>
    /// Testaa, että IsBlocked palauttaa true, jos diagonaalisesti kuljettaessa molemmat kulmat ovat esteitä
    /// </summary>
    [Fact]
    public void IsBlocked_ReturnsTrueWhenDiagonalCornersBlocked()
    {
        int[,] map =
        {
            {0, 1},
            {1, 0}
        };

        bool blocked = MapUtils.IsBlocked(map, 0, 0, 1, 1);

        Assert.True(blocked);
    }

    /// <summary>
    /// Testaa, että IsBlocked palauttaa false, kun liike on vapaa
    /// </summary>
    [Fact]
    public void IsBlocked_ReturnsFalseWhenFree()
    {
        int[,] map =
        {
            {0, 0},
            {0, 0}
        };

        bool blocked = MapUtils.IsBlocked(map, 0, 0, 1, 1);

        Assert.False(blocked);
    }
}
