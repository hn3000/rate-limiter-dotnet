using System;
using System.Threading;
using System.Transactions;

namespace RateLimiter
{
    class RateLimiterBlocking : RateLimiterTokenBucket, IRateLimiterBlocking
    {
        public new static RateLimiterBlocking CreateInstance(
            double ratePerSecond, 
            double maxAmount, 
            double initAmount = 0.0,
            ITimeSource timeSource = default(ITimeSource)
        ) {
            return new RateLimiterBlocking(ratePerSecond, maxAmount, Math.Min(initAmount, maxAmount), timeSource);
        }

        RateLimiterBlocking(double ratePerSecond, 
            double maxAmount, 
            double initAmount,
            ITimeSource timeSource = default(ITimeSource)
        ) : base(ratePerSecond, maxAmount, initAmount, timeSource)
        {
        }

        public void WaitForAmount(double amount=1.0)
        {
            Thread.Sleep(TimeSpan.FromTicks(WaitTimeForAmount(amount)));
        }
    }
}