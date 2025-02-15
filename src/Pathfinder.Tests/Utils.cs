using Pathfinder.Pathfinding;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pathfinder.Tests
{
    public class Utils
    {
        /// <summary>
        /// Rakentaa joukon solmuja 2D taulukosta, jossa 1=vierailtu. 
        /// </summary>
        /// <param name="grid">2D taulukko</param>
        /// <returns>HashSet Node-olioita</returns>
        public static HashSet<Node> BuildSetFromGrid(int[,] grid)
        {
            var set = new HashSet<Node>();
            for (int x = 0; x < grid.GetLength(0); x++)
            {
                for (int y = 0; y < grid.GetLength(1); y++)
                {
                    if (grid[x, y] == 1)
                    {
                        set.Add(new Node(x, y));
                    }
                }
            }
            return set;
        }
    }
}
