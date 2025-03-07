using System;
using System.Runtime.InteropServices;

namespace Pathfinder.Pathfinding.Utils;

public class ArrayUtils
{
    /// <summary>
    /// Alustaa gScore- tai fScore-taulukon asettamalla ne double.MaxValue.
    /// </summary>
    /// <param name="scoreArray">2D-taulukko, jonka arvot halutaan täyttää.</param>
    public static void InitArrayToMaxValue(double[,] scoreArray)
    {
        Span<double> span = MemoryMarshal.CreateSpan(ref scoreArray[0, 0], scoreArray.Length);
        span.Fill(double.MaxValue);
    }
}
