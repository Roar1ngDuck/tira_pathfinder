using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using static System.Formats.Asn1.AsnWriter;

namespace Pathfinder.Pathfinding;

/// <summary>
/// A* algoritmi, joka toimii pikselikartoilla
/// </summary>
/// <param name="map">Pikselikartta hakua varten</param>
public class AStar(int[,] map) : IPathFindingAlgorithm
{
    private readonly int[,] _map = map;

    /// <summary>
    /// Etsii lyhyimm�n reitin kartassa l�ht�pisteest� maalipisteeseen.
    /// </summary>
    /// <param name="start">L�ht�piste</param>
    /// <param name="goal">Maalipiste</param>
    /// <param name="allowDiagonal">Sallitaanko vinottaiset siirrot kartassa</param>
    /// <param name="callbackFunc">Kutsutaan ennen jokaisen pisteen prosessointia</param>
    /// <param name="stepDelay">Haluttu keskim��r�inen viive jokaisen pisteen k�sittelylle</param>
    /// <returns>PathFindingResult olio joka sis�lt�� reitin sek� kaikki l�pi k�ydyt pisteet</returns>
    public PathFindingResult Search(Node start, Node goal, bool allowDiagonal, Action<IEnumerable<Node>, List<Node>, Node>? callbackFunc, TimeSpan stepDelay)
    {
        var openSet = new PriorityQueue<Node, double>();
        openSet.Enqueue(start, 0);

        var width = _map.GetLength(0);
        var height = _map.GetLength(1);
        var cameFrom = new Node?[width, height];
        var gScore = new double[width, height];
        var fScore = new double[width, height];

        Func<Node, Node, double> heurestic = allowDiagonal ? EuclideanDistance : ManhattanDistance;

        InitScoresToMaxValue(ref gScore, ref fScore);

        gScore[start.X, start.Y] = 0;
        fScore[start.X, start.Y] = heurestic(start, goal);

        var timingStopwatch = Stopwatch.StartNew();
        long timingNodeCounter = 0;

        while (openSet.TryDequeue(out var current, out var priority))
        {
            CallCallbackIfNeeded(ref callbackFunc, ref gScore, ref openSet, ref current);

            if (current == goal)
            {
                var path = Helpers.ReconstructPath(cameFrom, current);
                return new PathFindingResult(ExtractVisitedNodes(gScore, openSet), path);
            }

            List<(Node neighbor, double cost)> neighbors = Helpers.GetNeighbors(_map, current, allowDiagonal);
            ProcessNodeNeighbors(ref neighbors, ref gScore, ref fScore, ref cameFrom, ref openSet, ref current, ref heurestic, ref goal);

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
    /// <param name="gScore"></param>
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
    /// Lis�� kaikki naapurit jotka johtavat lyhyemp��n reittiin jonoon
    /// </summary>
    /// <param name="neighbors"></param>
    /// <param name="gScore"></param>
    /// <param name="fScore"></param>
    /// <param name="cameFrom"></param>
    /// <param name="openSet"></param>
    /// <param name="current"></param>
    /// <param name="heurestic"></param>
    /// <param name="goal"></param>
    private static void ProcessNodeNeighbors(ref List<(Node neighbor, double cost)> neighbors, ref double[,] gScore, ref double[,] fScore, ref Node?[,] cameFrom, ref PriorityQueue<Node, double> openSet, ref Node current, ref Func<Node, Node, double> heurestic, ref Node goal)
    {
        foreach (var (neighbor, cost) in neighbors)
        {
            double tentative_gScore = gScore[current.X, current.Y] + cost;

            if (tentative_gScore < gScore[neighbor.X, neighbor.Y])
            {
                cameFrom[neighbor.X, neighbor.Y] = current;
                gScore[neighbor.X, neighbor.Y] = tentative_gScore;
                var tentativeAndHeurestic = tentative_gScore + heurestic(neighbor, goal);
                fScore[neighbor.X, neighbor.Y] = tentativeAndHeurestic;

                openSet.Enqueue(neighbor, fScore[neighbor.X, neighbor.Y]);
            }
        }
    }

    /// <summary>
    /// Jos haluttu viive on asetettu niin laskee kuluneen ajan perusteella sopivan ajan ja odottaa.
    /// </summary>
    /// <param name="stepDelay">Haluttu keskim��r�inen viive jokaisen pisteen k�sittelylle</param>
    /// <param name="timingStopwatch">Stopwatch joka mittaa algoritmin k�ynniss� ollessa kuluneen ajan</param>
    /// <param name="nodeCounter">K�siteltyjen pisteiden m��r� yhteens�</param>
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
    /// Etsii lyhyimm�n reitin kartassa l�ht�pisteest� maalipisteeseen.
    /// </summary>
    /// <param name="start">L�ht�piste</param>
    /// <param name="goal">Maalipiste</param>
    /// <param name="allowDiagonal">Sallitaanko vinottaiset siirrot kartassa</param>
    /// <returns>PathFindingResult olio joka sis�lt�� reitin sek� kaikki l�pi k�ydyt pisteet</returns>
    public PathFindingResult Search(Node start, Node goal, bool allowDiagonal)
    {
        return Search(start, goal, allowDiagonal, null, TimeSpan.Zero);
    }

    /// <summary>
    /// Palauttaa et�isyyden pistees� a pisteeseen b kun voidaan kulkea vain pysty- ja vaakasuoraan.
    /// </summary>
    /// <param name="a"></param>
    /// <param name="b"></param>
    /// <returns></returns>
    private static double ManhattanDistance(Node a, Node b)
    {
        return Math.Abs(a.X - b.X) + Math.Abs(a.Y - b.Y);
    }

    /// <summary>
    /// Palauttaa et�isyyden pisteest� a pisteeseen b.
    /// </summary>
    /// <param name="a"></param>
    /// <param name="b"></param>
    /// <returns></returns>
    private static double EuclideanDistance(Node a, Node b)
    {
        return Math.Sqrt(Math.Pow(a.X - b.X, 2) + Math.Pow(a.Y - b.Y, 2));
    }

    /// <summary>
    /// Palauttaa listan l�pik�ydyist� pisteist�
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
