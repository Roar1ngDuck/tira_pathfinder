using Avalonia;
using Avalonia.Controls;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using Pathfinder.Pathfinding;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Pathfinder;

public partial class MainWindow : Window
{
    private StepDelay _stepDelay { get; set; }
    private Stopwatch _timingStopwatch { get; set; }
    private bool _shouldDrawVisualization { get; set; }
    private Stopwatch _drawStopwatch { get; set; }
    private WriteableBitmap _bitmap { get; set; }
    private int[,] _map;
    HashSet<Node> _lastQueue { get; set; }
    HashSet<Node> _lastVisited { get; set; }
    HashSet<Node> _lastCurrent { get; set; }
    private Point _startPosition { get; set; }

    public MainWindow()
    {
        InitializeComponent();

        _stepDelay = new StepDelay(TimeSpan.Zero);
        _timingStopwatch = new();
        _shouldDrawVisualization = true;
        _drawStopwatch = new();
        _map = new int[1, 1];
        _bitmap = new WriteableBitmap(
            new PixelSize(_map.GetLength(0), _map.GetLength(1)),
            new Vector(96, 96),
            PixelFormat.Rgb32);
        _lastQueue = new();
        _lastVisited = new();
        _lastCurrent = new();
    }
}
