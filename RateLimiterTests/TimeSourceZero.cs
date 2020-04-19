using RateLimiter;

namespace RateLimiterTests
{
    class TimeSourceZero : ITimeSource
    {
        public long CurrentTimeTicks()
        {
            return 0L;
        }
    }
}