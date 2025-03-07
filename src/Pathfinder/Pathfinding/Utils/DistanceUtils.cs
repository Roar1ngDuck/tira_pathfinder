using System;

namespace Pathfinder.Pathfinding.Utils;

public class DistanceUtils
{
    /// <summary>
    /// Palauttaa etäisyyden pisteestä a pisteeseen b, kun liikkua saa vain pysty- ja vaakasuunnassa.
    /// </summary>
    /// <param name="a">Ensimmäinen pisteiden.</param>
    /// <param name="b">Toinen pisteiden.</param>
    /// <returns>Manhattan-etäisyys pisteiden välillä.</returns>
    public static double ManhattanDistance(Node a, Node b)
    {
        return Math.Abs(a.X - b.X) + Math.Abs(a.Y - b.Y);
    }

    /// <summary>
    /// Palauttaa etäisyyden pisteestä a pisteeseen b
    /// </summary>
    /// <param name="a">Ensimmäinen pisteiden.</param>
    /// <param name="b">Toinen pisteiden.</param>
    /// <returns>Etäisyys pisteestä a pisteeseen b</returns>
    public static double EuclideanDistance(Node a, Node b)
    {
        return EuclideanDistance(a.X, a.Y, b.X, b.Y);
    }

    /// <summary>
    /// Palauttaa etäisyyden pisteestä 1 pisteeseen 2, kun saa liikkua vinottain.
    /// </summary>
    /// <param name="x1">Ensimmäisen pisteen x-koordinaatti.</param>
    /// <param name="y1">Ensimmäisen pisteen y-koordinaatti.</param>
    /// <param name="x2">Toisen pisteen x-koordinaatti.</param>
    /// <param name="y2">Toisen pisteen y-koordinaatti.</param>
    /// <returns>Arvioitu etäisyys pisteiden välillä kahdeksaan suuntaan liikuttaessa.</returns>
    public static double EuclideanDistance(int x1, int y1, int x2, int y2)
    {
        double dx = x1 - x2;
        double dy = y1 - y2;
        return Math.Sqrt(dx * dx + dy * dy);
    }
}
