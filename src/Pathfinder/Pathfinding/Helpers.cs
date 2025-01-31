using HarfBuzzSharp;
using System;
using System.Collections.Generic;
using System.Xml.Linq;

namespace Pathfinder.Pathfinding;

public class Helpers
{
    private static readonly (int, int, double)[] directionsStraight = [(-1, 0, 1), (1, 0, 1), (0, -1, 1), (0, 1, 1)];

    private static readonly(int, int, double)[] directionsDiagonal = [
        (-1, 0, 1), (1, 0, 1), (0, -1, 1), (0, 1, 1), // Suoraan
        (-1, -1, Math.Sqrt(2)), (-1, 1, Math.Sqrt(2)), (1, -1, Math.Sqrt(2)), (1, 1, Math.Sqrt(2)) // Vinottain
    ];

    /// <summary>
    /// Etsii kaikki pisteen validit naapurit kartassa
    /// </summary>
    /// <param name="map">Pikselikartta</param>
    /// <param name="node">Piste jonka naapurit haetaan</param>
    /// <param name="allowDiagonal">Sallitaanko vinottaiset siirrot</param>
    /// <param name="neighbors">Lista johon naapurit kirjoitetaan</param>
    /// <returns></returns>
    public static int GetNeighbors(int[,] map, Node node, bool allowDiagonal, Span<(Node, double)> neighbors)
    {
        var directions = allowDiagonal ? directionsDiagonal : directionsStraight;
        int count = 0;
        var width = map.GetLength(0);
        var height = map.GetLength(1);

        foreach (var (dx, dy, cost) in directions)
        {
            int nx = node.X + dx;
            int ny = node.Y + dy;

            if (nx < 0 || nx >= width || ny < 0 || ny >= height)
            {
                continue;
            }
            if (map[nx, ny] != 0)
            {
                continue;
            }

            if (allowDiagonal && dx != 0 && dy != 0)
            {
                int checkX1 = node.X + dx;
                int checkY1 = node.Y;
                int checkX2 = node.X;
                int checkY2 = node.Y + dy;

                // Diagonaalista siirtoa ei sallista jos siirrytt‰v‰n ruudun kummallakin puolella on sein‰
                if (map[checkX1, checkY1] != 0 && map[checkX2, checkY2] != 0)
                {
                    continue;
                }
            }

            neighbors[count++] = (new Node(nx, ny), cost);
        }

        return count;
    }

    /// <summary>
    /// Palauttaa kuljetun reitin kyseiseen pisteeseen.
    /// </summary>
    /// <param name="cameFrom">Taulukko tiedosta mist‰ pisteest‰ on p‰‰sty mihin</param>
    /// <param name="current">Valittu piste johon kuljettu reitti m‰‰ritet‰‰n</param>
    /// <returns>Lista kuljetusta reitist‰ alkaen alkupisteest‰</returns>
    public static List<Node> ReconstructPath(Node?[,] cameFrom, Node current)
    {
        var totalPath = new List<Node> { current };
        while (current is not null && cameFrom[current.X, current.Y] is not null)
        {
            current = cameFrom[current.X, current.Y];

            if (current is not null)
            {
                totalPath.Insert(0, current);
            }
        }
        return totalPath;
    }
}
