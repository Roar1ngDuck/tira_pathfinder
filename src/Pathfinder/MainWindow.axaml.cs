using System.Collections.Generic;
using System.Threading;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Threading;
using System.Threading.Tasks;
using Pathfinder.Pathfinding;
using System;
using Avalonia.Media.Imaging;
using Avalonia;
using Avalonia.Platform;

namespace Pathfinder;

public partial class MainWindow : Window
{
    private WriteableBitmap _bitmap;
    private bool _isUiUpdating = false;

    public MainWindow()
    {
        InitializeComponent();
    }

    private void Button_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        StartVisualization();
    }

    private async void StartVisualization()
    {
        var map = Input.ReadMapFromFile(MapTextBox.Text);

        _bitmap = new WriteableBitmap(new PixelSize(map.GetLength(0), map.GetLength(1)), new Vector(96, 96), PixelFormat.Rgba8888);
        VisualizationImage.Source = _bitmap;

        DrawEmptyMap(map);

        var startNumbers = StartTextBox.Text.Split(',');
        var start = new Node(int.Parse(startNumbers[0]), int.Parse(startNumbers[1]));

        var goalNumbers = GoalTextBox.Text.Split(',');
        var goal = new Node(int.Parse(goalNumbers[0]), int.Parse(goalNumbers[1]));

        var callbackFunc = (int[,] map, HashSet<Node> visited, Queue<Node> queue, Node current) => 
        {
            if (_isUiUpdating)
            {
                return;
            }
            _isUiUpdating = true;

            Dispatcher.UIThread.InvokeAsync(() =>
            {
                NodesVisitedTextBox.Text = visited.Count.ToString();
                DrawMap(map, visited, queue, current);
            });
        };

        await Task.Run(() => BFS.Search(map, start, goal, callbackFunc));
    }

    private void DrawEmptyMap(int[,] map)
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
                        var brush = map[x, y] == 1 ? Brushes.Black : Brushes.White;
                        var color = brush.Color.ToUInt32();

                        buffer[y * stride + x] = color;
                    }
                }
            }
        }

        VisualizationImage.InvalidateVisual();
    }

    private void DrawMap(int[,] map, HashSet<Node> visited, Queue<Node> queue, Node current)
    {
        using (var frameBuffer = _bitmap.Lock())
        {
            unsafe
            {
                uint* buffer = (uint*)frameBuffer.Address.ToPointer();
                int stride = frameBuffer.RowBytes / sizeof(uint);

                foreach (var node in visited)
                {
                    buffer[node.Y * stride + node.X] = Brushes.LightGreen.Color.ToUInt32();
                }
                foreach (var node in queue)
                {
                    buffer[node.Y * stride + node.X] = Brushes.LightBlue.Color.ToUInt32();
                }
                buffer[current.Y * stride + current.X] = Brushes.Red.Color.ToUInt32();
            }
        }

        VisualizationImage.InvalidateVisual();

        _isUiUpdating = false;
    }
}