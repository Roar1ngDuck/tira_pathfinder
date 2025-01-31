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
        int width = grid.GetLength(0);
        int height = grid.GetLength(1);
        _grid = new Node[width, height];
        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                _grid[i, j] = new Node(i, j);
                _grid[i, j].Obstacle = grid[i, j] == 1;
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
        var visitedParentsDict = new Dictionary<(int, int), (int, int)?>();
        var result = FindPath(
            _grid,
            start,
            goal,
            out var visitedParents,
            callbackFunc,
            stepDelay);

        for (int x = 0; x < visitedParents.GetLength(0); x++)
        {
            for (int y = 0; y < visitedParents.GetLength(1); y++)
            {
                visitedParentsDict[(x, y)] = visitedParents[x, y];
            }
        }

        List<Node>? path = null;
        if (result.Item1 is not null)
        {
            path = new List<Node>();
            var currentPos = result.Item1.Position;
            while (true)
            {
                path.Add(_grid[currentPos.Item1, currentPos.Item2]);
                if (!visitedParentsDict[currentPos].HasValue)
                    break;
                currentPos = visitedParentsDict[currentPos].Value;
            }
            path.Reverse();
        }

        return new PathFindingResult(result.Item2, path);
    }

    public PathFindingResult Search(Node start, Node goal, bool allowDiagonal)
    {
        return Search(start, goal, allowDiagonal, null, TimeSpan.Zero);
    }

    private (Node foundGoal, IEnumerable<Node> visited, List<Node> queueNodes) FindPath(
        Node[,] grid,
        Node start,
        Node goal,
        out (int, int)?[,] visitedParents,
        Action<IEnumerable<Node>, List<Node>, Node>? callbackFunc = null,
        TimeSpan stepDelay = default)
    {
        int width = grid.GetLength(0);
        int height = grid.GetLength(1);
        visitedParents = new (int, int)?[width, height];
        var queue = new PriorityQueue<Node, double>();
        var openSet = new HashSet<Node>();
        var closedSet = new HashSet<Node>();
        var allNodesEvaluated = new HashSet<Node>();

        start.Cost = 0;
        start.H = OctagonalDistance(start, goal);
        queue.Enqueue(start, start.Cost.Value + start.H.Value);
        openSet.Add(start);
        visitedParents[start.X, start.Y] = null;

        while (queue.Count > 0)
        {
            var current = queue.Dequeue();
            openSet.Remove(current);
            closedSet.Add(current);
            allNodesEvaluated.Add(current);

            callbackFunc?.Invoke(
                closedSet.Union(allNodesEvaluated).ToList(),
                queue.UnorderedItems.Select(i => i.Element).ToList(),
                current);

            if (stepDelay > TimeSpan.Zero)
                Thread.Sleep(stepDelay);

            if (current.Position == goal.Position)
            {
                return (current, closedSet.Union(allNodesEvaluated),
                        queue.UnorderedItems.Select(i => i.Element).ToList());
            }

            var pruned = Prune(grid, current);
            foreach (var dir in pruned.Neighbors)
            {
                var (jp, jumpCost) = Jump(grid, current, dir, current.Cost.Value, goal, ref allNodesEvaluated);
                if (jp is null) continue;

                if (closedSet.Contains(jp)) continue;

                if (!jp.Cost.HasValue || jumpCost < jp.Cost.Value)
                {
                    jp.Cost = jumpCost;
                    visitedParents[jp.X, jp.Y] = current.Position;

                    if (!jp.H.HasValue)
                        jp.H = OctagonalDistance(jp, goal);

                    double priority = jp.Cost.Value + jp.H.Value;
                    if (!openSet.Contains(jp))
                    {
                        openSet.Add(jp);
                    }
                    queue.Enqueue(jp, priority);
                }
            }
        }

        return (null, closedSet.Union(allNodesEvaluated),
                queue.UnorderedItems.Select(i => i.Element).ToList());
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
        Node goal,
        ref HashSet<Node> allNodesEvaluated)
    {
        var (next, cost) = Step(grid, node, direction, costSoFar);
        if (next is null) return (null, 0);

        allNodesEvaluated.Add(node);

        if (next.Position == goal.Position)
            return (next, cost);

        var pruned = Prune(grid, next, direction);
        if (pruned.Forced)
            return (next, cost);

        if (direction % 2 == 1)
        {
            foreach (var sideDir in new[] { ChangeDirection(direction, 1), ChangeDirection(direction, -1) })
            {
                var (jp, forcedCost) = Jump(grid, next, sideDir, cost, goal, ref allNodesEvaluated);
                if (jp is not null)
                {
                    if (jp.Position == goal.Position)
                        return (jp, forcedCost);
                    return (next, cost);
                }
            }
        }

        return Jump(grid, next, direction, cost, goal, ref allNodesEvaluated);
    }

    private static (Node, double) Step(Node[,] grid, Node node, int direction, double costSoFar)
    {
        var (dx, dy) = DirectionOffsets[direction];
        int newX = node.X + dx;
        int newY = node.Y + dy;

        if (newX < 0 || newY < 0 || newX >= grid.GetLength(0) || newY >= grid.GetLength(1))
            return (null, 0);

        bool diagonal = dx != 0 && dy != 0;
        if (diagonal)
        {
            Node side1 = grid[node.X + dx, node.Y];
            Node side2 = grid[node.X, node.Y + dy];
            if (side1.Obstacle && side2.Obstacle)
                return (null, 0);
        }

        Node next = grid[newX, newY];
        if (next.Obstacle)
            return (null, 0);

        double stepCost = diagonal ? _costDiagonal : _costHorizontalVertical;
        return (next, costSoFar + stepCost);
    }

    private static int ChangeDirection(int direction, int amount) => (direction + amount + 8) % 8;

    private static (HashSet<int> Neighbors, bool Forced) Prune(Node[,] grid, Node node, int? direction = null)
    {
        var neighbors = new HashSet<int>();
        bool forced = false;

        for (int dir = 0; dir < 8; dir++)
        {
            var (dx, dy) = DirectionOffsets[dir];
            int newX = node.X + dx;
            int newY = node.Y + dy;

            if (newX >= 0 && newY >= 0 && newX < grid.GetLength(0) && newY < grid.GetLength(1))
            {
                if (!grid[newX, newY].Obstacle)
                    neighbors.Add(dir);
            }
        }

        if (direction.HasValue)
        {
            int dir = direction.Value;
            bool isDiagonal = dir % 2 == 1;
            int offset = isDiagonal ? 1 : 0;

            int parentDir = ChangeDirection(dir, 4);
            neighbors.Remove(parentDir);

            for (int i = -offset; i <= offset; i++)
            {
                int currentDir = ChangeDirection(dir, i);
                if (neighbors.Contains(currentDir))
                {
                    var (dx, dy) = DirectionOffsets[currentDir];
                    int x = node.X + dx;
                    int y = node.Y + dy;
                    if (grid[x, y].Obstacle)
                        neighbors.Remove(currentDir);
                }
            }

            CheckForcedNeighbors(dir, 2 + offset, node, grid, neighbors, ref forced);
            CheckForcedNeighbors(dir, -2 - offset, node, grid, neighbors, ref forced);

            neighbors.Remove(ChangeDirection(dir, 5));
            neighbors.Remove(ChangeDirection(dir, 3));
        }

        return (neighbors, forced);
    }

    private static void CheckForcedNeighbors(
        int baseDir,
        int change,
        Node node,
        Node[,] grid,
        HashSet<int> directions,
        ref bool forced)
    {
        int mainDir = ChangeDirection(baseDir, change);
        int checkDir = ChangeDirection(mainDir, change > 0 ? -1 : 1);

        if (directions.Contains(checkDir))
        {
            var (dx, dy) = DirectionOffsets[checkDir];
            int x = node.X + dx;
            int y = node.Y + dy;
            bool checkIsObstacle = grid[x, y].Obstacle;

            bool mainDirIsValid = directions.Contains(mainDir);
            bool mainIsObstacle = false;
            if (mainDirIsValid)
            {
                (dx, dy) = DirectionOffsets[mainDir];
                x = node.X + dx;
                y = node.Y + dy;
                mainIsObstacle = grid[x, y].Obstacle;
            }

            if (checkIsObstacle || (mainDirIsValid && !mainIsObstacle))
            {
                directions.Remove(checkDir);
            }
            else
            {
                forced = true;
            }
        }

        directions.Remove(mainDir);
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