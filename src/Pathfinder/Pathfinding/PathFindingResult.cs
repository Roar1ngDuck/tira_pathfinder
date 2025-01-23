using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pathfinder.Pathfinding
{
    public class PathFindingResult
    {
        public bool PathFound => Path != null;
        public ICollection<Node>? VisitedNodes { get; set; }
        public ICollection<Node>? Path { get; set; }

        public PathFindingResult(ICollection<Node>? visitedNodes, ICollection<Node>? path)
        {
            VisitedNodes = visitedNodes;
            Path = path;
        }
    }
}
