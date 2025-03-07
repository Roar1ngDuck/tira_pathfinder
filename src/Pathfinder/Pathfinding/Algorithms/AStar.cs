using System;
using System.Collections.Generic;
using System.Linq;

namespace Pathfinder.Pathfinding.Algorithms;

/// <summary>
/// A* algoritmi, joka toimii pikselikartoilla.
/// </summary>
/// <param name="map">Pikselikartta, jossa 0 = kuljettava ruutu ja != 0 = este.</param>
public class AStar(int[,] map) : PathFindingAlgorithm
{
    private readonly int[,] _map = map;

    // Siirtymäsuunnat ja niiden kustannukset ilman vinottain liikkumista.
    private static readonly (int dx, int dy, double cost)[] _directionsStraight =
    {
        (-1, 0, 1), (1, 0, 1), (0, -1, 1), (0, 1, 1)
    };

    // Siirtymäsuunnat ja niiden kustannukset sisältäen vinottain liikkumisen.
    private static readonly (int dx, int dy, double cost)[] _directionsAll =
    {
        (-1, 0, 1), (1, 0, 1), (0, -1, 1), (0, 1, 1), // Suoraan
        (-1, -1, Math.Sqrt(2)), (-1, 1, Math.Sqrt(2)), // Vinottain
        (1, -1, Math.Sqrt(2)), (1, 1, Math.Sqrt(2))
    };

    /// <summary>
    /// Etsii lyhyimmän reitin kartassa lähtöpisteestä maalipisteeseen.
    /// </summary>
    /// <param name="start">Lähtöpiste.</param>
    /// <param name="goal">Maalipiste.</param>
    /// <param name="allowDiagonal">Sallitaanko vinottaiset siirrot.</param>
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
        var context = CreateSearchContext(start, goal, allowDiagonal, callbackFunc, stepDelay);

        return PerformAStarSearch(context);
    }

    /// <summary>
    /// Etsii lyhyimmän reitin kartassa lähtöpisteestä maalipisteeseen.
    /// </summary>
    /// <param name="start">Lähtöpiste.</param>
    /// <param name="goal">Maalipiste.</param>
    /// <param name="allowDiagonal">Sallitaanko vinottaiset siirrot.</param>
    /// <returns>PathFindingResult-olio, joka sisältää lopullisen reitin jos sellainen löytyy ja kaikki vieraillut solmut.</returns>
    public override PathFindingResult Search(Node start, Node goal, bool allowDiagonal)
    {
        return Search(start, goal, allowDiagonal, null, null);
    }

    /// <summary>
    /// Suorittaa varsinaisen A*-haun.
    /// </summary>
    /// <param name="context">SearchContext olio joka sisältää kartan ja muut haun tietorakenteet.</param>
    /// <returns>PathFindingResult-olio, joka sisältää lopullisen reitin jos sellainen löytyy ja kaikki vieraillut solmut.</returns>
    private PathFindingResult PerformAStarSearch(SearchContext context)
    {
        Utils.ArrayUtils.InitArrayToMaxValue(context.FScore);
        Utils.ArrayUtils.InitArrayToMaxValue(context.GScore);

        SetStartNodeScores(context);

        context.OpenSet.Enqueue(context.Start, context.FScore[context.Start.X, context.Start.Y]);

        Span<(Node neighbor, double cost)> neighborBuffer = new (Node, double)[8];

        while (context.OpenSet.TryDequeue(out var current, out _))
        {
            if (context.ClosedSet[current.X, current.Y])
            {
                continue;
            }
            context.ClosedSet[current.X, current.Y] = true;

            if (CallbackInterval.ShouldCallCallback())
            {
                context.CallbackFunc?.Invoke(
                    Utils.PathUtils.ExtractVisitedNodes(context.FScore, context.OpenSet).ToList(),
                    context.OpenSet.UnorderedItems.Select(item => item.Element).ToList(),
                    current);
            }

            if (current == context.Goal)
            {
                var path = Utils.PathUtils.ReconstructPath(context.CameFrom, current);
                return new PathFindingResult(Utils.PathUtils.ExtractVisitedNodes(context.FScore, context.OpenSet), path);
            }

            int neighborCount = GetNeighbors(_map, current, context.AllowDiagonal, neighborBuffer);
            ProcessNeighbors(context, neighborBuffer, neighborCount, current);

            // Mahdollinen viive jokaisen solmun käsittelyn jälkeen, jota käytetään visualisoinnissa.
            context.StepDelay?.Wait();
        }

        // Jos kaikki avoimet solmut on käsitelty ilman että maalia löytyi, palautetaan tulos ilman polkua.
        return new PathFindingResult(Utils.PathUtils.ExtractVisitedNodes(context.FScore, context.OpenSet), null);
    }

    /// <summary>
    /// Luo hakukontekstin ja tarvittavat tietorakenteet ja asettaa perusarvot
    /// </summary>
    /// <param name="start">Lähtösolmu.</param>
    /// <param name="goal">Maalisolmu.</param>
    /// <param name="allowDiagonal">Sallitaanko vinottaiset siirtymät.</param>
    /// <param name="callbackFunc">Mahdollinen callback-funktio.</param>
    /// <param name="stepDelay">Mahdollinen viive.</param>
    /// <returns>SearchContext olio, joka sisältää tiedot hakua varten.</returns>
    private SearchContext CreateSearchContext(
        Node start,
        Node goal,
        bool allowDiagonal,
        Action<IEnumerable<Node>, List<Node>, Node>? callbackFunc,
        StepDelay? stepDelay)
    {
        var width = _map.GetLength(0);
        var height = _map.GetLength(1);

        // Valitaan heuristiikkafunktio sen perusteella, sallitanko vinottaiset liikkeet.
        Func<Node, Node, double> heuristic = allowDiagonal ? Utils.DistanceUtils.EuclideanDistance : Utils.DistanceUtils.ManhattanDistance;

        return new SearchContext
        {
            Start = start,
            Goal = goal,
            AllowDiagonal = allowDiagonal,
            CallbackFunc = callbackFunc,
            StepDelay = stepDelay,
            CameFrom = new Node?[width, height],
            GScore = new double[width, height],
            FScore = new double[width, height],
            ClosedSet = new bool[width, height],
            Heuristic = heuristic,
            OpenSet = new PriorityQueue<Node, double>()
        };
    }

    /// <summary>
    /// Asettaa lähtösolmulle alkuarvot gScoreen ja fScoreen.
    /// </summary>
    /// <param name="context">Hakukonteksti, jossa taulukot sijaitsevat.</param>
    private void SetStartNodeScores(SearchContext context)
    {
        context.GScore[context.Start.X, context.Start.Y] = 0;
        context.FScore[context.Start.X, context.Start.Y] = context.Heuristic(context.Start, context.Goal);
    }

    /// <summary>
    /// Käsittelee annetun solmun kaikki naapurit ja päivittää niiden arvoja (gScore, fScore).
    /// </summary>
    /// <param name="context">Hakukonteksti josta tiedot ja rakenteet haetaan.</param>
    /// <param name="neighbors">Taulukko naapureista ja niiden kustannuksista. Kaikki 8 paikkaa ei välttämättä ole käytössä.</param>
    /// <param name="neighborCount">Kuinka monta naapuria taulukossa oikeasti on.</param>
    /// <param name="current">Tällä hetkellä käsiteltävä solmu, jonka naapurit prosessoidaan./param>
    private void ProcessNeighbors(
        SearchContext context,
        Span<(Node neighbor, double cost)> neighbors,
        int neighborCount,
        Node current)
    {
        for (int i = 0; i < neighborCount; i++)
        {
            var (neighbor, cost) = neighbors[i];
            double tentativeGScore = context.GScore[current.X, current.Y] + cost;

            // Jos uuden reitin kustannus on parempi, päivitetään taulukot ja lisätään jonoon.
            if (tentativeGScore < context.GScore[neighbor.X, neighbor.Y])
            {
                context.CameFrom[neighbor.X, neighbor.Y] = current;
                context.GScore[neighbor.X, neighbor.Y] = tentativeGScore;
                context.FScore[neighbor.X, neighbor.Y] = tentativeGScore +
                                                         context.Heuristic(neighbor, context.Goal);

                context.OpenSet.Enqueue(neighbor, context.FScore[neighbor.X, neighbor.Y]);
            }
        }
    }

    /// <summary>
    /// Etsii kaikki annettavan solmun validit naapurit kartassa.
    /// </summary>
    /// <param name="map">Pikselikartta, jossa arvo 0 tarkoittaa kuljettavissa olevaa ruutua.</param>
    /// <param name="node">Kohdesolmu, jonka naapurit halutaan etsiä.</param>
    /// <param name="allowDiagonal">Sallitaanko myös vinottaiset siirrot.</param>
    /// <param name="neighbors">Span, johon tallennetaan naapurit (Node, double cost). Oikean lukumäärän saa metodin palautusarvosta.</param>
    /// <returns>Naapureiden lukumäärä</returns>
    public static int GetNeighbors(
        int[,] map,
        Node node,
        bool allowDiagonal,
        Span<(Node, double)> neighbors)
    {
        var directions = allowDiagonal ? _directionsAll : _directionsStraight;
        int count = 0;

        foreach (var (dx, dy, cost) in directions)
        {
            if (Utils.MapUtils.IsBlocked(map, node.X, node.Y, dx, dy))
            {
                continue;
            }

            int nx = node.X + dx;
            int ny = node.Y + dy;

            neighbors[count++] = (new Node(nx, ny), cost);
        }

        return count;
    }

    /// <summary>
    /// Sisäinen luokka, joka sisältää kaikki A*-hakua varten tarvittavan tietorakenteet.
    /// </summary>
    private class SearchContext
    {
        public required Node Start { get; set; }
        public required Node Goal { get; set; }
        public required bool AllowDiagonal { get; set; }
        public Action<IEnumerable<Node>, List<Node>, Node>? CallbackFunc { get; set; }
        public StepDelay? StepDelay { get; set; }

        public required Node?[,] CameFrom { get; set; }
        public required double[,] GScore { get; set; }
        public required double[,] FScore { get; set; }
        public required bool[,] ClosedSet { get; set; }
        public required Func<Node, Node, double> Heuristic { get; set; }

        public required PriorityQueue<Node, double> OpenSet { get; set; }
    }
}
