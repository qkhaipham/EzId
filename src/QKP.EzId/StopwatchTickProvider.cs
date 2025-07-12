using System;
using System.Diagnostics;

namespace QKP.EzId
{
    /// <summary>
    /// Implementation of <see cref="ITickProvider"/> that uses a Stopwatch for precise time tracking.
    /// </summary>
    internal class StopwatchTickProvider : ITickProvider
    {
        private readonly Stopwatch _stopWatch;
        private readonly long _initializedTicksInMs;
        private static readonly DateTimeOffset s_epoch = DateTimeOffset.UnixEpoch;

        /// <summary>
        /// Initializes a new instance of the <see cref="StopwatchTickProvider"/> class.
        /// </summary>
        public StopwatchTickProvider()
        {
            _stopWatch = new Stopwatch();
            _initializedTicksInMs = (long)(DateTimeOffset.UtcNow - s_epoch).TotalMilliseconds;
            _stopWatch.Start();
        }

        /// <inheritdoc />
        public long GetTick()
        {
            return _initializedTicksInMs + _stopWatch.ElapsedTicks / TimeSpan.TicksPerMillisecond;
        }
    }
}
