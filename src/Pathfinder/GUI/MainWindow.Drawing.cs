using Avalonia;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using Pathfinder.Pathfinding;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Pathfinder
{
    /// <summary>
    /// Sisältää piirtämiseen ja kartan käsittelyyn liittyvät metodit.
    /// </summary>
    public partial class MainWindow
    {
        private WriteableBitmap _bitmap;
        private int[,] _map;

        /// <summary>
        /// Piirtää viivan pisteiden välille
        /// </summary>
        /// <param name="start">Alkupiste</param>
        /// <param name="end">Loppupiste</param>
        private void UpdateLine(Point start, Point end)
        {
            DynamicLine.StartPoint = start;
            DynamicLine.EndPoint = end;
            DynamicLine.IsVisible = true;
        }

        /// <summary>
        /// Alustaa kartan polusta ja piirtää sen ruudulle
        /// </summary>
        /// <param name="pathToMap">Karttatiedoston polku</param>
        private void InitMap(string pathToMap)
        {
            if (!Helpers.Input.TryReadMap(pathToMap, out _map))
            {
                return;
            }

            _bitmap = new WriteableBitmap(
                new PixelSize(_map.GetLength(0), _map.GetLength(1)),
                new Vector(96, 96),
                PixelFormat.Rgb32);

            _lastQueue.Clear();
            _lastVisited.Clear();
            _lastCurrent.Clear();

            VisualizationImage.Source = _bitmap;
            DrawMap(_map);
        }

        /// <summary>
        /// Tyhjentää edellisen reitinhaun ja nollaa kuvan
        /// </summary>
        private void ClearPreviousRun()
        {
            _bitmap = new WriteableBitmap(
                new PixelSize(1, 1),
                new Vector(96, 96),
                PixelFormat.Rgb32);

            VisualizationImage.Source = _bitmap;
            DynamicLine.IsVisible = false;

            NodesVisitedTextBox.Text = "";
            PathLengthTextBox.Text = "";
            TimeTakenTextBox.Text = "";
        }

        /// <summary>
        /// Piirtää annetun kartan WriteableBitmap-kuvaan
        /// </summary>
        /// <param name="map">Kaksulotteinen taulukko, jossa 1 = seinä, 0 = vapaa</param>
        private void DrawMap(int[,] map)
        {
            using (var frameBuffer = _bitmap.Lock())
            {
                unsafe
                {
                    uint* buffer = (uint*)frameBuffer.Address.ToPointer();
                    int stride = frameBuffer.RowBytes / sizeof(uint);

                    for (int y = 0; y < map.GetLength(1); y++)
                    {
                        for (int x = 0; x < map.GetLength(0); x++)
                        {
                            var mapValue = map[x, y];
                            var brush = (mapValue == 1) ? Brushes.Black : Brushes.White;
                            buffer[y * stride + x] = brush.Color.ToUInt32();
                        }
                    }
                }
            }
            VisualizationImage.InvalidateVisual();
        }

        HashSet<Node> _lastQueue = new();
        HashSet<Node> _lastVisited = new();
        HashSet<Node> _lastCurrent = new();

        /// <summary>
        /// Piirtää polkuun liittyvät pisteet (visited, queue, path) bitmap-kuvaan
        /// </summary>
        /// <param name="visited">Läpikäydyt solmut</param>
        /// <param name="queue">Jonossa olevat solmut</param>
        /// <param name="current">Käsittelyssä oleva solmu</param>
        /// <param name="path">Löydetty polku</param>
        private void DrawPaths(ref IEnumerable<Node> visited,
                               ref IEnumerable<Node> queue,
                               Node? current,
                               List<Node>? path)
        {
            using (var frameBuffer = _bitmap.Lock())
            {
                unsafe
                {
                    uint* buffer = (uint*)frameBuffer.Address.ToPointer();
                    int stride = frameBuffer.RowBytes / sizeof(uint);

                    // Piirretään jonossa olevat oranssilla
                    foreach (var node in queue)
                    {
                        if (!_lastCurrent.Contains(node) && _lastQueue.Contains(node))
                        {
                            continue;
                        }
                        _lastQueue.Add(node);

                        buffer[node.Y * stride + node.X] = ToBgr(Brushes.DarkOrange.Color);
                    }

                    // Piirretään läpikäydyt vaaleanvihreällä
                    var visitedCount = 0;
                    foreach (var node in visited)
                    {
                        visitedCount++;

                        if (!_lastCurrent.Contains(node) && _lastVisited.Contains(node))
                        {
                            continue;
                        }
                        _lastVisited.Add(node);

                        buffer[node.Y * stride + node.X] = ToBgr(Brushes.LightGreen.Color);
                    }
                    NodesVisitedTextBox.Text = visitedCount.ToString();

                    // Piirretään polku (jos löytynyt)
                    if (path is not null)
                    {
                        foreach (var node in path)
                        {
                            buffer[node.Y * stride + node.X] = ToBgr(Brushes.Blue.Color);
                        }
                    }

                    // Piirretään nykyinen solmu tummanpunaisella
                    if (current is not null)
                    {
                        _lastCurrent.Add((Node)current);

                        buffer[current.Value.Y * stride + current.Value.X] = ToBgr(Brushes.DarkRed.Color);
                    }
                }
            }

            VisualizationImage.InvalidateVisual();
        }

        /// <summary>
        /// Muuntaa annetun Avalonia Color arvon BGR-muotoon uint
        /// </summary>
        /// <param name="color">Väri joka muunnetaan</param>
        /// <returns>BGR väri uint muodossa</returns>
        private uint ToBgr(Color color)
        {
            return (uint)((color.B << 16) | (color.G << 8) | color.R);
        }
    }
}
