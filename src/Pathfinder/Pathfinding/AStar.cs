using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;

namespace Pathfinder.Pathfinding;

/// <summary>
/// A* algoritmi, joka toimii pikselikartoilla
/// </summary>
/// <param name="map">Pikselikartta hakua varten</param>
public class AStar(int[,] map) : IPathFindingAlgorithm
{
    private readonly int[,] _map = map;

    /// <summary>
    /// Etsii lyhyimmän reitin kartassa lähtäpisteestä maalipisteeseen.
    /// </summary>
    /// <param name="start">Lähtäpiste</param>
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
        var fScore = new double[width, height];

        Func<Node, Node, double> heurestic = allowDiagonal ? OctagonalDistance : ManhattanDistance;

        InitScoresToMaxValue(ref gScore, ref fScore);

        gScore[start.X, start.Y] = 0;
        fScore[start.X, start.Y] = heurestic(start, goal);

        var timingStopwatch = Stopwatch.StartNew();
        long timingNodeCounter = 0;

        Span<(Node neighbor, double cost)> neighbors = new (Node, double)[8];

        while (openSet.TryDequeue(out var current, out var priority))
        {
            CallCallbackIfNeeded(ref callbackFunc, ref fScore, ref openSet, ref current);

            if (current == goal)
            {
                var path = Helpers.ReconstructPath(cameFrom, current);
                return new PathFindingResult(ExtractVisitedNodes(fScore, openSet), path);
            }

            int neighborCount = Helpers.GetNeighbors(_map, current, allowDiagonal, neighbors);
            ProcessNodeNeighbors(ref neighbors, neighborCount, ref gScore, ref fScore, ref cameFrom, ref openSet, ref current, ref heurestic, ref goal);

            DelayIfNeeded(ref stepDelay, ref timingStopwatch, ref timingNodeCounter);
            timingNodeCounter++;
        }

        return new PathFindingResult(ExtractVisitedNodes(fScore, openSet), null);
    }

    /// <summary>
    /// Asettaa gScore ja fScore taulukoiden arvot maksimiin
    /// </summary>
    /// <param name="gScore"></param>
    /// <param name="fScore"></param>
    private static void InitScoresToMaxValue(ref double[,] gScore, ref double[,] fScore)
    {
        Span<double> gSpan = MemoryMarshal.CreateSpan(ref gScore[0, 0], gScore.Length);
        Span<double> fSpan = MemoryMarshal.CreateSpan(ref fScore[0, 0], fScore.Length);

        gSpan.Fill(double.MaxValue);
        fSpan.Fill(double.MaxValue);
    }

    /// <summary>
    /// Kutsuu callback funktion jos se on olemassa ja jos ShouldCallCallback on tosi.
    /// </summary>
    /// <param name="callbackFunc"></param>
    /// <param name="fScore"></param>
    /// <param name="openSet"></param>
    /// <param name="current"></param>
    private static void CallCallbackIfNeeded(ref Action<IEnumerable<Node>, List<Node>, Node>? callbackFunc, ref double[,] fScore, ref PriorityQueue<Node, double> openSet, ref Node current)
    {
        if (callbackFunc != null && MainWindow.ShouldCallCallback)
        {
            var visited = ExtractVisitedNodes(fScore, openSet).ToList();
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
    /// <param name="fScore"></param>
    /// <param name="cameFrom"></param>
    /// <param name="openSet"></param>
    /// <param name="current"></param>
    /// <param name="heurestic"></param>
    /// <param name="goal"></param>
    private static void ProcessNodeNeighbors(ref Span<(Node neighbor, double cost)> neighbors, int neighborCount, ref double[,] gScore, ref double[,] fScore, ref Node?[,] cameFrom, ref PriorityQueue<Node, double> openSet, ref Node current, ref Func<Node, Node, double> heurestic, ref Node goal)
    {
        for (int i = 0; i < neighborCount; i++)
        {
            var (neighbor, cost) = neighbors[i];
            double tentative_gScore = gScore[current.X, current.Y] + cost;

            if (tentative_gScore < gScore[neighbor.X, neighbor.Y])
            {
                cameFrom[neighbor.X, neighbor.Y] = current;
                gScore[neighbor.X, neighbor.Y] = tentative_gScore;
                fScore[neighbor.X, neighbor.Y] = tentative_gScore + heurestic(neighbor, goal);

                openSet.Enqueue(neighbor, fScore[neighbor.X, neighbor.Y]);
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
    /// Etsii lyhyimmän reitin kartassa lähtäpisteestä maalipisteeseen.
    /// </summary>
    /// <param name="start">Lähtäpiste</param>
    /// <param name="goal">Maalipiste</param>
    /// <param name="allowDiagonal">Sallitaanko vinottaiset siirrot kartassa</param>
    /// <returns>PathFindingResult olio joka sisältää reitin sekä kaikki läpi käydyt pisteet</returns>
    public PathFindingResult Search(Node start, Node goal, bool allowDiagonal)
    {
        return Search(start, goal, allowDiagonal, null, TimeSpan.Zero);
    }

    /// <summary>
    /// Palauttaa etäisyyden pisteesä a pisteeseen b kun voidaan kulkea vain pysty- ja vaakasuoraan.
    /// </summary>
    /// <param name="a"></param>
    /// <param name="b"></param>
    /// <returns></returns>
    private static double ManhattanDistance(Node a, Node b)
    {
        return Math.Abs(a.X - b.X) + Math.Abs(a.Y - b.Y);
    }

    /// <summary>
    /// Palauttaa etäisyyden pisteestä a pisteeseen b.
    /// </summary>
    /// <param name="a"></param>
    /// <param name="b"></param>
    /// <returns></returns>
    private static double EuclideanDistance(Node a, Node b)
    {
        return Math.Sqrt(Math.Pow(a.X - b.X, 2) + Math.Pow(a.Y - b.Y, 2));
    }

    /// <summary>
    /// Palauttaa etäisyyden pisteestä a pisteeseen b kun voidaan liikkua 8 suuntaan.
    /// </summary>
    /// <param name="a"></param>
    /// <param name="b"></param>
    /// <returns></returns>
    private static double OctagonalDistance(Node a, Node b)
    {
        double dx = Math.Abs(a.X - b.X);
        double dy = Math.Abs(a.Y - b.Y);
        return Math.Max(dx, dy) + (Math.Sqrt(2) - 1) * Math.Min(dx, dy);
    }

    /// <summary>
    /// Palauttaa listan läpikäydyistä pisteistä
    /// </summary>
    /// <param name="fScore"></param>
    /// <param name="openSet"></param>
    /// <returns></returns>
    private static IEnumerable<Node> ExtractVisitedNodes(double[,] fScore, PriorityQueue<Node, double> openSet)
    {
        var visitedNodes = new List<(Node, double)>();

        for (int x = 0; x < fScore.GetLength(0); x++)
        {
            for (int y = 0; y < fScore.GetLength(1); y++)
            {
                if (fScore[x, y] != double.MaxValue)
                {
                    visitedNodes.Add((new Node(x, y), fScore[x, y]));
                }
            }
        }

        var inQueue = openSet.UnorderedItems;
        return visitedNodes.Except(inQueue).Select(item => item.Item1);
    }
}
