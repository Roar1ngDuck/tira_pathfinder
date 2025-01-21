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
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace Pathfinder;

public partial class MainWindow : Window
{
    private WriteableBitmap _bitmap;

    public MainWindow()
    {
        InitializeComponent();

        WarmupAlgorithms();
    }

    private async Task WarmupAlgorithms()
    {
        StatusTextBox.Text = "Warming up algorithms";

        var algorithms = new IPathFindingAlgorithm[] { new BFS(), new AStar() };

        await Task.Run(() => 
        {
            var rnd = new Random();

            for (int i = 0; i < 10; i++)
            {
                var map = new int[100, 100];
                for (int y = 0; y < map.GetLength(1); y++)
                {
                    for (int x = 0; x < map.GetLength(0); x++)
                    {
                        map[x, y] = rnd.Next(0, 2);
                    }
                }

                var start = new Node(rnd.Next(0, map.GetLength(0)), rnd.Next(0, map.GetLength(1)));
                var goal = new Node(rnd.Next(0, map.GetLength(0)), rnd.Next(0, map.GetLength(1)));

                foreach (var algorithm in algorithms)
                {
                    var path = algorithm.Search(map, start, goal, null, 0, TimeSpan.Zero);
                    Console.WriteLine(path.Count);
                }

                Thread.Sleep(100);
            }
        });

        StatusTextBox.Text = "Ready";
    }

    private void Button_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        StartVisualization();
    }

    public async void StartVisualization()
    {
        var map = Input.ReadMapFromFile(MapTextBox.Text);

        _bitmap = new WriteableBitmap(new PixelSize(map.GetLength(0), map.GetLength(1)), new Vector(96, 96), PixelFormat.Rgba8888);
        VisualizationImage.Source = _bitmap;
        
        DrawEmptyMap(map);

        var startNumbers = StartTextBox.Text.Split(',');
        var start = new Node(int.Parse(startNumbers[0]), int.Parse(startNumbers[1]));

        var goalNumbers = GoalTextBox.Text.Split(',');
        var goal = new Node(int.Parse(goalNumbers[0]), int.Parse(goalNumbers[1]));

        IPathFindingAlgorithm algorithm = AlgorithmComboBox.SelectedIndex switch
        {
            0 => new BFS(),
            1 => new AStar(),
            _ => throw new NotImplementedException()
        };

        var sw = new Stopwatch();

        if (VisualizeRadioButton.IsChecked.HasValue && VisualizeRadioButton.IsChecked.Value)
        {
            sw.Start();
            await Task.Run(() => algorithm.Search(map, start, goal, Callback, (int)Math.Pow(2, 6), TimeSpan.FromMicroseconds(1000)));
            sw.Stop();
        }
        else
        {
            sw.Start();
            var path = algorithm.Search(map, start, goal, null, 0, TimeSpan.Zero);
            sw.Stop();

            DrawMap(map, new List<Node>(), new List<Node>(), new Node(0, 0), path);
        }

        TimeTakenTextBox.Text = $"{sw.Elapsed.TotalMilliseconds} ms";
    }

    private void Callback(int[,] map, ICollection<Node> visited, ICollection<Node> queue, Node current, ICollection<Node>? path)
    {
        Dispatcher.UIThread.InvokeAsync(() =>
        {
            NodesVisitedTextBox.Text = visited.Count.ToString();
            DrawMap(map, visited, queue, current, path);
        });
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

    private void DrawMap(int[,] map, ICollection<Node> visited, ICollection<Node> queue, Node current, ICollection<Node>? path)
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
                if (path != null)
                {
                    foreach (var node in path)
                    {
                        buffer[node.Y * stride + node.X] = Brushes.Red.Color.ToUInt32();
                    }
                }
                
                buffer[current.Y * stride + current.X] = Brushes.Red.Color.ToUInt32();
            }
        }

        VisualizationImage.InvalidateVisual();
    }
}