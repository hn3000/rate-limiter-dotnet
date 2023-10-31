using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using RateLimiter;

namespace RateLimiterTests
{

    [TestClass]
    public class TestRateLimiterAsync
    {

        [TestMethod]
        public async Task TestConsumeWhenAvailableWaitsTheRequiredTime()
        {
            var limiter = RateLimiterAsync.CreateInstance(1, 2);
            Task<bool> delayedPermission = limiter.ConsumeAmountWhenAvailable(1);
            var sw = Stopwatch.StartNew();
            var result = await delayedPermission;
            var timeTaken = sw.Elapsed;
            
            Assert.IsTrue(timeTaken <= TimeSpan.FromSeconds(1.10), $"{timeTaken} ! <= 1.10");
            Assert.IsTrue(timeTaken >= TimeSpan.FromSeconds(0.90), $"{timeTaken} ! >= 0.90");
            Assert.IsTrue(result);
        }

        [TestMethod]
        public async Task TestConsumeWhenAvailableSupportsCancellation()
        {
            var limiter = RateLimiterAsync.CreateInstance(1, 2);
            var cancel = new CancellationTokenSource();
            Task<bool> delayedPermission = limiter.ConsumeAmountWhenAvailable(1, cancel.Token);
            var sw = Stopwatch.StartNew();

            cancel.Cancel();

            var result = await delayedPermission;
            var timeTaken = sw.Elapsed;

            Assert.IsFalse(result);
            Assert.IsTrue(timeTaken <= TimeSpan.FromSeconds(0.05), $"{timeTaken} ! <= 0.05");
            Assert.IsTrue(timeTaken >= TimeSpan.FromSeconds(0.0), $"{timeTaken} ! >= 1.05");
        }

        [TestMethod]
        public async Task TestConsumeWhenAvailableSupportsCancellationPreCancelled()
        {
            var limiter = RateLimiterAsync.CreateInstance(1, 2);
            var cancel = new CancellationTokenSource();
            cancel.Cancel();
            Task<bool> delayedPermission = limiter.ConsumeAmountWhenAvailable(1, cancel.Token);
            var sw = Stopwatch.StartNew();

            var result = await delayedPermission;
            var timeTaken = sw.Elapsed;

            Assert.IsFalse(result);
            Assert.IsTrue(timeTaken <= TimeSpan.FromSeconds(0.05), $"{timeTaken} ! <= 0.05");
            Assert.IsTrue(timeTaken >= TimeSpan.FromSeconds(0.0), $"{timeTaken} ! >= 1.05");
        }

        [TestMethod]
        public async Task TestConsumeWhenAvailableWaitsTheRequiredTimeWithSeparateRateLimiter()
        {
            var baseLimiter = RateLimiterTokenBucket.CreateInstance(1, 2);
            var limiter = RateLimiterAsync.CreateInstance(baseLimiter);
            Task<bool> delayedPermission = limiter.ConsumeAmountWhenAvailable(1);
            var sw = Stopwatch.StartNew();
            var result = await delayedPermission;
            var timeTaken = sw.Elapsed;
            
            Assert.IsTrue(timeTaken <= TimeSpan.FromSeconds(1.01));
            Assert.IsTrue(timeTaken >= TimeSpan.FromSeconds(0.99));
            Assert.IsTrue(result);
        }

        [TestMethod]
        public async Task TestConsumeWhenAvailableReturnsImmediatelyWhenPossible()
        {
            var limiter = RateLimiterAsync.CreateInstance(1, 2, 2);
            Task<bool> delayedPermission = limiter.ConsumeAmountWhenAvailable(2);
            var sw = Stopwatch.StartNew();
            var result = await delayedPermission;
            var timeTaken = sw.Elapsed;
            
            Assert.IsTrue(timeTaken <= TimeSpan.FromSeconds(0.02), $"{timeTaken} ! <= 0.02");
            Assert.IsTrue(timeTaken >= TimeSpan.FromSeconds(0.0), $"{timeTaken} ! >= 0.00");
            Assert.IsTrue(result);
        }

        [TestMethod]
        public async Task TestConsumeWhenAvailableRejectsImpossibleAmount()
        {
            var limiter = RateLimiterAsync.CreateInstance(1, 2);
            Task<bool> delayedPermission = limiter.ConsumeAmountWhenAvailable(3);
            var sw = Stopwatch.StartNew();
            var result = await delayedPermission;
            var timeTaken = sw.Elapsed;
            
            Assert.IsTrue(timeTaken <= TimeSpan.FromSeconds(0.01), $"{timeTaken} ! <= 0.01");
            Assert.IsTrue(timeTaken >= TimeSpan.FromSeconds(0.0), $"{timeTaken} ! >= 0.0");
            Assert.IsFalse(result);
        }

        [TestMethod]
        public async Task TestConsumeWhenAvailableWaitsTheRequiredTimeWithMultipleConsumptions()
        {
            var limiter = RateLimiterAsync.CreateInstance(1, 2, 0);

            var sw = Stopwatch.StartNew();

            Task<bool> delayedPermission1 = limiter.ConsumeAmountWhenAvailable(1);
            Task<bool> delayedPermission2 = limiter.ConsumeAmountWhenAvailable(1);

            var ttt1 = delayedPermission1.ContinueWith((t) => sw.Elapsed);
            var ttt2 = delayedPermission2.ContinueWith((t) => sw.Elapsed);

            await Task.WhenAll(delayedPermission1, delayedPermission2, ttt1, ttt2);

            var result1 = await delayedPermission1;
            var timeTaken1 = await ttt1;
            var result2 = await delayedPermission2;
            var timeTaken2 = await ttt2;
            
            Assert.IsTrue(timeTaken1 <= TimeSpan.FromSeconds(1.05), $"{timeTaken1} ! <= 1.05");
            Assert.IsTrue(timeTaken1 >= TimeSpan.FromSeconds(0.95), $"{timeTaken1} ! >= 0.95");
            Assert.IsTrue(result1);
            Assert.IsTrue(timeTaken2 <= TimeSpan.FromSeconds(2.05), $"{timeTaken1} ! <= 2.05");
            Assert.IsTrue(timeTaken2 >= TimeSpan.FromSeconds(1.95), $"{timeTaken2} ! >= 1.95");
            Assert.IsTrue(result2);
        }

        [TestMethod]
        public async Task TestConsumeWhenAvailableRejectsImpossibleTimeImmediatelyWithMultipleConsumptions()
        {
            var limiter = RateLimiterAsync.CreateInstance(1, 2, 0);

            var sw = Stopwatch.StartNew();

            Task<bool> delayedPermission1 = limiter.ConsumeAmountWhenAvailable(1);
            Task<bool> delayedPermission2 = limiter.ConsumeAmountWhenAvailable(3);

            var ttt1 = delayedPermission1.ContinueWith((t) => sw.Elapsed);
            var ttt2 = delayedPermission2.ContinueWith((t) => sw.Elapsed);

            await Task.WhenAll(delayedPermission1, delayedPermission2, ttt1, ttt2);

            var result1 = await delayedPermission1;
            var timeTaken1 = await ttt1;
            var result2 = await delayedPermission2;
            var timeTaken2 = await ttt2;
            
            Assert.IsTrue(timeTaken1 <= TimeSpan.FromSeconds(1.05), $"{timeTaken1} ! <= 0.1");
            Assert.IsTrue(timeTaken1 >= TimeSpan.FromSeconds(0.95), $"{timeTaken1} ! >= 0.95");
            Assert.IsTrue(result1);
            Assert.IsTrue(timeTaken2 <= TimeSpan.FromSeconds(0.1), $"{timeTaken2} ! <= 0.1");
            Assert.IsTrue(timeTaken2 >= TimeSpan.FromSeconds(0.0), $"{timeTaken2} ! >= 0.0");
            Assert.IsFalse(result2);
        }

    }

}
