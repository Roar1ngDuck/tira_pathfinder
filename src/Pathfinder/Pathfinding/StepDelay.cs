using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Pathfinder.Pathfinding
{
    public class StepDelay
    {
        private Stopwatch _timingStopwatch;
        private long _timingNodeCounter;
        private TimeSpan _targetStepDelay;

        public StepDelay(TimeSpan targetStepDelay)
        {
            SetTargetStepDelay(targetStepDelay);
        }

        public void SetTargetStepDelay(TimeSpan targetStepDelay)
        {
            _targetStepDelay = targetStepDelay;
            _timingStopwatch = Stopwatch.StartNew();
            _timingNodeCounter = 0;
        }

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
}
