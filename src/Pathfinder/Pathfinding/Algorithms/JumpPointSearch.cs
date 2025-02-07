using Pathfinder.Pathfinding;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace Pathfinder.Pathfinding.Algorithms;

/// <summary>
/// JPS algoritmi, joka toimii pikselikartoilla
/// </summary>
/// <param name="map">Pikselikartta hakua varten</param>
public class JumpPointSearch(int[,] map) : IPathFindingAlgorithm
{
    private readonly int[,] _map = map;
    private List<Node> _allVisited = new List<Node>();

    private static readonly (int dx, int dy)[] _directions = new (int dx, int dy)[]
    {
        (-1, 0), (1, 0), (0, -1), (0, 1),
        (-1, -1), (-1, 1), (1, -1), (1, 1)
    };

    /// <summary>
    /// Etsii lyhyimmän reitin kartassa lähtäpisteestä maalipisteeseen.
    /// </summary>
    /// <param name="start">Lähtöpiste</param>
    /// <param name="goal">Maalipiste</param>
    /// <param name="allowDiagonal">Sallitaanko vinottaiset siirrot kartassa</param>
    /// <param name="callbackFunc">Kutsutaan ennen jokaisen pisteen prosessointia</param>
    /// <param name="stepDelay">Haluttu keskimääräinen viive jokaisen pisteen käsittelylle</param>
    /// <returns>PathFindingResult olio joka sisältää reitin sekä kaikki läpi käydyt pisteet</returns>
    public PathFindingResult Search(
        Node start,
        Node goal,
        bool allowDiagonal,
        Action<IEnumerable<Node>, List<Node>, Node>? callbackFunc,
        StepDelay? stepDelay)
    {
        var (cameFrom, closedSet, gScore, fScore, openSet) = InitializeSearchDataStructures(start, goal);

        while (openSet.Count > 0)
        {
            var current = openSet.Dequeue();
            closedSet.Add(current);

            callbackFunc?.Invoke(
                ExtractVisitedNodes(gScore),
                openSet.UnorderedItems.Select(i => new Node(i.Element.x, i.Element.y)).Union(_allVisited).ToList(),
                new Node(current.x, current.y));

            var pathResult = CheckForGoalReached(current, goal, cameFrom, start, gScore);
            if (pathResult != null)
            {
                return pathResult;
            }

            ProcessNodeNeighbors(current, cameFrom, goal, closedSet, gScore, fScore, openSet);

            stepDelay?.Wait();
        }

        return new PathFindingResult(ExtractVisitedNodes(gScore), null);
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
        return Search(start, goal, allowDiagonal, null, null);
    }

    /// <summary>
    /// Alustaa ja määrittää hakualgoritmia varten tarvittavat tietorakenteet
    /// </summary>
    private (
        Dictionary<(int x, int y), (int x, int y)> cameFrom,
        HashSet<(int x, int y)> closedSet,
        Dictionary<(int x, int y), double> gScore,
        Dictionary<(int x, int y), double> fScore,
        PriorityQueue<(int x, int y), double> openSet
    ) InitializeSearchDataStructures(Node start, Node goal)
    {
        var cameFrom = new Dictionary<(int x, int y), (int x, int y)>();
        var closedSet = new HashSet<(int x, int y)>();
        var gScore = new Dictionary<(int x, int y), double>();
        var fScore = new Dictionary<(int x, int y), double>();
        var openSet = new PriorityQueue<(int x, int y), double>();

        var startPos = (start.X, start.Y);
        gScore[startPos] = 0;
        fScore[startPos] = OctagonalDistance(start.X, start.Y, goal.X, goal.Y);
        openSet.Enqueue(startPos, fScore[startPos]);

        return (cameFrom, closedSet, gScore, fScore, openSet);
    }

    /// <summary>
    /// Tarkistaa onko nykyinen sijainti maali ja palauttaa reitin jos on
    /// </summary>
    private PathFindingResult? CheckForGoalReached(
        (int x, int y) current,
        Node goal,
        Dictionary<(int x, int y), (int x, int y)> cameFrom,
        Node start,
        Dictionary<(int x, int y), double> gScore)
    {
        if (current.x == goal.X && current.y == goal.Y)
        {
            var path = ReconstructPath(cameFrom, start, current);
            return new PathFindingResult(ExtractVisitedNodes(gScore).Union(_allVisited), path);
        }
        return null;
    }

    /// <summary>
    /// Käsittelee kaikki nykyisen solmun seuraajat
    /// </summary>
    private void ProcessNodeNeighbors(
        (int x, int y) current,
        Dictionary<(int x, int y), (int x, int y)> cameFrom,
        Node goal,
        HashSet<(int x, int y)> closedSet,
        Dictionary<(int x, int y), double> gScore,
        Dictionary<(int x, int y), double> fScore,
        PriorityQueue<(int x, int y), double> openSet)
    {
        foreach (var successor in GetValidSuccessors(current, cameFrom, goal))
        {
            if (closedSet.Contains(successor)) continue;

            UpdateNodeScore(current, successor, gScore, fScore, goal, cameFrom, openSet);
        }
    }

    /// <summary>
    /// Päivittää solmun pisteet ja sijainnin jonossa annetulle solmulle
    /// </summary>
    private void UpdateNodeScore(
        (int x, int y) current,
        (int x, int y) successor,
        Dictionary<(int x, int y), double> gScore,
        Dictionary<(int x, int y), double> fScore,
        Node goal,
        Dictionary<(int x, int y), (int x, int y)> cameFrom,
        PriorityQueue<(int x, int y), double> openSet)
    {
        var tentativeG = gScore[current] + OctagonalDistance(current.x, current.y, successor.x, successor.y);

        if (!gScore.ContainsKey(successor) || tentativeG < gScore[successor])
        {
            cameFrom[successor] = current;
            gScore[successor] = tentativeG;
            fScore[successor] = tentativeG + OctagonalDistance(successor.x, successor.y, goal.X, goal.Y);
            openSet.Enqueue(successor, fScore[successor]);
        }
    }

    /// <summary>
    /// Hakee nykyisen solmun seuraajat
    /// </summary>
    private List<(int x, int y)> GetValidSuccessors(
        (int x, int y) current,
        Dictionary<(int x, int y), (int x, int y)> cameFrom,
        Node goal)
    {
        var successors = new List<(int, int)>();
        var parent = cameFrom.ContainsKey(current) ? cameFrom[current] : ((int x, int y)?)null;
        var neighbors = GetPrunedNeighbors(current.x, current.y, parent);

        foreach (var neighbor in neighbors)
        {
            (int dx, int dy) direction = (neighbor.x - current.x, neighbor.y - current.y);
            var jumpPoint = Jump(current.x, current.y, direction.dx, direction.dy, goal);
            if (jumpPoint.HasValue) successors.Add(jumpPoint.Value);
        }

        return successors;
    }

    /// <summary>
    /// Hakee karsitun naapurisolmujen listan
    /// </summary>
    private List<(int x, int y)> GetPrunedNeighbors(int x, int y, (int x, int y)? parent)
    {
        return parent == null
            ? GetInitialNeighbors(x, y)
            : GetDirectedNeighbors(x, y, parent.Value);
    }

    /// <summary>
    /// Hakee kaikki kelvolliset naapurisolmut aloitustilanteessa
    /// </summary>
    private List<(int x, int y)> GetInitialNeighbors(int x, int y)
    {
        return _directions
            .Where(d => !IsBlocked(x, y, d.dx, d.dy))
            .Select(d => (x + d.dx, y + d.dy))
            .ToList();
    }

    /// <summary>
    /// Hakee naapurisolmut aiemman solmun liikesuunnan perusteella
    /// </summary>
    private List<(int x, int y)> GetDirectedNeighbors(int x, int y, (int px, int py) parent)
    {
        var dx = Math.Sign(x - parent.px);
        var dy = Math.Sign(y - parent.py);

        return (dx, dy) switch
        {
            (0, _) => GetVerticalNeighbors(x, y, dy),
            (_, 0) => GetHorizontalNeighbors(x, y, dx),
            _ => GetDiagonalNeighbors(x, y, dx, dy)
        };
    }

    /// <summary>
    /// Hakee naapurisolmut pystysuunnassa
    /// </summary>
    private List<(int x, int y)> GetVerticalNeighbors(int x, int y, int dy)
    {
        var neighbors = new List<(int, int)>();
        if (!IsBlocked(x, y, 0, dy))
            neighbors.Add((x, y + dy));

        // Tarkistetaan pakotetut naapurit molemmilta puolilta
        if (IsBlocked(x, y, 1, 0) && !IsBlocked(x, y, 1, dy))
            neighbors.Add((x + 1, y + dy));
        if (IsBlocked(x, y, -1, 0) && !IsBlocked(x, y, -1, dy))
            neighbors.Add((x - 1, y + dy));

        return neighbors;
    }

    /// <summary>
    /// Hakee naapurisolmut vaakasuunnassa
    /// </summary>
    private List<(int x, int y)> GetHorizontalNeighbors(int x, int y, int dx)
    {
        var neighbors = new List<(int, int)>();
        if (!IsBlocked(x, y, dx, 0))
            neighbors.Add((x + dx, y));

        // Tarkistetaan pakotetut naapurisolmut ylä ja alapuolelta
        if (IsBlocked(x, y, 0, 1) && !IsBlocked(x, y, dx, 1))
            neighbors.Add((x + dx, y + 1));
        if (IsBlocked(x, y, 0, -1) && !IsBlocked(x, y, dx, -1))
            neighbors.Add((x + dx, y - 1));

        return neighbors;
    }

    /// <summary>
    /// Hakee naapurisolmut diagonaalisessa liikkeessä
    /// </summary>
    private List<(int x, int y)> GetDiagonalNeighbors(int x, int y, int dx, int dy)
    {
        var neighbors = new List<(int, int)>();
        if (!IsBlocked(x, y, 0, dy)) neighbors.Add((x, y + dy));
        if (!IsBlocked(x, y, dx, 0)) neighbors.Add((x + dx, y));
        if (!IsBlocked(x, y, dx, dy)) neighbors.Add((x + dx, y + dy));

        // Tarkistetaan pakotetut naapurisolmut kohtisuorissa suunnissa
        if (IsBlocked(x, y, -dx, 0) && !IsBlocked(x, y, 0, dy))
            neighbors.Add((x - dx, y + dy));
        if (IsBlocked(x, y, 0, -dy) && !IsBlocked(x, y, dx, 0))
            neighbors.Add((x + dx, y - dy));

        return neighbors;
    }

    /// <summary>
    /// Suorittaa jump point haun määritettyyn suuntaan rekursiivisesti
    /// </summary>
    private (int x, int y)? Jump(int x, int y, int dx, int dy, Node goal)
    {
        var (nx, ny) = (x + dx, y + dy);
        if (IsBlocked(x, y, dx, dy))
        {
            return null;
        }
        if (nx == goal.X && ny == goal.Y)
        {
            return (nx, ny);
        }

        return (dx, dy) switch
        {
            (0, _) => JumpVertical(nx, ny, dy, goal),
            (_, 0) => JumpHorizontal(nx, ny, dx, goal),
            _ => JumpDiagonal(nx, ny, dx, dy, goal)
        };
    }

    /// <summary>
    /// Käsittelee pystysuunnassa jump point hakua
    /// </summary>
    private (int x, int y)? JumpVertical(int x, int y, int dy, Node goal)
    {
        // Tarkistetaan pakotetut naapurit molemmilta puolilta
        if ((!IsBlocked(x, y, 1, dy) && IsBlocked(x, y, 1, 0)) ||
            (!IsBlocked(x, y, -1, dy) && IsBlocked(x, y, -1, 0)))
        {
            return (x, y);
        }
        return Jump(x, y, 0, dy, goal);
    }

    /// <summary>
    /// Käsittelee vaakasuunnassa jump point hakua
    /// </summary>
    private (int x, int y)? JumpHorizontal(int x, int y, int dx, Node goal)
    {
        // Tarkistetaan pakotetut naapurisolmut ylä ja alapuolelta
        if ((!IsBlocked(x, y, dx, 1) && IsBlocked(x, y, 0, 1)) ||
            (!IsBlocked(x, y, dx, -1) && IsBlocked(x, y, 0, -1)))
        {
            return (x, y);
        }
        return Jump(x, y, dx, 0, goal);
    }

    /// <summary>
    /// Käsittelee diagonaalisesti jump point hakua
    /// </summary>
    private (int x, int y)? JumpDiagonal(int x, int y, int dx, int dy, Node goal)
    {
        if ((!IsBlocked(x, y, -dx, dy) && IsBlocked(x, y, -dx, 0)) ||
            (!IsBlocked(x, y, dx, -dy) && IsBlocked(x, y, 0, -dy)))
        {
            return (x, y);
        }

        if (Jump(x, y, dx, 0, goal) != null || Jump(x, y, 0, dy, goal) != null)
            return (x, y);

        return Jump(x, y, dx, dy, goal);
    }

    /// <summary>
    /// Tarkistaa onko liike koordinaattien välillä estetty esteiden tai kartan reunojen vuoksi
    /// </summary>
    private bool IsBlocked(int x, int y, int dx, int dy)
    {
        var (nx, ny) = (x + dx, y + dy);
        if (nx < 0 || ny < 0 || nx >= _map.GetLength(0) || ny >= _map.GetLength(1))
            return true;

        // Tarkistetaan diagonaaliliike kulmien läpi
        if (dx != 0 && dy != 0)
        {
            if (_map[x + dx, y] == 1 && _map[x, y + dy] == 1 || _map[nx, ny] == 1)
            {
                return true;
            }

            _allVisited.Add(new Node(nx, ny));
        }

        if (_map[nx, ny] == 1)
        {
            return true;
        }

        _allVisited.Add(new Node(nx, ny));

        return false;
    }

    /// <summary>
    /// Luo reitin maalista lähtöön
    /// </summary>
    private List<Node> ReconstructPath(
   Dictionary<(int x, int y), (int x, int y)> cameFrom,
   Node start,
   (int x, int y) current
)
    {
        var cur = current;
        var s = (start.X, start.Y);
        var path = new List<Node>() { new Node(cur.x, cur.y) };

        while (cameFrom.ContainsKey(cur))
        {
            var parent = cameFrom[cur];

            // Lisätään hypyn pisteiden väliin jäävät pisteet
            var segment = GetLineBetween(parent.x, parent.y, cur.x, cur.y);
            path.AddRange(segment);

            path.Add(new Node(parent.x, parent.y));

            cur = parent;
        }

        path.Reverse();
        return path;
    }

    /// <summary>
    /// Palauttaa pisteet kahden pisteen välillä
    /// </summary>
    private static List<Node> GetLineBetween(int startX, int startY, int endX, int endY)
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

    /// <summary>
    /// Palauttaa etäisyyden pisteestä 1 pisteeseen 2 kun voidaan liikkua 8 suuntaan.
    /// </summary>
    private static double OctagonalDistance(int x1, int y1, int x2, int y2)
    {
        var dx = Math.Abs(x1 - x2);
        var dy = Math.Abs(y1 - y2);
        return Math.Max(dx, dy) + (Math.Sqrt(2) - 1) * Math.Min(dx, dy);
    }

    /// <summary>
    /// Palauttaa listan läpikäydyistä pisteistä
    /// </summary>
    private IEnumerable<Node> ExtractVisitedNodes(Dictionary<(int x, int y), double> gScore)
    {
        return gScore.Keys.Select(p => new Node(p.x, p.y));
    }
}