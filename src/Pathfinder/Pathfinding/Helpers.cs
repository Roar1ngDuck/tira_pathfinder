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
            int x = node.X + dx;
            int y = node.Y + dy;

            if (x >= 0 && x < width &&
                y >= 0 && y < height &&
                map[x, y] == 0)
            {
                neighbors[count++] = (new Node(x, y), cost);
            }
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
