using Pathfinder.Pathfinding;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

public class JumpPointSearch : IPathFindingAlgorithm
{
    private readonly Node[,] _grid;

    private static readonly double _costHorizontalVertical = 1;
    private static readonly double _costDiagonal = Math.Sqrt(2);

    public JumpPointSearch(int[,] grid)
    {
        var width = grid.GetLength(0);
        var height = grid.GetLength(1);
        _grid = new Node[width, height];
        for (int i = 0; i < width; i++)
        {
            for(int j = 0; j < height; j++)
            {
                _grid[i, j] = new Node(i, j);
                if (grid[i, j] == 1)
                {
                    _grid[i, j].Obstacle = true;
                }
            }
        }
    }

    public PathFindingResult Search(
        Node start,
        Node goal,
        bool allowDiagonal,
        Action<IEnumerable<Node>, List<Node>, Node>? callbackFunc,
        TimeSpan stepDelay)
    {
        var visitedDict = new Dictionary<(int, int), (int, int)?>();
        var result = FindPath(
            _grid,
            start,
            goal,
            out visitedDict,
            callbackFunc,
            stepDelay);

        List<Node>? path = null;
        if (result.Item1 is not null)
        {
            path = new List<Node>();
            var currentPos = result.Item1.Position;
            while (true)
            {
                path.Add(_grid[currentPos.Item1, currentPos.Item2]);
                if (!visitedDict[currentPos].HasValue) break;
                currentPos = visitedDict[currentPos].Value;
            }
            path.Reverse();
        }

        return new PathFindingResult(result.Item2, path);
    }

    public PathFindingResult Search(Node start, Node goal, bool allowDiagonal)
    {
        return Search(start, goal, allowDiagonal, null, TimeSpan.Zero);
    }

    private (Node, HashSet<Node>, List<Node>) FindPath(
        Node[,] grid,
        Node start,
        Node goal,
        out Dictionary<(int, int), (int, int)?> visited,
        Action<IEnumerable<Node>, List<Node>, Node>? callbackFunc = null,
        TimeSpan stepDelay = default)
    {
        visited = new Dictionary<(int, int), (int, int)?>();
        var queue = new PriorityQueue<Node, double>();
        var explored = new HashSet<Node>();

        start.Cost = 0;
        start.H = OctagonalDistance(start, goal);
        queue.Enqueue(start, start.Cost.Value + start.H.Value);
        visited[start.Position] = null;

        while (queue.Count > 0)
        {
            var current = queue.Dequeue();
            explored.Add(current);

            callbackFunc?.Invoke(explored, queue.UnorderedItems.Select(i => i.Element).ToList(), current);
            if (stepDelay > TimeSpan.Zero)
            {
                Thread.Sleep(stepDelay);
            }

            if (current.Position == goal.Position)
                return (current, explored, queue.UnorderedItems.Select(i => i.Element).ToList());

            var pruned = Prune(grid, current);
            foreach (var (dir, neighbor) in pruned.Neighbors)
            {
                var (jp, jumpCost) = Jump(grid, current, dir, current.Cost ?? 0, goal);
                if (jp is null || explored.Contains(jp))
                    continue;

                var newCost = jumpCost;
                if (!jp.Cost.HasValue || newCost < jp.Cost)
                {
                    jp.Cost = newCost;
                    jp.Direction = dir;
                }

                if (!visited.ContainsKey(jp.Position))
                {
                    jp.H = jp.H ?? OctagonalDistance(jp, goal);
                    visited[jp.Position] = current.Position;
                    queue.Enqueue(jp, jp.Cost.Value + jp.H.Value);
                }
            }
        }

        return (null, null, null);
    }

    private static double OctagonalDistance(Node a, Node b)
    {
        double dx = Math.Abs(a.X - b.X);
        double dy = Math.Abs(a.Y - b.Y);
        return Math.Max(dx, dy) + (Math.Sqrt(2) - 1) * Math.Min(dx, dy);
    }

    private static (Node, double) Jump(
        Node[,] grid,
        Node node,
        int direction,
        double costSoFar,
        Node goal)
    {
        var (next, cost) = Step(grid, node, direction, costSoFar);
        if (next is null)
            return (null, 0);

        if (next.Position == goal.Position)
            return (next, cost);

        if (Prune(grid, next, direction).Forced)
            return (next, cost);

        // Diagonaalisissa siirtymisissä katsotaan sivut läpi
        if (direction % 2 == 1)
        {
            foreach (var sideDir in new[] { ChangeDirection(direction, 1), ChangeDirection(direction, -1) })
            {
                var (jp, forcedCost) = Jump(grid, next, sideDir, cost, goal);
                if (jp is null)
                {
                    continue;
                }

                if (jp.Position == goal.Position)
                {
                    return (jp, forcedCost);
                }

                // Löydettiin forced neighbor => 'next' on jump point
                return (next, cost);
            }
        }

        // Jatketaan samaan suuntaan
        return Jump(grid, next, direction, cost, goal);
    }

    private static (Node, double) Step(Node[,] grid, Node node, int direction, double costSoFar)
    {
        var (x, y) = node.Position;
        var (dx, dy) = DirectionOffsets[direction];
        int newX = x + dx, newY = y + dy;

        if (newX >= 0 && newY >= 0 && newX < grid.GetLength(0) && newY < grid.GetLength(1))
        {
            var next = grid[newX, newY];
            if (!next.Obstacle)
            {
                var stepCost = (dx == 0 || dy == 0) ? _costHorizontalVertical : _costDiagonal;
                return (next, costSoFar + stepCost);
            }
        }
        return (null, 0);
    }

    private static int ChangeDirection(int direction, int amount) => (direction + amount + 8) % 8;

    private static (Dictionary<int, Node> Neighbors, bool Forced) Prune(Node[,] grid, Node node, int? direction = null)
    {
        var neighbors = new Dictionary<int, Node>();
        bool forced = false;
        direction ??= node.Direction;

        foreach (var dir in Enumerable.Range(0, 8))
        {
            var (dx, dy) = DirectionOffsets[dir];
            int newX = node.Position.Item1 + dx;
            int newY = node.Position.Item2 + dy;

            if (newX >= 0 && newY >= 0 && newX < grid.GetLength(0) && newY < grid.GetLength(1))
                neighbors[dir] = grid[newX, newY];
        }

        if (direction.HasValue)
        {
            int dir = direction.Value;
            bool isDiagonal = dir % 2 == 1;
            int offset = isDiagonal ? 1 : 0;

            // Poistetaan suunta mistä tultiin
            int parentDir = ChangeDirection(dir, 4);
            neighbors.Remove(parentDir);

            // Poistetaan suunnat joihin ei pääse
            for (int i = -offset; i <= offset; i++)
            {
                int currentDir = ChangeDirection(dir, i);
                if (neighbors.TryGetValue(currentDir, out var v) && v.Obstacle)
                    neighbors.Remove(currentDir);
            }

            // Etsitään forced neighbors eteen oikealle ja vasemmalle
            CheckForcedNeighbors(dir, 2 + offset, ref neighbors, ref forced);
            CheckForcedNeighbors(dir, -2 - offset, ref neighbors, ref forced);

            // Poistetaan takana olevat suunnat
            neighbors.Remove(ChangeDirection(dir, 5));
            neighbors.Remove(ChangeDirection(dir, 3));
        }

        return (neighbors, forced);
    }

    private static void CheckForcedNeighbors(int baseDir, int change, ref Dictionary<int, Node> neighbors, ref bool forced)
    {
        int mainDir = ChangeDirection(baseDir, change);
        int checkDir = ChangeDirection(mainDir, change > 0 ? -1 : 1);

        bool hasObstacle = neighbors.TryGetValue(checkDir, out var v1) && v1.Obstacle;
        bool noObstacle = !neighbors.TryGetValue(mainDir, out var v2) || !v2.Obstacle;

        if (hasObstacle || noObstacle)
            neighbors.Remove(checkDir);
        else
            forced = true;

        neighbors.Remove(mainDir);
    }

    private static readonly (int, int)[] DirectionOffsets =
    {
        (0, 1),   // 0 = ↑
        (1, 1),   // 1 = ↗
        (1, 0),   // 2 = →
        (1, -1),  // 3 = ↘
        (0, -1),  // 4 = ↓
        (-1, -1), // 5 = ↙
        (-1, 0),  // 6 = ←
        (-1, 1)   // 7 = ↖
    };
}