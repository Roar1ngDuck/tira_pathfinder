using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using static System.Formats.Asn1.AsnWriter;

namespace Pathfinder.Pathfinding.Algorithms;

/// <summary>
/// Dijkstran algoritmi, joka toimii pikselikartoilla
/// </summary>
/// <param name="map">Pikselikartta hakua varten</param>
public class Dijkstra(int[,] map) : IPathFindingAlgorithm
{
    private readonly int[,] _map = map;

    /// <summary>
    /// Etsii lyhyimmän reitin kartassa lähtöpisteestä maalipisteeseen.
    /// </summary>
    /// <param name="start">Lähtöpiste</param>
    /// <param name="goal">Maalipiste</param>
    /// <param name="allowDiagonal">Sallitaanko vinottaiset siirrot kartassa</param>
    /// <param name="callbackFunc">Kutsutaan ennen jokaisen pisteen prosessointia</param>
    /// <param name="stepDelay">Haluttu keskimääräinen viive jokaisen pisteen käsittelylle</param>
    /// <returns>PathFindingResult olio joka sisältää reitin sekä kaikki läpi käydyt pisteet</returns>
    public PathFindingResult Search(Node start, Node goal, bool allowDiagonal, Action<IEnumerable<Node>, List<Node>, Node>? callbackFunc, StepDelay stepDelay)
    {
        var openSet = new PriorityQueue<Node, double>();
        openSet.Enqueue(start, 0);

        var width = _map.GetLength(0);
        var height = _map.GetLength(1);
        var cameFrom = new Node?[width, height];
        var gScore = new double[width, height];

        InitScoresToMaxValue(ref gScore);

        gScore[start.X, start.Y] = 0;

        Span<(Node neighbor, double cost)> neighbors = new (Node, double)[8];

        while (openSet.TryDequeue(out var current, out var priority))
        {
            callbackFunc?.Invoke(
                ExtractVisitedNodes(gScore, openSet).ToList(),
                openSet.UnorderedItems.Select(item => item.Element).ToList(),
                current);

            if (current == goal)
            {
                var path = ReconstructPath(cameFrom, current);
                return new PathFindingResult(ExtractVisitedNodes(gScore, openSet), path);
            }

            int neighborCount = GetNeighbors(_map, current, allowDiagonal, neighbors);
            ProcessNodeNeighbors(ref neighbors, neighborCount, ref gScore, ref cameFrom, ref openSet, ref current, ref goal);

            stepDelay?.Wait();
        }

        return new PathFindingResult(ExtractVisitedNodes(gScore, openSet), null);
    }

    /// <summary>
    /// Etsii lyhyimmän reitin kartassa lähtöpisteestä maalipisteeseen.
    /// </summary>
    /// <param name="start">Lähtöpiste</param>
    /// <param name="goal">Maalipiste</param>
    /// <param name="allowDiagonal">Sallitaanko vinottaiset siirrot kartassa</param>
    /// <returns>PathFindingResult olio joka sisältää reitin sekä kaikki läpi käydyt pisteet</returns>
    public PathFindingResult Search(Node start, Node goal, bool allowDiagonal)
    {
        return Search(start, goal, allowDiagonal, null, null);
    }

    /// <summary>
    /// Asettaa gScore ja fScore taulukoiden arvot maksimiin
    /// </summary>
    /// <param name="gScore"></param>
    /// <param name="fScore"></param>
    private static void InitScoresToMaxValue(ref double[,] gScore)
    {
        Span<double> gSpan = MemoryMarshal.CreateSpan(ref gScore[0, 0], gScore.Length);

        gSpan.Fill(double.MaxValue);
    }

    /// <summary>
    /// Lisää kaikki naapurit jotka johtavat lyhyempään reittiin jonoon
    /// </summary>
    /// <param name="neighbors"></param>
    /// <param name="neighborCount"></param>
    /// <param name="gScore"></param>
    /// <param name="cameFrom"></param>
    /// <param name="openSet"></param>
    /// <param name="current"></param>
    /// <param name="goal"></param>
    private static void ProcessNodeNeighbors(ref Span<(Node neighbor, double cost)> neighbors, int neighborCount, ref double[,] gScore, ref Node?[,] cameFrom, ref PriorityQueue<Node, double> openSet, ref Node current, ref Node goal)
    {
        for (int i = 0; i < neighborCount; i++)
        {
            var (neighbor, cost) = neighbors[i];
            double tentative_gScore = gScore[current.X, current.Y] + cost;

            if (tentative_gScore < gScore[neighbor.X, neighbor.Y])
            {
                cameFrom[neighbor.X, neighbor.Y] = current;
                gScore[neighbor.X, neighbor.Y] = tentative_gScore;

                openSet.Enqueue(neighbor, gScore[neighbor.X, neighbor.Y]);
            }
        }
    }

    /// <summary>
    /// Palauttaa listan läpikäydyistä pisteistä
    /// </summary>
    /// <param name="gScore"></param>
    /// <param name="openSet"></param>
    /// <returns></returns>
    private static IEnumerable<Node> ExtractVisitedNodes(double[,] gScore, PriorityQueue<Node, double> openSet)
    {
        var visitedNodes = new List<Node>();
        for (int x = 0; x < gScore.GetLength(0); x++)
        {
            for (int y = 0; y < gScore.GetLength(1); y++)
            {
                if (gScore[x, y] != double.MaxValue)
                {
                    visitedNodes.Add(new Node(x, y));
                }
            }
        }
        var inQueue = openSet.UnorderedItems.Select(item => item.Element);
        return visitedNodes.Except(inQueue);
    }

    private static readonly (int, int, double)[] directionsStraight = [(-1, 0, 1), (1, 0, 1), (0, -1, 1), (0, 1, 1)];

    private static readonly (int, int, double)[] directionsDiagonal = [
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

                // Diagonaalista siirtoa ei sallista jos siirryttävän ruudun kummallakin puolella on seinä
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
    /// <param name="cameFrom">Taulukko tiedosta mistä pisteestä on päästy mihin</param>
    /// <param name="current">Valittu piste johon kuljettu reitti määritetään</param>
    /// <returns>Lista kuljetusta reitistä alkaen alkupisteestä</returns>
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
