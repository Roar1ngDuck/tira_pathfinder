using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;

namespace Pathfinder.Pathfinding;

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
    public PathFindingResult Search(Node start, Node goal, bool allowDiagonal, Action<IEnumerable<Node>, List<Node>, Node>? callbackFunc, TimeSpan stepDelay)
    {
        var openSet = new PriorityQueue<Node, double>();
        openSet.Enqueue(start, 0);

        var width = _map.GetLength(0);
        var height = _map.GetLength(1);
        var cameFrom = new Node?[width, height];
        var gScore = new double[width, height];

        InitScoresToMaxValue(ref gScore);

        gScore[start.X, start.Y] = 0;

        var timingStopwatch = Stopwatch.StartNew();
        long timingNodeCounter = 0;

        Span<(Node neighbor, double cost)> neighbors = new (Node, double)[8];

        while (openSet.TryDequeue(out var current, out var priority))
        {
            CallCallbackIfNeeded(ref callbackFunc, ref gScore, ref openSet, ref current);

            if (current == goal)
            {
                var path = Helpers.ReconstructPath(cameFrom, current);
                return new PathFindingResult(ExtractVisitedNodes(gScore, openSet), path);
            }

            int neighborCount = Helpers.GetNeighbors(_map, current, allowDiagonal, neighbors);
            ProcessNodeNeighbors(ref neighbors, neighborCount, ref gScore, ref cameFrom, ref openSet, ref current, ref goal);

            DelayIfNeeded(ref stepDelay, ref timingStopwatch, ref timingNodeCounter);
            timingNodeCounter++;
        }

        return new PathFindingResult(ExtractVisitedNodes(gScore, openSet), null);
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
    /// Kutsuu callback funktion jos se on olemassa ja jos ShouldCallCallback on tosi.
    /// </summary>
    /// <param name="callbackFunc"></param>
    /// <param name="gScore"></param>
    /// <param name="fScore"></param>
    /// <param name="openSet"></param>
    /// <param name="current"></param>
    private static void CallCallbackIfNeeded(ref Action<IEnumerable<Node>, List<Node>, Node>? callbackFunc, ref double[,] gScore, ref PriorityQueue<Node, double> openSet, ref Node current)
    {
        if (callbackFunc != null && MainWindow.ShouldCallCallback)
        {
            var visited = ExtractVisitedNodes(gScore, openSet).ToList();
            var queue = openSet.UnorderedItems.Select(item => item.Element).ToList();
            callbackFunc(visited, queue, current);
        }
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
    /// Jos haluttu viive on asetettu niin laskee kuluneen ajan perusteella sopivan ajan ja odottaa.
    /// </summary>
    /// <param name="stepDelay">Haluttu keskimääräinen viive jokaisen pisteen käsittelylle</param>
    /// <param name="timingStopwatch">Stopwatch joka mittaa algoritmin käynnissä ollessa kuluneen ajan</param>
    /// <param name="nodeCounter">Käsiteltyjen pisteiden määrä yhteensä</param>
    private static void DelayIfNeeded(ref TimeSpan stepDelay, ref Stopwatch timingStopwatch, ref long nodeCounter)
    {
        if (stepDelay.TotalMilliseconds > 0)
        {
            double elapsedMs = timingStopwatch.Elapsed.TotalMilliseconds;
            double targetDelay = nodeCounter * stepDelay.TotalMilliseconds;
            if (elapsedMs < targetDelay)
            {
                Thread.Sleep((int)Math.Max(1, targetDelay - elapsedMs));
            }
        }
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
        return Search(start, goal, allowDiagonal, null, TimeSpan.Zero);
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
}
