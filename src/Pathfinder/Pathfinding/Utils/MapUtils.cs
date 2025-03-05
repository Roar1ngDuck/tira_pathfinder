using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pathfinder.Pathfinding.Utils
{
    public class MapUtils
    {
        /// <summary>
        /// Tarkistaa onko liike koordinaattien välillä estetty (esim. este tai kartan reuna).
        /// </summary>
        /// <param name="map">Kartta, jossa 0 = kuljettava ruutu ja != 0 = este.</param>
        /// <param name="x">Nykyinen x-koordinaatti.</param>
        /// <param name="y">Nykyinen y-koordinaatti.</param>
        /// <param name="dx">Liikesuunnan x-komponentti.</param>
        /// <param name="dy">Liikesuunnan y-komponentti.</param>
        /// <returns>True, jos liike estetty. False, jos vapaa.</returns>
        public static bool IsBlocked(int[,] map, int x, int y, int dx, int dy)
        {
            var (nx, ny) = (x + dx, y + dy);
            // Reunatarkistus
            if (nx < 0 || ny < 0 || nx >= map.GetLength(0) || ny >= map.GetLength(1))
            {
                return true;
            }

            // Tarkistetaan, onko suoraan seuraava ruutu estelty
            if (map[nx, ny] == 1)
            {
                return true;
            }

            // Diagonaaliliikkeessä lisäksi kulmien tarkistus
            if (dx != 0 && dy != 0)
            {
                // Molemmissa kulmissa este tai suoraan vinottainen ruutu estää
                if (map[nx, y] == 1 && map[x, ny] == 1)
                {
                    return true;
                }
            }

            return false;
        }
    }
}
