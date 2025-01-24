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

        private double CalculatePathLength()
        {
            if (Path == null)
            {
                return 0;
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
                else
                {
                    throw new InvalidOperationException("Invalid path: Nodes are not adjacent.");
                }
            }

            return totalLength;
        }
    }
}
