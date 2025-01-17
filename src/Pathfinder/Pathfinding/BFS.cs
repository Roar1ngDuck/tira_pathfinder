using System;
using System.Collections.Generic;
using System.Threading;

namespace Pathfinder.Pathfinding;

public class BFS
{
    public static void Search(int[,] map, (int x, int y) start, (int x, int y) goal, Action<int[,], HashSet<(int x, int y)>, Queue<(int x, int y)>, (int x, int y)> callbackFunc)
    {
        var visited = new HashSet<(int x, int y)>();
        var queue = new Queue<(int x, int y)>();

        queue.Enqueue(start);

        while (queue.Count > 0)
        {
            var current = queue.Dequeue();
            visited.Add(current);

            callbackFunc(map, visited, queue, current);

            if (current == goal)
            {
                break;
            }

            var neighbors = Helpers.GetNeighbors(map, current);

            foreach (var neighbor in neighbors)
            {
                if (!visited.Contains(neighbor))
                {
                    queue.Enqueue(neighbor);
                }
            }

            Thread.Sleep(250);
        }
    }
}
