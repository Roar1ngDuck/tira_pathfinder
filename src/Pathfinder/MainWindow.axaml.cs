using System.Collections.Generic;
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
using Avalonia.Input;
using System.Linq;
using Pathfinder.Pathfinding.Algorithms;
using Avalonia.Controls.Primitives;
using System.Collections;

namespace Pathfinder;

public partial class MainWindow : Window
{
    private bool _shouldDrawVisualization = true;
    private WriteableBitmap _bitmap;
    private Point _startPosition;
    private Stopwatch _timingStopwatch;
    private int[,] _map;
    private StepDelay _stepDelay;

    public MainWindow()
    {
        InitializeComponent();
    }

    /// <summary>
    /// Event handler start napin klikkausta varten
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void StartButton_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        StartVisualization(VisualizationMode.SinglePath);
    }

    private void RandomPathsButton_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        StartVisualization(VisualizationMode.RandomPathBenchmark);
    }

    /// <summary>
    /// Event handler kuvan päällä hiiren pohjassapidon aloitusta varten.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void OnPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (e.GetCurrentPoint(VisualizationImage).Properties.IsLeftButtonPressed)
        {
            _startPosition = e.GetPosition(VisualizationImage);
            StartTextBox.Text = $"{(int)_startPosition.X},{(int)_startPosition.Y}";
        }
    }

    /// <summary>
    /// Event handler kuvan päällä hiiren liikuttamista varten. Piirtää viivan pohjassapidon alkupisteestä tämän hetkiseen hiiren sijaintiin.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void OnPointerMoved(object? sender, PointerEventArgs e)
    {
        if (e.GetCurrentPoint(VisualizationImage).Properties.IsLeftButtonPressed)
        {
            Point currentPosition = e.GetPosition(VisualizationImage);
            UpdateLine(_startPosition, currentPosition);
        }
    }

    /// <summary>
    /// Event handler kuvan päällä hiiren pohjassapidon lopetusta varten.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void OnPointerReleased(object? sender, PointerReleasedEventArgs e)
    {
        if (e.InitialPressMouseButton == MouseButton.Left)
        {
            var goalPosition = e.GetPosition(VisualizationImage);
            GoalTextBox.Text = $"{(int)goalPosition.X},{(int)goalPosition.Y}";
        }
    }

    private void Slider_ValueChanged(object? sender, RangeBaseValueChangedEventArgs e)
    {
        if (_stepDelay is null)
        {
            return;
        }

        var stepDelay = Math.Pow(StepDelaySlider.Value, 4);
        var stepDelayTimeSpan = TimeSpan.FromMicroseconds(stepDelay);
        _stepDelay.UpdateTargetStepDelay(stepDelayTimeSpan);
    }

    /// <summary>
    /// Piirtää viivan pisteiden välille
    /// </summary>
    /// <param name="start"></param>
    /// <param name="end"></param>
    private void UpdateLine(Point start, Point end)
    {
        DynamicLine.StartPoint = start;
        DynamicLine.EndPoint = end;
        DynamicLine.IsVisible = true;
    }

    /// <summary>
    /// Aloittaa reitinhaun visualisoinnin
    /// </summary>
    public void StartVisualization(VisualizationMode mode)
    {
        ClearPreviousRun();

        InitMap(MapTextBox.Text);

        var startNumbers = StartTextBox.Text.Split(',');
        var start = new Node(int.Parse(startNumbers[0]), int.Parse(startNumbers[1]));

        var goalNumbers = GoalTextBox.Text.Split(',');
        var goal = new Node(int.Parse(goalNumbers[0]), int.Parse(goalNumbers[1]));

        var algorithm = GetSelectedAlgorithm();

        var allowDiagonal = AllowDiagonalCheckBox.IsChecked.HasValue && AllowDiagonalCheckBox.IsChecked.Value;

        var stepDelay = Math.Pow(StepDelaySlider.Value, 4);

        switch (mode)
        {
            case VisualizationMode.SinglePath:
                RunPathfinding(start, goal, algorithm, allowDiagonal, stepDelay);
                break;
            case VisualizationMode.RandomPathBenchmark:
                RunRandomPathBenchmark(algorithm, allowDiagonal);
                break;
        }

        
    }

    /// <summary>
    /// Aloittaa reitinetsinnän kartassa aloituspisteestä maalipisteeseen ja piirtää tuloksen.
    /// </summary>
    /// <param name="start">Aloituspiste</param>
    /// <param name="goal">Maalipiste</param>
    /// <param name="algorithm">Käytettävä reitinhakualgoritmi</param>
    /// <param name="allowDiagonal">Sallitaanko vinottaiset siirtymiset</param>
    /// <param name="stepDelay">Haluttu keskimääräinen viive jokaisen pisteen käsittelylle mikrosekunteina</param>
    private async void RunPathfinding(Node start, Node goal, IPathFindingAlgorithm algorithm, bool allowDiagonal, double stepDelay)
    {
        PathFindingResult result;

        if (stepDelay > 0)
        {
            var stepDelayTimeSpan = TimeSpan.FromMicroseconds(stepDelay);
            _stepDelay = new StepDelay(stepDelayTimeSpan);
            _timingStopwatch = Stopwatch.StartNew();
            result = await Task.Run(() => algorithm.Search(start, goal, allowDiagonal, Callback, _stepDelay));
            _timingStopwatch.Stop();
        }
        else
        {
            _timingStopwatch = Stopwatch.StartNew();
            result = algorithm.Search(start, goal, allowDiagonal);
            _timingStopwatch.Stop();
        }

        if (!_shouldDrawVisualization)
        {
            await Task.Delay(100);
        }

        var visited = result.VisitedNodes;
        IEnumerable<Node> emptyList = new List<Node>();

        DrawPaths(ref visited, ref emptyList, new Node(0, 0), result.Path);

        NodesVisitedTextBox.Text = result.VisitedNodes.Count().ToString();
        PathLengthTextBox.Text = $"{result.PathLength} / {result.Path?.Count}";
        TimeTakenTextBox.Text = $"{_timingStopwatch.Elapsed.TotalMilliseconds} ms";
    }

    private async void RunRandomPathBenchmark(IPathFindingAlgorithm algorithm, bool allowDiagonal)
    {
        double totalTimeTaken = 0;
        var rnd = new Random();

        var randomPathCount = 100;
        DebugOutputTextBox.Text += $"Benchmarking with {randomPathCount} random paths with {algorithm.GetType().Name}\n";

        for (int i = 0; i < randomPathCount; i++)
        {
            PathFindingResult result;

            var start = new Node(rnd.Next(0, _map.GetLength(0)), rnd.Next(0, _map.GetLength(1)));
            var goal = new Node(rnd.Next(0, _map.GetLength(0)), rnd.Next(0, _map.GetLength(1)));

            DebugOutputTextBox.Text += $"Random path: ({start.X}, {start.Y}) -> ({goal.X}, {goal.Y})\n";

            _timingStopwatch = Stopwatch.StartNew();
            result = algorithm.Search(start, goal, allowDiagonal);
            _timingStopwatch.Stop();

            // Käyttöliittymälle jää aikaa piirtää
            //if (i % 5 == 0)
            {
                await Task.Delay(1);
            }

            var visited = result.VisitedNodes;
            IEnumerable<Node> emptyList = new List<Node>();

            DrawPaths(ref emptyList, ref emptyList, new Node(0, 0), result.Path);

            DebugOutputTextBox.Text += $"Result: Path found={result.PathFound}, Path length={result.Path?.Count}\n";

            //NodesVisitedTextBox.Text = result.VisitedNodes.Count().ToString();
            //PathLengthTextBox.Text = $"{result.PathLength} / {result.Path?.Count}";
            //TimeTakenTextBox.Text = $"{_timingStopwatch.Elapsed.TotalMilliseconds} ms";

            totalTimeTaken += _timingStopwatch.Elapsed.TotalMilliseconds;
        }

        double averageTime = Math.Round(totalTimeTaken / randomPathCount, 4);
        TimeTakenTextBox.Text = $"Average: {averageTime} ms";
    }

    /// <summary>
    /// Luo kartan annetusta tiedostosta ja piirtää sen.
    /// </summary>
    /// <param name="pathToMap">Polku tiedostoon. Suhteellinen ja absoluuttinen polku käy</param>
    private void InitMap(string pathToMap)
    {
        try
        {
            _map = Input.ReadMapFromImage(pathToMap);
        }
        catch
        {
            _map = Input.ReadMapFromFile(pathToMap);
        }

        _bitmap = new WriteableBitmap(new PixelSize(_map.GetLength(0), _map.GetLength(1)), new Vector(96, 96), PixelFormat.Rgb32);
        VisualizationImage.Source = _bitmap;

        DrawMap(_map);
    }

    /// <summary>
    /// Tyhjentää aiemman reitinhaun aikana piirretyn kuvan sekä statistiikat
    /// </summary>
    private void ClearPreviousRun()
    {
        _bitmap = new WriteableBitmap(new PixelSize(1, 1), new Vector(96, 96), PixelFormat.Rgb32);
        VisualizationImage.Source = _bitmap;

        DynamicLine.IsVisible = false;
        NodesVisitedTextBox.Text = "";
        PathLengthTextBox.Text = "";
        TimeTakenTextBox.Text = "";
    }

    /// <summary>
    /// Palauttaa valitun reitinhakualgoritmin ja heittää InvalidOperationException jos valittu reitti on virheellinen.
    /// </summary>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException"></exception>
    private IPathFindingAlgorithm GetSelectedAlgorithm()
    {
        IPathFindingAlgorithm algorithm = AlgorithmComboBox.SelectedIndex switch
        {
            0 => new Dijkstra(_map),
            1 => new AStar(_map),
            2 => new JumpPointSearch(_map),
            _ => throw new InvalidOperationException($"Unsupported algorithm: {AlgorithmComboBox.SelectedIndex}")
        };

        return algorithm;
    }

    /// <summary>
    /// Callback funktio kartan piirtämistä varten, joka kutsutaan reitinhakualgoritmissa.
    /// </summary>
    /// <param name="visited">Lista kaikista läpikäydyistä pisteistä</param>
    /// <param name="queue">Lista jonossa olevista pisteistä</param>
    /// <param name="current">Tällä hetkellä prosessoitavana oleva piste</param>
    private void Callback(IEnumerable<Node> visited, IEnumerable<Node> queue, Node current)
    {
        if (!_shouldDrawVisualization)
        {
            return;
        }

        _shouldDrawVisualization = false;

        Dispatcher.UIThread.InvokeAsync(() =>
        {
            DrawPaths(ref visited, ref queue, current, null);
            TimeTakenTextBox.Text = $"{_timingStopwatch.Elapsed.TotalMilliseconds} ms";
            _shouldDrawVisualization = true;
        });
    }

    /// <summary>
    /// Piirtää seinät WriteableBitmap kuvaan annetusta pikselikartasta
    /// </summary>
    /// <param name="map">Pikselikartta</param>
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
                        var brush = map[x, y] == 1 ? Brushes.Black : Brushes.White;
                        var color = brush.Color.ToUInt32();

                        buffer[y * stride + x] = color;
                    }
                }
            }
        }

        VisualizationImage.InvalidateVisual();
    }

    /// <summary>
    /// Piirtää kuljetut reitit WriteableBitmap kuvaan
    /// </summary>
    /// <param name="visited">Lista kaikista läpikäydyistä pisteistä</param>
    /// <param name="queue">Lista jonossa olevista pisteistä</param>
    /// <param name="current">Tällä hetkellä prosessoitavana oleva piste</param>
    /// <param name="path">Lyhin läydetty polku jos sitä on olemassa</param>
    private void DrawPaths(ref IEnumerable<Node> visited, ref IEnumerable<Node> queue, Node? current, List<Node>? path)
    {
        using (var frameBuffer = _bitmap.Lock())
        {
            unsafe
            {
                uint* buffer = (uint*)frameBuffer.Address.ToPointer();
                int stride = frameBuffer.RowBytes / sizeof(uint);

                foreach (var node in queue)
                {
                    buffer[node.Y * stride + node.X] = ToBgr(Brushes.DarkOrange.Color);
                }

                var visitedCount = 0;
                foreach (var node in visited)
                {
                    buffer[node.Y * stride + node.X] = ToBgr(Brushes.LightGreen.Color);
                    visitedCount++;
                }
                NodesVisitedTextBox.Text = visitedCount.ToString();

                if (path is not null)
                {
                    foreach (var node in path)
                    {
                        buffer[node.Y * stride + node.X] = ToBgr(Brushes.Blue.Color);
                    }
                }
                
                if (current is not null)
                {
                    buffer[current.Y * stride + current.X] = ToBgr(Brushes.DarkRed.Color);
                }
            }
        }

        VisualizationImage.InvalidateVisual();
    }

    /// <summary>
    /// Muuttaa värin BGR muotoon
    /// </summary>
    /// <param name="color">Väri joka muunnetaan</param>
    /// <returns>BGR väri uint muodossa</returns>
    uint ToBgr(Color color)
    {
        return (uint)((color.B << 16) | (color.G << 8) | color.R);
    }
}