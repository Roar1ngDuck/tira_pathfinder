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
using System.Collections;
using System.Security.Cryptography;
using System.ComponentModel;
using Avalonia.Input;

namespace Pathfinder;

public partial class MainWindow : Window
{
    private WriteableBitmap _bitmap;
    private Point _startPosition;
    public static bool ShouldCallCallback = true;

    public MainWindow()
    {
        InitializeComponent();
    }

    private void Button_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        StartVisualization();
    }

    private void OnPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (e.GetCurrentPoint(VisualizationImage).Properties.IsLeftButtonPressed)
        {
            _startPosition = e.GetPosition(VisualizationImage);
            StartTextBox.Text = $"{(int)_startPosition.X},{(int)_startPosition.Y}";
        }
    }

    private void OnPointerMoved(object? sender, PointerEventArgs e)
    {
        if (e.GetCurrentPoint(VisualizationImage).Properties.IsLeftButtonPressed)
        {
            Point currentPosition = e.GetPosition(VisualizationImage);
            UpdateLine(_startPosition, currentPosition);
        }
    }

    private void OnPointerReleased(object? sender, PointerReleasedEventArgs e)
    {
        if (e.InitialPressMouseButton == MouseButton.Left)
        {
            var goalPosition = e.GetPosition(VisualizationImage);
            GoalTextBox.Text = $"{(int)goalPosition.X},{(int)goalPosition.Y}";
        }
    }

    private void UpdateLine(Point start, Point end)
    {
        DynamicLine.StartPoint = start;
        DynamicLine.EndPoint = end;
        DynamicLine.IsVisible = true;
    }

    public async void StartVisualization()
    {
        DynamicLine.IsVisible = false;
        NodesVisitedTextBox.Text = "";
        PathLengthTextBox.Text = "";
        TimeTakenTextBox.Text = "";

        var map = Input.ReadMapFromFile(MapTextBox.Text);
        //var map = Input.ReadMapFromImage(MapTextBox.Text);

        _bitmap = new WriteableBitmap(new PixelSize(map.GetLength(0), map.GetLength(1)), new Vector(96, 96), PixelFormat.Rgba8888);
        VisualizationImage.Source = _bitmap;
        
        DrawEmptyMap(map);

        var startNumbers = StartTextBox.Text.Split(',');
        var start = new Node(int.Parse(startNumbers[0]), int.Parse(startNumbers[1]));

        var goalNumbers = GoalTextBox.Text.Split(',');
        var goal = new Node(int.Parse(goalNumbers[0]), int.Parse(goalNumbers[1]));

        var allowDiagonal = AllowDiagonalCheckBox.IsChecked.HasValue && AllowDiagonalCheckBox.IsChecked.Value;

        IPathFindingAlgorithm algorithm = AlgorithmComboBox.SelectedIndex switch
        {
            0 => new Dijkstra(map),
            1 => new AStar(map),
            _ => throw new NotImplementedException()
        };

        var sw = new Stopwatch();

        var stepDelay = StepDelaySlider.Value;

        PathFindingResult result;

        if (stepDelay > 0)
        {
            var stepDelayTimeSpan = TimeSpan.FromMicroseconds(Math.Pow(stepDelay, 4));
            sw.Start();
            result = await Task.Run(() => algorithm.Search(start, goal, allowDiagonal, Callback, stepDelayTimeSpan));
            sw.Stop();
        }
        else
        {
            sw.Start();
            result = algorithm.Search(start, goal, allowDiagonal);
            sw.Stop();
        }

        var visited = result.VisitedNodes;
        ICollection<Node> emptyList = new List<Node>();

        DrawMap(map, ref visited, ref emptyList, new Node(0, 0), result.Path);

        NodesVisitedTextBox.Text = result.VisitedNodes.Count.ToString();
        PathLengthTextBox.Text = result.Path.Count.ToString();
        TimeTakenTextBox.Text = $"{sw.Elapsed.TotalMilliseconds} ms";
    }

    private void Callback(int[,] map, ICollection<Node> visited, ICollection<Node> queue, Node current)
    {
        ShouldCallCallback = false;

        Dispatcher.UIThread.InvokeAsync(() =>
        {
            DrawMap(map, ref visited, ref queue, current, null);
            ShouldCallCallback = true;
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

    private void DrawMap(int[,] map, ref ICollection<Node> visited, ref ICollection<Node> queue, Node? current, ICollection<Node>? path)
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
                
                if (current != null)
                {
                    buffer[current.Value.Y * stride + current.Value.X] = Brushes.Red.Color.ToUInt32();
                }
            }
        }

        VisualizationImage.InvalidateVisual();
    }
}