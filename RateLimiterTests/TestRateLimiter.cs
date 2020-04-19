using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using RateLimiter;

namespace RateLimiterTests
{

    [TestClass]
    public class TestRateLimiter
    {

        private long TicksPerSecond(double seconds)
        {
            return (long) Math.Ceiling(seconds * (double)TimeSpan.TicksPerSecond);
        }

        [TestMethod]
        public void TestTicksPerSecondFractions()
        {
            Assert.AreEqual(
                TicksPerSecond(0.01+0.99), 
                TicksPerSecond(0.01)+TicksPerSecond(0.99)
            );
        }

        [TestMethod]
        public void TestRateLimiterCreateWorksWithoutInitAmountAndTimeSource()
        {
            var rateLimiter = RateLimiterTokenBucket.CreateInstance(1.0, 5.0);
            
            Assert.IsNotNull(rateLimiter);
            Assert.IsTrue(TicksPerSecond(1) >= rateLimiter.WaitTimeForAmount(1.0));
            Assert.IsTrue(TicksPerSecond(1) >= rateLimiter.WaitTimeForAmount());
            Assert.IsTrue(TicksPerSecond(0.95) <= rateLimiter.WaitTimeForAmount(1.0));
            Assert.IsTrue(TicksPerSecond(0.95) <= rateLimiter.WaitTimeForAmount());
        }

        [TestMethod]
        public void TestRateLimiterGivesOutInitAmount()
        {
            var rateLimiter = RateLimiterTokenBucket.CreateInstance(1.0, 5.0, 1.0, new TimeSourceZero());
            
            Assert.IsTrue(rateLimiter.ConsumeAmount(1.0));
        }

        [TestMethod]
        public void TestRateLimiterDoesNotWaitForInitAmount()
        {
            var rateLimiter = RateLimiterTokenBucket.CreateInstance(1.0, 5.0, 1.0, new TimeSourceZero());
            
            Assert.AreEqual(0, rateLimiter.WaitTimeForAmount(1.0));
            Assert.AreEqual(0, rateLimiter.WaitTimeForAmount());
        }

        [TestMethod]
        public void TestRateLimiterConsumesAdditionalAmount()
        {
            var timeSource = new TimeSourceAdjustable(0);
            var rateLimiter = RateLimiterTokenBucket.CreateInstance(1.0, 5.0, 1.0, timeSource);
            
            Assert.AreEqual(TicksPerSecond(0), rateLimiter.WaitTimeForAmount(1.0));
            Assert.AreEqual(TicksPerSecond(1), rateLimiter.WaitTimeForAmount(2.0));

            timeSource.AddTimeTicks(TicksPerSecond(0.999));

            Assert.AreEqual(TicksPerSecond(0), rateLimiter.WaitTimeForAmount(1.0));
            Assert.AreEqual(TicksPerSecond(0.001), rateLimiter.WaitTimeForAmount(2.0));


            timeSource.SetTime(TicksPerSecond(1));

            Assert.AreEqual(TicksPerSecond(0), rateLimiter.WaitTimeForAmount(1.0));
            Assert.AreEqual(TicksPerSecond(0), rateLimiter.WaitTimeForAmount(2.0));
        }

        [TestMethod]
        public void TestRateLimiterWontGiveOutMoreThanMaxAmount()
        {
            var timeSource = new TimeSourceAdjustable(0);
            var rateLimiter = RateLimiterTokenBucket.CreateInstance(1.0, 5.0, 6.0, timeSource);

            Assert.AreEqual(RateLimiterConstants.WaitImpossible, rateLimiter.WaitTimeForAmount(6.0));
            Assert.IsFalse(rateLimiter.ConsumeAmount(6.0));
        }

        
        
        [TestMethod]
        public void TestRateLimiterWontReturnImpossiblyLongWaitTime()
        {
            var timeSource = new TimeSourceAdjustable(0);
            var rateLimiter = RateLimiterTokenBucket.CreateInstance(1e-9, 1e10, 6.0, timeSource);

            Assert.AreEqual(RateLimiterConstants.WaitImpossible, rateLimiter.WaitTimeForAmount(1e9));
        }
    }


    class TimeSourceZero : ITimeSource
    {
        public long CurrentTimeTicks()
        {
            return 0L;
        }
    }
    
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
