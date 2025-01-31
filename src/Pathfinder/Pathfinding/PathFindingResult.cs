using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pathfinder.Pathfinding
{
    public class PathFindingResult
    {
        /// <summary>
        /// Onko lyhin polku olemassa
        /// </summary>
        public bool PathFound => Path != null;
        /// <summary>
        /// Lista kaikista algoritmin läpikäymistä pisteistä
        /// </summary>
        public IEnumerable<Node>? VisitedNodes { get; set; }
        /// <summary>
        /// Lyhin polku jos se on olemassa
        /// </summary>
        public List<Node>? Path { get; set; }
        public double PathLength => CalculatePathLength();

        public PathFindingResult(IEnumerable<Node>? visitedNodes, List<Node>? path)
        {
            VisitedNodes = visitedNodes;
            Path = path;
        }

        public PathFindingResult(IEnumerable<(int x, int y)> visitedNodes, List<(int x, int y)>? path)
        {
            //VisitedNodes = visitedNodes;
            //Path = path;

            var visitedNodesList = new List<Node>();
            foreach (var visited in visitedNodes)
            {
                visitedNodesList.Add(new Node(visited.x, visited.y));
            }
            VisitedNodes = visitedNodesList;
            
            var pathList = new List<Node>();
            foreach (var point in path)
            {
                pathList.Add(new Node(point.x, point.y));
            }
            Path = pathList;
        }

        private double CalculatePathLength()
        {
            if (Path is null)
            {
                return 0;
            }

            if (Path.Last().Cost is not null)
            {
                return (double)Path.Last().Cost;
            }

            double totalLength = 0;

            for (int i = 1; i < Path.Count; i++)
            {
                var prev = Path[i - 1];
                var current = Path[i];

                int dx = Math.Abs(current.X - prev.X);
                int dy = Math.Abs(current.Y - prev.Y);

                if (dx == 1 && dy == 1)
                {
                    totalLength += Math.Sqrt(2);
                }
                else if (dx == 1 || dy == 1)
                {
                    totalLength += 1;
                }
            }

            return totalLength;
        }
    }
}
