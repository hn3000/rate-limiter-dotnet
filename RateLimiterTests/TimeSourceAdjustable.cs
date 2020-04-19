using RateLimiter;

namespace RateLimiterTests
{
    class TimeSourceAdjustable : ITimeSource
    {

        public TimeSourceAdjustable(long startTime = 0L)
        {
            _currentTime = startTime;
        }

        public long CurrentTimeTicks()
        {
            return _currentTime;
        }

        public void AddTimeTicks(long ticks)
        {
            _currentTime += ticks;
        }

        public void SetTime(long ticks)
        {
            _currentTime = ticks;
        }
        
        private long _currentTime;
    }
}