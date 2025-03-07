using System;
using System.Collections.Generic;
using System.Linq;

namespace Pathfinder.Pathfinding.Utils;

public class PathUtils
{
    /// <summary>
    /// Rakentaa kuljetun reitin lopusta alkuun asti cameFrom-taulukon perusteella.
    /// </summary>
    /// <param name="cameFrom">Taulukko, joka kertoo jokaiselle solmulle, mistä solmusta siihen on tultu.</param>
    /// <param name="current">Nykyinen solmu, josta aloitetaan polun rekonstruointi.</param>
    /// <returns>Lista solmuista, jotka muodostavat reitin alkupisteestä maalipisteeseen.</returns>
    public static List<Node> ReconstructPath(Node?[,] cameFrom, Node current)
    {
        var totalPath = new List<Node> { current };

        while (cameFrom[current.X, current.Y] is Node newCurrent)
        {
            current = newCurrent;
            totalPath.Insert(0, current);
        }

        return totalPath;
    }

    /// <summary>
    /// Luo kuljetun reitin cameFrom sanakirjan perusteella.
    /// </summary>
    /// <param name="cameFrom">Sanakirja kuljettujen solmujen ketjusta.</param>
    /// <param name="start">Lähtöpisteen koordinaatti.</param>
    /// <param name="current">Nykyinen solmu, josta aloitetaan polun rekonstruointi.</param>
    /// <returns>Lista Node olioista, jotka muodostavat reitin alkupisteestä maalipisteeseen.</returns>
    public static List<Node> ReconstructPath(
        Dictionary<(int x, int y), (int x, int y)> cameFrom,
        (int x, int y) start,
        (int x, int y) current)
    {
        var path = new List<Node> { new Node(current.x, current.y) };
        var cur = current;

        while (cameFrom.ContainsKey(cur))
        {
            var parent = cameFrom[cur];

            var segment = GetLineBetween(parent.x, parent.y, cur.x, cur.y);
            path.AddRange(segment);

            path.Add(new Node(parent.x, parent.y));
            cur = parent;
        }

        path.Reverse();
        return path;
    }

    /// <summary>
    /// Palauttaa kaikki pisteet kahden pisteen välillä
    /// </summary>
    /// <param name="startX">Aloitusruudun x-koordinaatti.</param>
    /// <param name="startY">Aloitusruudun y-koordinaatti.</param>
    /// <param name="endX">Lopetusruudun x-koordinaatti.</param>
    /// <param name="endY">Lopetusruudun y-koordinaatti.</param>
    /// <returns>Lista Node olioita pisteiden väliltä.</returns>
    public static List<Node> GetLineBetween(int startX, int startY, int endX, int endY)
    {
        var result = new List<Node>();

        int dx = Math.Sign(endX - startX);
        int dy = Math.Sign(endY - startY);
        int steps = Math.Max(Math.Abs(endX - startX), Math.Abs(endY - startY));

        int x = startX;
        int y = startY;

        for (int i = 0; i < steps - 1; i++)
        {
            x += dx;
            y += dy;
            result.Add(new Node(x, y));
        }

        result.Reverse();
        return result;
    }


    /// Palauttaa kaikki solmut, joissa on käyty
    /// </summary>
    /// <param name="gScore">GScore sanakirja, josta luetaan arvot.</param>
    /// <returns>Lista Node olioita.</returns>
    public static IEnumerable<Node> ExtractVisitedNodes(Dictionary<(int x, int y), double> gScore)
    {
        return gScore.Keys.Select(p => new Node(p.x, p.y));
    }

    /// <summary>
    /// Palauttaa kaikki solmut, joissa on käyty
    /// </summary>
    /// <param name="score">FScore/GScore-taulukko, josta luetaan arvot.</param>
    /// <param name="openSet">Avoin solmu lista, joista osa voi olla vielä prosessoimatta.</param>
    /// <returns>Lista Node-olioita, joissa on käyty</returns>
    public static IEnumerable<Node> ExtractVisitedNodes(double[,] score, PriorityQueue<Node, double> openSet)
    {
        var visitedNodes = new List<(Node node, double score)>();

        // Käydään kaikki pisteet läpi ja lisätään ne, joilla fScore != double.MaxValue.
        for (int x = 0; x < score.GetLength(0); x++)
        {
            for (int y = 0; y < score.GetLength(1); y++)
            {
                if (score[x, y] != double.MaxValue)
                {
                    visitedNodes.Add((new Node(x, y), score[x, y]));
                }
            }
        }

        // Poistetaan joukosta ne, jotka ovat jonossa
        var inQueue = openSet.UnorderedItems;
        return visitedNodes.Except(inQueue).Select(item => item.Item1);
    }
}
