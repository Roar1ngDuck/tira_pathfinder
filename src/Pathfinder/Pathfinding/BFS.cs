using System;
using System.Collections.Generic;
using System.Threading;

namespace Pathfinder.Pathfinding;

public class BFS
{
    public static void Search(int[,] map, Node start, Node goal, Action<int[,], HashSet<Node>, Queue<Node>, Node> callbackFunc)
    {
        var visited = new HashSet<Node>();
        var queue = new Queue<Node>();

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
                if (!visited.Contains(neighbor) && !queue.Contains(neighbor))
                {
                    queue.Enqueue(neighbor);
                }
            }

            Thread.Sleep(10);
        }
    }
}
