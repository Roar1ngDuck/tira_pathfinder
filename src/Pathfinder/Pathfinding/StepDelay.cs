using System;
using System.Diagnostics;
using System.Threading;

namespace Pathfinder.Pathfinding;

public class StepDelay
{
    private Stopwatch _timingStopwatch { get; set; }
    private long _timingNodeCounter { get; set; }
    private TimeSpan _targetStepDelay { get; set; }

    /// <summary>
    /// Hallitsee viivettä
    /// </summary>
    /// <param name="targetStepDelay">Haluttu keskiarvo viiveelle</param>
    public StepDelay(TimeSpan targetStepDelay)
    {
        _targetStepDelay = targetStepDelay;
        _timingStopwatch = Stopwatch.StartNew();
        _timingNodeCounter = 0;
    }

    /// <summary>
    /// Asettaa uuden viiveen
    /// </summary>
    /// <param name="targetStepDelay">Haluttu keskiarvo viiveelle</param>
    public void SetTargetStepDelay(TimeSpan targetStepDelay)
    {
        _targetStepDelay = targetStepDelay;
        _timingStopwatch = Stopwatch.StartNew();
        _timingNodeCounter = 0;
    }

    /// <summary>
    /// Pysäyttää threadin pyrkien pitämään keskimääräisen viiveen valitussa arvossa
    /// </summary>
    public void Wait()
    {
        if (_targetStepDelay.TotalMilliseconds == 0)
        {
            return;
        }

        double elapsedMs = _timingStopwatch.Elapsed.TotalMilliseconds;
        double targetDelay = _timingNodeCounter * _targetStepDelay.TotalMilliseconds;
        if (elapsedMs < targetDelay)
        {
            Thread.Sleep((int)Math.Max(1, targetDelay - elapsedMs));
        }

        _timingNodeCounter++;
    }
}
