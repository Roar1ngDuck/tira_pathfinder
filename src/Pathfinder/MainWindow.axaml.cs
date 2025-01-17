using System.Collections.Generic;
using System.Threading;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Threading;
using System.Threading.Tasks;

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

        await Task.Run(() => BFS(map, start, goal));
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

    private void BFS(int[,] map, (int x, int y) start, (int x, int y) goal)
    {
        var visited = new HashSet<(int x, int y)>();
        var queue = new Queue<(int x, int y)>();

        queue.Enqueue(start);

        while (queue.Count > 0)
        {
            var current = queue.Dequeue();
            visited.Add(current);

            Dispatcher.UIThread.InvokeAsync(() =>
            {
                DrawMap(map, visited, queue, current);
            });

            if (current == goal)
            {
                break;
            }

            var neighbors = GetNeighbors(map, current);

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

    private List<(int x, int y)> GetNeighbors(int[,] map, (int x, int y) current)
    {
        var neighbors = new List<(int x, int y)>();

        var dx = new int[] { 1, -1, 0, 0 };
        var dy = new int[] { 0, 0, 1, -1 };

        for (int i = 0; i < 4; i++)
        {
            var x = current.x + dx[i];
            var y = current.y + dy[i];

            if (x >= 0 && x < map.GetLength(0) && y >= 0 && y < map.GetLength(1) && map[x, y] == 0)
            {
                neighbors.Add((x, y));
            }
        }

        return neighbors;
    }
}