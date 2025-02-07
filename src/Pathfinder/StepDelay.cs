using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Pathfinder
{
    public class StepDelay
    {
        Stopwatch _timingStopwatch;
        long _timingNodeCounter;
        private TimeSpan _targetStepDelay;

        public StepDelay(TimeSpan targetStepDelay)
        {
            _targetStepDelay = targetStepDelay;
            _timingStopwatch = Stopwatch.StartNew();
            _timingNodeCounter = 0;
        }

        public void UpdateTargetStepDelay(TimeSpan targetStepDelay)
        {
            _targetStepDelay = targetStepDelay;
            _timingStopwatch = Stopwatch.StartNew();
            _timingNodeCounter = 0;
        }

        public void Wait()
        {
            double elapsedMs = _timingStopwatch.Elapsed.TotalMilliseconds;
            double targetDelay = _timingNodeCounter * _targetStepDelay.TotalMilliseconds;
            if (elapsedMs < targetDelay)
            {
                Thread.Sleep((int)Math.Max(1, targetDelay - elapsedMs));
            }

            _timingNodeCounter++;
        }
    }
}
