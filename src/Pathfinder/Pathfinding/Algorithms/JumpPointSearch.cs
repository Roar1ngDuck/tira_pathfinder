﻿using System;
using System.Collections.Generic;
using System.Linq;

namespace Pathfinder.Pathfinding.Algorithms;

/// <summary>
/// Jump Point Search -algoritmi, joka toimii pikselikartoilla.
/// </summary>
/// <param name="map">Pikselikartta, jossa 0 = kuljettava ruutu ja != 0 = este.</param>
public class JumpPointSearch(int[,] map) : PathFindingAlgorithm
{
    private readonly int[,] _map = map;

    private static readonly (int dx, int dy)[] _directions =
    {
        (-1, 0), (1, 0), (0, -1), (0, 1),
        (-1, -1), (-1, 1), (1, -1), (1, 1)
    };

    /// <summary>
    /// Etsii lyhyimmän reitin kartassa lähtöpisteestä maalipisteeseen.
    /// </summary>
    /// <param name="start">Lähtöpiste.</param>
    /// <param name="goal">Maalipiste.</param>
    /// <param name="allowDiagonal">JPS olettaa, että vinottaiset liikkeet ovat sallittuja, joten allowDiagonal-parametriä ei käytetä.</param>
    /// <param name="callbackFunc">Kutsutaan ennen jokaisen pisteen käsittelyä. Palauttaa tiedon läpi käydyistä solmuista ja jonossa olevista solmuista, sekä parhaillaan käsiteltävän solmun.
    /// <param name="stepDelay">Jos asetettu, viivästyttää suorittamista halutulla viiveellä jokaisen solmun käsittelyn jälkeen.
    /// <returns>PathFindingResult-olio, joka sisältää lopullisen reitin jos sellainen löytyy ja kaikki vieraillut solmut.</returns>
    public override PathFindingResult Search(
        Node start,
        Node goal,
        bool allowDiagonal,
        Action<IEnumerable<Node>, List<Node>, Node>? callbackFunc,
        StepDelay? stepDelay)
    {
        var context = CreateJpsContext(start, goal, callbackFunc, stepDelay);

        return PerformJumpPointSearch(context);
    }

    /// <summary>
    /// Etsii lyhyimmän reitin kartassa lähtöpisteestä maalipisteeseen.
    /// </summary>
    /// <param name="start">Lähtöpiste.</param>
    /// <param name="goal">Maalipiste.</param>
    /// <param name="allowDiagonal">JPS olettaa, että vinottaiset liikkeet ovat sallittuja, joten allowDiagonal-parametriä ei käytetä.</param>
    /// <returns>PathFindingResult-olio, joka sisältää lopullisen reitin jos sellainen löytyy ja kaikki vieraillut solmut.</returns>
    public override PathFindingResult Search(Node start, Node goal, bool allowDiagonal)
    {
        return Search(start, goal, allowDiagonal, null, null);
    }

    /// <summary>
    /// Luo ja alustaa hakukontekstin.
    /// </summary>
    /// <param name="start">Lähtösolmu.</param>
    /// <param name="goal">Maalisolmu.</param>
    /// <param name="callbackFunc">Mahdollinen callback-funktio.</param>
    /// <param name="stepDelay">Mahdollinen käsittelyviive.</param>
    /// <returns>JpsContext-olio, joka sisältää kaikki tiedot ja rakenteet JPS-hakua varten.</returns>
    private JpsContext CreateJpsContext(
        Node start,
        Node goal,
        Action<IEnumerable<Node>, List<Node>, Node>? callbackFunc,
        StepDelay? stepDelay)
    {
        var ctx = new JpsContext
        {
            Map = _map,
            TrackAllVisitedNodes = stepDelay is not null,
            CallbackFunc = callbackFunc,
            StepDelay = stepDelay,
            Start = (start.X, start.Y),
            Goal = (goal.X, goal.Y),
            CameFrom = new Dictionary<(int x, int y), (int x, int y)>(),
            ClosedSet = new HashSet<(int x, int y)>(),
            GScore = new Dictionary<(int x, int y), double>(),
            FScore = new Dictionary<(int x, int y), double>(),
            OpenSet = new PriorityQueue<(int x, int y), double>()
        };

        ctx.GScore[ctx.Start] = 0;
        ctx.FScore[ctx.Start] = Utils.DistanceUtils.EuclideanDistance(ctx.Start.x, ctx.Start.y, ctx.Goal.x, ctx.Goal.y);
        ctx.OpenSet.Enqueue(ctx.Start, ctx.FScore[ctx.Start]);

        return ctx;
    }

    /// <summary>
    /// Suorittaa varsinaisen Jump Point Search haun annetun hakukontekstin avulla.
    /// </summary>
    /// <param name="ctx">JpsContext olio, joka sisältää kaikki tietorakenteet hakua varten.</param>
    /// <returns>PathFindingResult-olio, joka sisältää lopullisen reitin jos sellainen löytyy ja kaikki vieraillut solmut (IEnumerable&lt;Node&gt;).
    /// </returns>
    private PathFindingResult PerformJumpPointSearch(JpsContext ctx)
    {
        while (ctx.OpenSet.Count > 0)
        {
            var current = ctx.OpenSet.Dequeue();
            ctx.ClosedSet.Add(current);

            // Visualisointia varten kutsutaan callback-funktiota, jos asetettu.
            if (CallbackInterval.ShouldCallCallback())
            {
                ctx.CallbackFunc?.Invoke(
                    Utils.PathUtils.ExtractVisitedNodes(ctx.GScore),
                    ctx.AllVisitedNodes.ToList(),
                    new Node(current.x, current.y));
            }

            var pathResult = CheckForGoalReached(current, ctx);
            if (pathResult is not null)
            {
                return pathResult;
            }

            ProcessNodeNeighbors(current, ctx);

            // Mahdollinen viive jokaisen solmun käsittelyn jälkeen, jota käytetään visualisoinnissa.
            ctx.StepDelay?.Wait();
        }

        // Jos maalia ei löytynyt, palautetaan tulos ilman polkua.
        return new PathFindingResult(Utils.PathUtils.ExtractVisitedNodes(ctx.GScore), null);
    }

    /// <summary>
    /// Tarkistaa, onko current-koordinaatti sama kuin maalikoordinaatti. Jos on, luo polun ja palauttaa PathFindingResultin.
    /// </summary>
    /// <param name="current">Nykyinen piste (x, y).</param>
    /// <param name="ctx">Hakukonteksti, joka sisältää tarvittavat tietorakenteet.</param>
    /// <returns>PathFindingResult, jos maali on saavutettu, muuten null.</returns>
    private PathFindingResult? CheckForGoalReached((int x, int y) current, JpsContext ctx)
    {
        if (current == ctx.Goal)
        {
            var path = Utils.PathUtils.ReconstructPath(ctx.CameFrom, ctx.Start, current);
            return new PathFindingResult(
                Utils.PathUtils.ExtractVisitedNodes(ctx.GScore),
                path
            );
        }
        return null;
    }

    /// <summary>
    /// Prosessoi nykyisen solmun naapurit ja päivittää gScore- ja fScore-arvoja.
    /// </summary>
    /// <param name="current">Tällä hetkellä käsiteltävä solmu.</param>
    /// <param name="ctx">Hakukonteksti, joka sisältää tarvittavat tietorakenteet.</param>
    private void ProcessNodeNeighbors((int x, int y) current, JpsContext ctx)
    {
        // Haetaan JPS:n mukaiset seuraajat.
        var successors = GetValidSuccessors(current, ctx);
        foreach (var successor in successors)
        {
            if (ctx.ClosedSet.Contains(successor))
            {
                continue;
            }

            UpdateNodeScore(current, successor, ctx);
        }
    }

    /// <summary>
    /// Päivittää solmun pisteet (gScore, fScore) ja sijainnin openSetissä annettulle successor-solmulle.
    /// </summary>
    /// <param name="current">Nykyinen solmu.</param>
    /// <param name="successor">Seuraaja, jolle pisteet päivitetään.</param>
    /// <param name="ctx">Hakukonteksti, jolta tiedot haetaan.</param>
    private void UpdateNodeScore((int x, int y) current, (int x, int y) successor, JpsContext ctx)
    {
        var tentativeG = ctx.GScore[current] + Utils.DistanceUtils.EuclideanDistance(current.x, current.y, successor.x, successor.y);

        if (!ctx.GScore.ContainsKey(successor) || tentativeG < ctx.GScore[successor])
        {
            ctx.CameFrom[successor] = current;
            ctx.GScore[successor] = tentativeG;
            ctx.FScore[successor] = tentativeG + Utils.DistanceUtils.EuclideanDistance(successor.x, successor.y, ctx.Goal.x, ctx.Goal.y);

            ctx.OpenSet.Enqueue(successor, ctx.FScore[successor]);
        }
    }

    /// <summary>
    /// Palauttaa listan solmuista, joihin nykyisestä solmusta voidaan hypätä.
    /// </summary>
    /// <param name="current">Nykyinen solmu (x, y).</param>
    /// <param name="ctx">Hakukonteksti, jolta tiedot haetaan.</param>
    /// <returns>Lista koordinaatteja (x, y), joihin hypätä.</returns>
    private List<(int x, int y)> GetValidSuccessors((int x, int y) current, JpsContext ctx)
    {
        var successors = new List<(int, int)>();
        var parent = ctx.CameFrom.ContainsKey(current) ? ctx.CameFrom[current] : ((int x, int y)?)null;

        // Haetaan naapurit edellisen solmun suunnan perusteella, tai kaikki suunnat, jos aiempaa ei ole.
        var neighbors = parent == null ? GetInitialNeighbors(current.x, current.y, ctx) : GetDirectedNeighbors(current.x, current.y, parent.Value, ctx);

        // Suoritetaan jump-toiminto jokaiselle naapurille.
        foreach (var neighbor in neighbors)
        {
            var dx = neighbor.x - current.x;
            var dy = neighbor.y - current.y;
            var jumpPoint = Jump(current.x, current.y, dx, dy, ctx.Goal, ctx);
            if (!jumpPoint.HasValue)
            {
                continue;
            }

            successors.Add((jumpPoint.Value.x, jumpPoint.Value.y));
        }

        if (ctx.TrackAllVisitedNodes)
        {
            ctx.AllVisitedNodes.AddRange(successors.Select(n => new Node(n.Item1, n.Item2)));
        }

        return successors;
    }

    /// <summary>
    /// Palauttaa kaikki validit naapurit aloitustilanteessa, kun edeltäjää ei ole (start-solmu).
    /// </summary>
    /// <param name="x">Nykyinen x-koordinaatti.</param>
    /// <param name="y">Nykyinen y-koordinaatti.</param>
    /// <returns>Lista naapureita (x, y).</returns>
    private List<(int x, int y)> GetInitialNeighbors(int x, int y, JpsContext ctx)
    {
        var neighbors = _directions
            .Where(d => !Utils.MapUtils.IsBlocked(_map, x, y, d.dx, d.dy))
            .Select(d => (x + d.dx, y + d.dy))
            .ToList();

        if (ctx.TrackAllVisitedNodes)
        {
            ctx.AllVisitedNodes.AddRange(neighbors.Select(n => new Node(n.Item1, n.Item2)));
        }

        return neighbors;
    }

    /// <summary>
    /// Palauttaa karsitun naapurilistan edeltäjän perusteella.
    /// </summary>
    /// <param name="x">Nykyinen x-koordinaatti.</param>
    /// <param name="y">Nykyinen y-koordinaatti.</param>
    /// <param name="parent">Edeltäjän x- ja y-koordinaatit.</param>
    /// <returns>Lista naapureita (x, y).</returns>
    private List<(int x, int y)> GetDirectedNeighbors(int x, int y, (int px, int py) parent, JpsContext ctx)
    {
        var dx = Math.Sign(x - parent.px);
        var dy = Math.Sign(y - parent.py);

        // Ohjaa liikkeet sen mukaan, missä suunnassa edeltävä solmu sijaitsee.
        return (dx, dy) switch
        {
            (0, _) => GetVerticalNeighbors(x, y, dy, ctx),
            (_, 0) => GetHorizontalNeighbors(x, y, dx, ctx),
            _ => GetDiagonalNeighbors(x, y, dx, dy, ctx)
        };
    }

    /// <summary>
    /// Palauttaa pystyliikkeeseen validit naapurit.
    /// </summary>
    private List<(int x, int y)> GetVerticalNeighbors(int x, int y, int dy, JpsContext ctx)
    {
        var neighbors = new List<(int, int)>();
        // Suoraan ylös/alas, jos ei estettä
        if (!Utils.MapUtils.IsBlocked(_map, x, y, 0, dy))
        {
            neighbors.Add((x, y + dy));
        }

        // Pakotetut naapurit sivuilta
        if (Utils.MapUtils.IsBlocked(_map, x, y, 1, 0) && !Utils.MapUtils.IsBlocked(_map, x, y, 1, dy))
        {
            neighbors.Add((x + 1, y + dy));
        }
        if (Utils.MapUtils.IsBlocked(_map, x, y, -1, 0) && !Utils.MapUtils.IsBlocked(_map, x, y, -1, dy))
        {
            neighbors.Add((x - 1, y + dy));
        }

        return neighbors;
    }

    /// <summary>
    /// Palauttaa vaakaliikkeeseen validit naapurit.
    /// </summary>
    private List<(int x, int y)> GetHorizontalNeighbors(int x, int y, int dx, JpsContext ctx)
    {
        var neighbors = new List<(int, int)>();
        // Suoraan vasemmalle/oikealle, jos ei estettä
        if (!Utils.MapUtils.IsBlocked(_map, x, y, dx, 0))
        {
            neighbors.Add((x + dx, y));
        }

        // Pakotetut naapurit ylä- ja alapuolelta
        if (Utils.MapUtils.IsBlocked(_map, x, y, 0, 1) && !Utils.MapUtils.IsBlocked(_map, x, y, dx, 1))
        {
            neighbors.Add((x + dx, y + 1));
        }
        if (Utils.MapUtils.IsBlocked(_map, x, y, 0, -1) && !Utils.MapUtils.IsBlocked(_map, x, y, dx, -1))
        {
            neighbors.Add((x + dx, y - 1));
        }

        return neighbors;
    }

    /// <summary>
    /// Palauttaa diagonaaliliikkeeseen validit naapurit.
    /// </summary>
    private List<(int x, int y)> GetDiagonalNeighbors(int x, int y, int dx, int dy, JpsContext ctx)
    {
        var neighbors = new List<(int, int)>();
        // Suoraan diagonaali, jos ei estettä
        if (!Utils.MapUtils.IsBlocked(_map, x, y, dx, dy))
        {
            neighbors.Add((x + dx, y + dy));
        }

        // Samassa diagonaalissa ylös/alas ja vasen/oikea
        if (!Utils.MapUtils.IsBlocked(_map, x, y, 0, dy))
        {
            neighbors.Add((x, y + dy));
        }
        if (!Utils.MapUtils.IsBlocked(_map, x, y, dx, 0))
        {
            neighbors.Add((x + dx, y));
        }

        // Pakotetut naapurit
        if (Utils.MapUtils.IsBlocked(_map, x, y, -dx, 0) && !Utils.MapUtils.IsBlocked(_map, x, y, 0, dy))
        {
            neighbors.Add((x - dx, y + dy));
        }
        if (Utils.MapUtils.IsBlocked(_map, x, y, 0, -dy) && !Utils.MapUtils.IsBlocked(_map, x, y, dx, 0))
        {
            neighbors.Add((x + dx, y - dy));
        }

        return neighbors;
    }

    /// <summary>
    /// Suorittaa varsinaisen jump point haun.
    /// </summary>
    /// <param name="x">Nykyinen x-koordinaatti.</param>
    /// <param name="y">Nykyinen y-koordinaatti.</param>
    /// <param name="dx">Liikesuunta x-akselilla.</param>
    /// <param name="dy">Liikesuunta y-akselilla.</param>
    /// <param name="goal">Maalin sijainti.</param>
    /// <param name="ctx">Hakukonteksti, josta data saadaan.</param>
    /// <returns>Koordinaatti (x, y), jos hyppypiste löytyi, tai null.</returns>
    private (int x, int y)? Jump(int x, int y, int dx, int dy, (int x, int y) goal, JpsContext ctx)
    {
        var nx = x + dx;
        var ny = y + dy;

        if (Utils.MapUtils.IsBlocked(_map, x, y, dx, dy))
        {
            return null;
        }

        // Visualisointia varten tallennetaan kaikki käydyt solmut. Tämä on hidas operaatio, joten käytetään vain visualisointiin.
        if (ctx.TrackAllVisitedNodes)
        {
            ctx.AllVisitedNodes.Add(new Node(nx, ny));
        }

        // Visualisointia varten kutsutaan callback-funktiota, jos asetettu.
        if (CallbackInterval.ShouldCallCallback())
        {
            ctx.CallbackFunc?.Invoke(
                Utils.PathUtils.ExtractVisitedNodes(ctx.GScore),
                ctx.AllVisitedNodes.ToList(),
                new Node(x, y));
        }

        ctx.StepDelay?.Wait();

        if (nx == goal.x && ny == goal.y)
        {
            return (nx, ny);
        }

        // Jatketaan sen mukaan, missä suunnassa liikutaan (vaaka/pysty/diagonaali).
        if (dx == 0)  // Pysty
        {
            if (CheckForcedNeighborVertical(nx, ny, dy))
            {
                return (nx, ny);
            }

            return Jump(nx, ny, dx, dy, goal, ctx);
        }
        else if (dy == 0)  // Vaaka
        {
            if (CheckForcedNeighborHorizontal(nx, ny, dx))
            {
                return (nx, ny);
            }

            return Jump(nx, ny, dx, dy, goal, ctx);
        }
        else // Diagonaali
        {
            if (CheckForcedNeighborDiagonal(nx, ny, dx, dy))
            {
                return (nx, ny);
            }

            var hJump = Jump(nx, ny, dx, 0, goal, ctx);
            if (hJump != null)
            {
                return (nx, ny);
            }

            var vJump = Jump(nx, ny, 0, dy, goal, ctx);
            if (vJump != null)
            {
                return (nx, ny);
            }

            return Jump(nx, ny, dx, dy, goal, ctx);
        }
    }

    /// <summary>
    /// Tarkistaa pakotetut naapurit pystysuunnassa.
    /// </summary>
    private bool CheckForcedNeighborVertical(int x, int y, int dy)
    {
        // Jos oikealla on este, mutta oikea-ylä/ala ei ole, tai vastaavasti vasemmalla on este mutta vasen-ylä/ala ei ole.
        if ((!Utils.MapUtils.IsBlocked(_map, x, y, 1, dy) && Utils.MapUtils.IsBlocked(_map, x, y, 1, 0)) ||
            (!Utils.MapUtils.IsBlocked(_map, x, y, -1, dy) && Utils.MapUtils.IsBlocked(_map, x, y, -1, 0)))
        {
            return true;
        }
        return false;
    }

    /// <summary>
    /// Tarkistaa pakotetut naapurit vaakasuunnassa.
    /// </summary>
    private bool CheckForcedNeighborHorizontal(int x, int y, int dx)
    {
        // Jos yläpuolella on este, mutta ylä-oikea/vasen ei ole, tai vastaavasti alapuolella on este mutta ala-oikea/vasen ei ole.
        if ((!Utils.MapUtils.IsBlocked(_map, x, y, dx, 1) && Utils.MapUtils.IsBlocked(_map, x, y, 0, 1)) ||
            (!Utils.MapUtils.IsBlocked(_map, x, y, dx, -1) && Utils.MapUtils.IsBlocked(_map, x, y, 0, -1)))
        {
            return true;
        }
        return false;
    }

    /// <summary>
    /// Tarkistaa pakotetut naapurit diagonaalisesti.
    /// </summary>
    private bool CheckForcedNeighborDiagonal(int x, int y, int dx, int dy)
    {
        // Jos vasemmalla/takana on este, mutta vinottainen piste ei ole estynyt.
        if ((!Utils.MapUtils.IsBlocked(_map, x, y, -dx, dy) && Utils.MapUtils.IsBlocked(_map, x, y, -dx, 0)) ||
            (!Utils.MapUtils.IsBlocked(_map, x, y, dx, -dy) && Utils.MapUtils.IsBlocked(_map, x, y, 0, -dy)))
        {
            return true;
        }
        return false;
    }

    /// <summary>
    /// Sisäinen luokka, joka sisältää Jump Point Search -hakua varten tarvittavat tietorakenteet.
    /// </summary>
    private class JpsContext
    {
        public required int[,] Map { get; set; }

        public bool TrackAllVisitedNodes { get; set; }
        public List<Node> AllVisitedNodes { get; set; } = new();

        public Action<IEnumerable<Node>, List<Node>, Node>? CallbackFunc { get; set; }
        public StepDelay? StepDelay { get; set; }

        public required  (int x, int y) Start { get; set; }
        public required  (int x, int y) Goal { get; set; }

        public required Dictionary<(int x, int y), (int x, int y)> CameFrom { get; set; }
        public required HashSet<(int x, int y)> ClosedSet { get; set; }
        public required Dictionary<(int x, int y), double> GScore { get; set; }
        public required Dictionary<(int x, int y), double> FScore { get; set; }
        public required PriorityQueue<(int x, int y), double> OpenSet { get; set; }
    }
}
