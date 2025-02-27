using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Pathfinder.Pathfinding
{
    public class CallbackInterval
    {
        private Stopwatch _timingStopwatch;
        private long _timingNodeCounter;
        private TimeSpan _targetInterval;

        public CallbackInterval(TimeSpan targetInterval)
        {
            SetTargetInterval(targetInterval);
        }

        public void SetTargetInterval(TimeSpan targetStepDelay)
        {
            _targetInterval = targetStepDelay;
            _timingStopwatch = Stopwatch.StartNew();
            _timingNodeCounter = 0;
        }

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
}
