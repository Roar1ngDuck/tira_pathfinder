using Pathfinder.Pathfinding.Utils;
using Xunit;
using System;

namespace Pathfinder.Tests.Tests.Utils;

public class ArrayUtilsTests
{
    /// <summary>
    /// Testaa, että InitArrayToMaxValue täyttää taulukon double.MaxValue:lla
    /// </summary>
    [Fact]
    public void InitArrayToMaxValue_FillsWithMaxValue()
    {
        var scoreArray = new double[3, 3];

        ArrayUtils.InitArrayToMaxValue(scoreArray);

        for (int x = 0; x < scoreArray.GetLength(0); x++)
        {
            for (int y = 0; y < scoreArray.GetLength(1); y++)
            {
                Assert.Equal(double.MaxValue, scoreArray[x, y]);
            }
        }
    }
}
