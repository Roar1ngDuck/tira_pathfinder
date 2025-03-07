using System;
using System.Diagnostics;

namespace Pathfinder.Pathfinding;

public class CallbackInterval
{
    private Stopwatch _timingStopwatch;
    private long _timingNodeCounter;
    private TimeSpan _targetInterval;

    /// <summary>
    /// Hallitsee aikaa joka on kulunut callbackin viimeisestä kutsusta
    /// </summary>
    /// <param name="targetInterval">Haluttu aika kutsujen välillä</param>
    public CallbackInterval(TimeSpan targetInterval)
    {
        _targetInterval = targetInterval;
        _timingStopwatch = Stopwatch.StartNew();
        _timingNodeCounter = 0; SetTargetInterval(targetInterval);
    }

    /// <summary>
    /// Asettaa halutun ajan kutsujen välille
    /// </summary>
    /// <param name="targetInterval">Haluttu aika kutsujen välillä</param>
    public void SetTargetInterval(TimeSpan targetInterval)
    {
        _targetInterval = targetInterval;
        _timingStopwatch = Stopwatch.StartNew();
        _timingNodeCounter = 0;
    }

    /// <summary>
    /// Tarkistaa jos aikaa on kulunut tarpeeksi jotta callback voitaisi kutsua uudestaan
    /// </summary>
    /// <returns>True jos callback pitäisi kutsua</returns>
    public bool ShouldCallCallback()
    {
        if (_targetInterval.TotalMilliseconds == 0)
        {
            return true;
        }

        double elapsedMs = _timingStopwatch.Elapsed.TotalMilliseconds;
        double targetInterval = _timingNodeCounter * _targetInterval.TotalMilliseconds;

        if (elapsedMs > targetInterval)
        {
            _timingNodeCounter++;

            return true;
        }

        return false;
    }
}
