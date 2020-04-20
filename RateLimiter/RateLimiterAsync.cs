using System;
using System.Threading;
using System.Threading.Tasks;

namespace RateLimiter
{
    public class RateLimiterAsync : IRateLimiterAsync
    {
        public static IRateLimiterAsync CreateInstance(IRateLimiter that)
        {
            return new RateLimiterAsync(that);
        }

        public static IRateLimiterAsync CreateInstance(
            double ratePerSecond,
            double maxAmount,
            double initAmount = 0.0,
            ITimeSource timeSource = default(ITimeSource)
        )
        {
            var that = RateLimiterTokenBucket.CreateInstance(ratePerSecond, maxAmount, initAmount, timeSource);
            return new RateLimiterAsync(that);
        }

        protected RateLimiterAsync(IRateLimiter rateLimiter)
        {
            _rateLimiter = rateLimiter;
            _lastTask = null;
        }

        public bool ConsumeAmount(double numPermits = 1)
        {
            return _rateLimiter.ConsumeAmount(numPermits);
        }

        public long WaitTimeForAmount(double numPermits = 1)
        {
            return _rateLimiter.WaitTimeForAmount(numPermits);
        }

        /**
         * If different processes are asking for different amounts, processes asking for larger amounts
         * risk starvation.
         *
         * Our strategy to avoid starvation is a first-come -- first-serve policy: If we observe congestion,
         * the first process to ask for an amount gets a reserve for that amount and waits until it's available.
         *
         * If a queue already exists, we're adding ourselves to the queue -- if there is no queue, we create
         * one if we have to wait for our amount.
         */
        public async Task<bool> ConsumeAmountWhenAvailable(double amount = 1,
            CancellationToken cancel = default(CancellationToken))
        {
            if (cancel.IsCancellationRequested)
            {
                return false;
            }
            
            var ticket = default(Task<bool>);
            lock (_lock)
            {
                var waitNow = WaitTimeForAmount(amount);
                if (waitNow == RateLimiterConstants.WaitImpossible)
                    // amount can never be satisfied
                    return false;

                if (0L != waitNow || null != _lastTask)
                {
                    var previous = currentTask();
                    ticket = waitAndConsume(previous, amount, cancel);
                    _lastTask = ticket;
                }
                else
                {
                    ticket = waitAndConsume(Task.FromResult(true), amount, cancel);
                }
            }

            var result = await ticket;

            taskDone(ticket);

            return result;
        }

        private async Task<bool> waitAndConsume(Task<bool> previous, double amount, CancellationToken cancel)
        {
            var whatever = await previous;

            while (!ConsumeAmount(amount))
            {
                var waitTime = WaitTimeForAmount(amount);
                //if (waitTime == RateLimiterConstants.WaitImpossible) return false;

                try
                {
                    await Task.Delay(TimeSpan.FromTicks(waitTime), cancel);
                }
                catch (TaskCanceledException)
                {
                    // we're interpreting the token cancellation
                    // as cancelling the consumption of an amount
                    return false;
                }

            }

            return true;
        }

        private Task<bool> currentTask()
        {
            return _lastTask ?? Task.FromResult(true);
        }

        private void taskDone(Task<bool> ticket)
        {
            lock (_lock)
            {
                if (_lastTask == ticket) _lastTask = null;
            }
        }

        private readonly IRateLimiter _rateLimiter;
        private Task<bool> _lastTask;
        private Object _lock = new Object();
    }
}