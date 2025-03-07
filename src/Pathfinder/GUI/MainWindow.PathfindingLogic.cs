using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Threading;
using Pathfinder.GUI;
using Pathfinder.Pathfinding;
using Pathfinder.Pathfinding.Algorithms;
using Pathfinder.Pathfinding.Utils;

namespace Pathfinder;

/// <summary>
/// Sisältää reitinhaun ja sen käyttöön liittyvän logiikan.
/// </summary>
public partial class MainWindow
{
    /// <summary>
    /// Aloittaa reitinhaun visualisoinnin
    /// </summary>
    /// <param name="mode">Valittu visualisointimoodi</param>
    public void StartVisualization(VisualizationMode mode)
    {
        ClearPreviousRun();
        if (MapTextBox.Text is null || StartTextBox.Text is null || GoalTextBox.Text is null)
        {
            return;
        }

        InitMap(MapTextBox.Text);

        var startNumbers = StartTextBox.Text.Split(',');
        var start = new Node(int.Parse(startNumbers[0]), int.Parse(startNumbers[1]));

        var goalNumbers = GoalTextBox.Text.Split(',');
        var goal = new Node(int.Parse(goalNumbers[0]), int.Parse(goalNumbers[1]));

        var algorithm = GetSelectedAlgorithm();

        var allowDiagonal = AllowDiagonalCheckBox.IsChecked.HasValue && AllowDiagonalCheckBox.IsChecked.Value;

        var stepDelayValue = Math.Pow(StepDelaySlider.Value, 4);

        _drawStopwatch = Stopwatch.StartNew();
        lastMs = 0;

        switch (mode)
        {
            case VisualizationMode.SinglePath:
                RunPathfinding(start, goal, algorithm, allowDiagonal, stepDelayValue);
                break;
            case VisualizationMode.RandomPathBenchmark:
                RunRandomPathBenchmark(algorithm, allowDiagonal);
                break;
        }
    }

    /// <summary>
    /// Suorittaa yksittäisen reitinetsinnän ja näyttää sen tulokset.
    /// </summary>
    /// <param name="start">Aloituspiste</param>
    /// <param name="goal">Maalipiste</param>
    /// <param name="algorithm">Valittu algoritmi</param>
    /// <param name="allowDiagonal">Vinottaiset siirtymät sallittu?</param>
    /// <param name="stepDelay">Viive mikrosekunteina</param>
    private async void RunPathfinding(
        Node start, 
        Node goal, 
        PathFindingAlgorithm algorithm, 
        bool allowDiagonal, 
        double stepDelay)
    {
        PathFindingResult result;

        if (stepDelay > 0)
        {
            _stepDelay = new StepDelay(TimeSpan.FromMicroseconds(stepDelay));
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
        if (visited is null)
        {
            return;
        }

        IEnumerable<Node> emptyList = new List<Node>();

        DrawPaths(ref visited, ref emptyList, new Node(0, 0), result.Path);

        NodesVisitedTextBox.Text = visited.Count().ToString();
        PathLengthTextBox.Text = $"{result.PathLength} / {result.Path?.Count}";
        TimeTakenTextBox.Text = $"{_timingStopwatch.Elapsed.TotalMilliseconds} ms";
    }

    /// <summary>
    /// Suorittaa useita satunnaisia polkuhakuja (benchmark) ja näyttää niiden keskimääräisen suoritusajan.
    /// </summary>
    /// <param name="algorithm">Valittu algoritmi</param>
    /// <param name="allowDiagonal">Vinottaiset siirtymät sallittu?</param>
    private void RunRandomPathBenchmark(PathFindingAlgorithm algorithm, bool allowDiagonal)
    {
        double totalTimeTaken = 0;
        var rnd = new Random();
        var randomPathCount = 100;

        for (int i = 0; i < randomPathCount; i++)
        {
            var start = new Node(rnd.Next(0, _map.GetLength(0)), rnd.Next(0, _map.GetLength(1)));
            var goal = new Node(rnd.Next(0, _map.GetLength(0)), rnd.Next(0, _map.GetLength(1)));

            if (DistanceUtils.EuclideanDistance(start, goal) < 100)
            {
                i--;
                continue;
            }

            _timingStopwatch = Stopwatch.StartNew();
            var result = algorithm.Search(start, goal, allowDiagonal);
            _timingStopwatch.Stop();

            var visited = result.VisitedNodes;
            IEnumerable<Node> emptyList = new List<Node>();

            DrawPaths(ref emptyList, ref emptyList, new Node(0, 0), result.Path);

            totalTimeTaken += _timingStopwatch.Elapsed.TotalMilliseconds;
        }

        double averageTime = Math.Round(totalTimeTaken / randomPathCount, 4);
        TimeTakenTextBox.Text = $"Average: {averageTime} ms";
    }

    /// <summary>
    /// Palauttaa valitun reitinhakualgoritmin ComboBoxin perusteella.
    /// </summary>
    /// <returns>Valittu algoritmi</returns>
    /// <exception cref="InvalidOperationException">Heitetään, jos indeksi on tuntematon</exception>
    private PathFindingAlgorithm GetSelectedAlgorithm()
    {
        return AlgorithmComboBox?.SelectedIndex switch
        {
            0 => new Dijkstra(_map),
            1 => new AStar(_map),
            2 => new JumpPointSearch(_map),
            _ => throw new InvalidOperationException(
                      $"Unsupported algorithm: {AlgorithmComboBox?.SelectedIndex}")
        };
    }

    double lastMs = 0;

    /// <summary>
    /// Funktio, jota reitinhakualgoritmi kutsuu jokaisen pisteen prosessoinnin yhteydessä. Piirtää tilannekuvan.
    /// </summary>
    /// <param name="visited">Kaikki läpikäydyt solmut</param>
    /// <param name="queue">Jonossa olevat solmut</param>
    /// <param name="current">Nyt käsittelyssä oleva solmu</param>
    private void Callback(IEnumerable<Node> visited, IEnumerable<Node> queue, Node current)
    {
        double ms = _drawStopwatch.Elapsed.TotalMilliseconds;
        var delta = ms - lastMs;
        if (delta < 16.667) // 60 fps
        {
            return;
        }
        lastMs = ms;

        Dispatcher.UIThread.InvokeAsync(() =>
        {
            try
            {
                DrawPaths(ref visited, ref queue, current, null);
                TimeTakenTextBox.Text = $"{_timingStopwatch.Elapsed.TotalMilliseconds} ms";
            }
            catch { }
        });
    }
}
