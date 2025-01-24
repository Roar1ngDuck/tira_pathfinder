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

namespace Pathfinder;

public partial class MainWindow : Window
{
    public static bool ShouldCallCallback = true;

    private WriteableBitmap _bitmap;
    private Point _startPosition;
    private Stopwatch _timingStopwatch;
    private int[,] _map;

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
        StartVisualization();
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
    public void StartVisualization()
    {
        //for (int i = 0; i < 800; i++)
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

            RunPathfinding(start, goal, algorithm, allowDiagonal, stepDelay);
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
            _timingStopwatch = Stopwatch.StartNew();
            result = await Task.Run(() => algorithm.Search(start, goal, allowDiagonal, Callback, stepDelayTimeSpan));
            _timingStopwatch.Stop();
        }
        else
        {
            _timingStopwatch = Stopwatch.StartNew();
            result = algorithm.Search(start, goal, allowDiagonal);
            _timingStopwatch.Stop();
        }

        var visited = result.VisitedNodes;
        var emptyList = new List<Node>();

        DrawPaths(ref visited, ref emptyList, new Node(0, 0), result.Path);

        NodesVisitedTextBox.Text = result.VisitedNodes.Count().ToString();
        PathLengthTextBox.Text = $"{result.PathLength} / {result.Path.Count}";
        TimeTakenTextBox.Text = $"{_timingStopwatch.Elapsed.TotalMilliseconds} ms";
    }

    /// <summary>
    /// Luo kartan annetusta tiedostosta ja piirtää sen.
    /// </summary>
    /// <param name="pathToMap">Polku tiedostoon. Suhteellinen ja absoluuttinen polku käy</param>
    private void InitMap(string pathToMap)
    {
        _map = Input.ReadMapFromFile(pathToMap);
        //_map = Input.ReadMapFromImage(pathToMap);

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
    private void Callback(IEnumerable<Node> visited, List<Node> queue, Node current)
    {
        ShouldCallCallback = false;

        Dispatcher.UIThread.InvokeAsync(() =>
        {
            DrawPaths(ref visited, ref queue, current, null);
            TimeTakenTextBox.Text = $"{_timingStopwatch.Elapsed.TotalMilliseconds} ms";
            ShouldCallCallback = true;
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
    private void DrawPaths(ref IEnumerable<Node> visited, ref List<Node> queue, Node? current, List<Node>? path)
    {
        using (var frameBuffer = _bitmap.Lock())
        {
            unsafe
            {
                uint* buffer = (uint*)frameBuffer.Address.ToPointer();
                int stride = frameBuffer.RowBytes / sizeof(uint);

                var visitedCount = 0;
                foreach (var node in visited)
                {
                    buffer[node.Y * stride + node.X] = ToBgr(Brushes.LightGreen.Color);
                    visitedCount++;
                }
                NodesVisitedTextBox.Text = visitedCount.ToString();

                foreach (var node in queue)
                {
                    buffer[node.Y * stride + node.X] = ToBgr(Brushes.DarkOrange.Color);
                }
                
                if (path != null)
                {
                    foreach (var node in path)
                    {
                        buffer[node.Y * stride + node.X] = ToBgr(Brushes.Blue.Color);
                    }
                }
                
                if (current != null)
                {
                    buffer[current.Value.Y * stride + current.Value.X] = ToBgr(Brushes.DarkRed.Color);
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