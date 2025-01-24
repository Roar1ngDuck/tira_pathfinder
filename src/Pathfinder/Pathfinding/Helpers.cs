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
    /// <returns></returns>
    public static List<(Node neighbor, double cost)> GetNeighbors(int[,] map, Node node, bool allowDiagonal)
    {
        var neighbors = new List<(Node, double)>();

        var directions = allowDiagonal ? directionsDiagonal : directionsStraight;

        foreach (var (dr, dc, cost) in directions)
        {
            int newRow = node.X + dr;
            int newCol = node.Y + dc;

            if (newRow >= 0 && newRow < map.GetLength(0) &&
                newCol >= 0 && newCol < map.GetLength(1) &&
                map[newRow, newCol] == 0)
            {
                neighbors.Add((new Node(newRow, newCol), cost));
            }
        }

        return neighbors;
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
        while (cameFrom[current.X, current.Y] != null)
        {
            current = cameFrom[current.X, current.Y]!.Value;
            totalPath.Insert(0, current);
        }
        return totalPath;
    }
}
