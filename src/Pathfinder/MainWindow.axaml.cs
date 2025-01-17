using System.Collections.Generic;
using System.Threading;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Threading;
using System.Threading.Tasks;
using Pathfinder.Pathfinding;
using System;

namespace Pathfinder;

public partial class MainWindow : Window
{
    private const int CellSize = 20;

    public MainWindow()
    {
        InitializeComponent();
        StartVisualization();
    }

    private async void StartVisualization()
    {
        var map = new int[,] 
        {
            { 0, 0, 1, 0, 0, 0 },
            { 0, 0, 1, 0, 0, 0 },
            { 0, 0, 1, 0, 0, 0 },
            { 0, 0, 1, 0, 1, 0 },
            { 0, 0, 0, 0, 1, 0 },
            { 0, 0, 1, 0, 1, 0 },
            { 0, 0, 1, 0, 0, 0 },
        };
        
        var start = (3, 0);
        var goal = (3, 5);

        var callbackFunc = (int[,] map, HashSet<(int x, int y)> visited, Queue<(int x, int y)> queue, (int x, int y) current) => 
        {
            Dispatcher.UIThread.InvokeAsync(() =>
            {
                DrawMap(map, visited, queue, current);
            });
        };

        await Task.Run(() => BFS.Search(map, start, goal, callbackFunc));
    }
    private void DrawMap(int[,] map, HashSet<(int x, int y)> visited, Queue<(int x, int y)> queue, (int x, int y) current)
    {
        VisualizationCanvas.Children.Clear();

        for (int y = 0; y < map.GetLength(1); y++)
        {
            for (int x = 0; x < map.GetLength(0); x++)
            {
                var color = Brushes.White;
                if (current == (x, y))
                {
                    color = Brushes.Red;
                }
                else if (visited.Contains((x, y)))
                {
                    color = Brushes.LightGreen;
                }
                else if (queue.Contains((x, y)))
                {
                    color = Brushes.LightBlue;
                }
                else if (map[x, y] == 1)
                {
                    color = Brushes.Black;
                }

                var rect = new Avalonia.Controls.Shapes.Rectangle
                {
                    Width = CellSize,
                    Height = CellSize,
                    Fill = color,
                    Stroke = Brushes.Black,
                    StrokeThickness = 0.5
                };
                Canvas.SetLeft(rect, x * CellSize);
                Canvas.SetTop(rect, y * CellSize);
                VisualizationCanvas.Children.Add(rect);
            }
        }
    }
}