using System;

namespace RateLimiter
{
    public class RateLimiterTokenBucket : IRateLimiter
    {
        // Number of 100ns ticks per time unit (copied from DateTime)
        private const long TicksPerMillisecond = 10000;
        private const long TicksPerSecond = TicksPerMillisecond * 1000;
        
        public static RateLimiterTokenBucket CreateInstance(
            double ratePerSecond, 
            double maxAmount, 
            double initAmount = 0.0,
            ITimeSource timeSource = default(ITimeSource)
        ) {
            return new RateLimiterTokenBucket(ratePerSecond, maxAmount, Math.Min(initAmount, maxAmount), timeSource);
        }

        RateLimiterTokenBucket(double ratePerSecond, double maxAmount, double initAmount, ITimeSource timeSource)
        {
            _ratePerTick = ratePerSecond / TicksPerSecond;
            _maxAmount = maxAmount;
            _currentAmount = initAmount;
            _timeSource = timeSource ?? S_DefaultTimeSource;
            _lastTimeTicks = _timeSource.CurrentTimeTicks();
        }
        
        public bool ConsumeAmount(double numPermits = 1.0)
        {
            _updateAmount();
            bool success = _currentAmount >= numPermits;
            if (success) {
                _currentAmount -= numPermits;
            }
            return success;
        }

        public long WaitTimeForAmount(double numPermits = 1.0)
        {
            if (numPermits <= _currentAmount) {
                return 0;
            }
            _updateAmount();

            if (numPermits <= _currentAmount) {
                return 0;
            }
            if (numPermits > _maxAmount) {
                return RateLimiterConstants.WaitImpossible;
            }
    
            double lack = numPermits - _currentAmount;
            double waitTime = Math.Ceiling(lack / _ratePerTick);
            if (waitTime > RateLimiterConstants.WaitImpossible) {
                return RateLimiterConstants.WaitImpossible;
            }
            return (long)waitTime;        }
        
        private void _updateAmount()
        {
            long now = _timeSource.CurrentTimeTicks();
            long deltaTicks = now - _lastTimeTicks;
            if (deltaTicks > 0) {
                double adjust = deltaTicks * _ratePerTick;
                adjust += _currentAmount;
                _currentAmount = Math.Min(adjust, _maxAmount);
                _lastTimeTicks = now;
            }
        }

        
        private readonly double _ratePerTick;
        private readonly double _maxAmount;
        private double _currentAmount;
        private long   _lastTimeTicks;
  
        private readonly ITimeSource _timeSource;
  
        private static readonly ITimeSource S_DefaultTimeSource = new TimeSourceSystemTicks();

        private class TimeSourceSystemTicks : ITimeSource
        {
            public long CurrentTimeTicks()
            {
                return DateTime.Now.Ticks;
            }
        }
    }
    
}