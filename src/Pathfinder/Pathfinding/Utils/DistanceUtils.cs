using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pathfinder.Pathfinding.Utils
{
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
        /// Palauttaa approximaation etäisyydestä pisteestä a pisteeseen b, kun saa liikkua vinottain. Hieman nopeampi laskea kuin EuclideanDistance.
        /// </summary>
        /// <param name="a">Ensimmäinen pisteiden.</param>
        /// <param name="b">Toinen pisteiden.</param>
        /// <returns>Arvioitu etäisyys pisteiden välillä kahdeksaan suuntaan liikuttaessa.</returns>
        public static double OctagonalDistance(Node a, Node b)
        {
            return OctagonalDistance(a.X, a.Y, b.X, b.Y);
        }

        /// <summary>
        /// Palauttaa approximaation etäisyydestä pisteestä 1 pisteeseen 2, kun saa liikkua vinottain. Hieman nopeampi laskea kuin EuclideanDistance.
        /// </summary>
        /// <param name="x1">Ensimmäisen pisteen x-koordinaatti.</param>
        /// <param name="y1">Ensimmäisen pisteen y-koordinaatti.</param>
        /// <param name="x2">Toisen pisteen x-koordinaatti.</param>
        /// <param name="y2">Toisen pisteen y-koordinaatti.</param>
        /// <returns>Arvioitu etäisyys pisteiden välillä kahdeksaan suuntaan liikuttaessa.</returns>
        public static double OctagonalDistance(int x1, int y1, int x2, int y2)
        {
            var dx = Math.Abs(x1 - x2);
            var dy = Math.Abs(y1 - y2);
            return Math.Max(dx, dy) + (Math.Sqrt(2) - 1) * Math.Min(dx, dy);
        }

        /// <summary>
        /// Palauttaa etäisyyden pisteestä a pisteeseen b
        /// </summary>
        /// <param name="a">Ensimmäinen pisteiden.</param>
        /// <param name="b">Toinen pisteiden.</param>
        /// <returns>Etäisyys pisteestä a pisteeseen b</returns>
        public static double EuclideanDistance(Node a, Node b)
        {
            double dx = a.X - b.X;
            double dy = a.Y - b.Y;
            return Math.Sqrt(dx * dx + dy * dy);
        }
    }
}
