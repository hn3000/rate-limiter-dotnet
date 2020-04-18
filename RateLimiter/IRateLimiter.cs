using System;

namespace RateLimiter
{
    /**
     * An IRateLimiter provides a way to rate limit events. Each event is assigned 
     * an amount of some imaginary resource and the IRateLimiter restricts the 
     * amount per time that can be consumed.
     * 
     * If no amount is specified, each event is worth an amount of 1.0 and the 
     * configuration of the specific IRateLimiter implementation would specify 
     * the rate at which consumable amount is created.
     * 
     * The utility interface ITimeSource should be used by implementations
     * to make them testable.
     * 
     * If a rate limiter will never allow an event of a certain amount, it
     * will return a waiting time of RateLimiterConstants.WaitImpossible
     * from WaitTimeForAmount() and ConsumeAmount() will always return false.
     * 
     * This type of rate limiter can be used to rate limit incoming API calls
     * by dropping calls that exceed the limit. It can also be used to limit
     * outgoing API calls by waiting the time returned by WaitTimeForAmount(),
     * since this call does not consume the amount, this can only work properly
     * in single-threaded applications when using implementations of this 
     * interface. For multi-threaded operation the rate limiter needs to be
     * locked when checking if the required amount of permits is available.
     * 
     * @author Harald Niesche
     *
     */
    
    public interface IRateLimiter
    {
        bool ConsumeAmount(double numPermits = 1.0);
        long WaitTimeForAmount(double numPermits = 1.0);
    }

    /**
     * Constants for IRateLimiter implementations: when a request for permits can
     * not be satisfied, WaitImpossible will be returned instead.
     */
    public class RateLimiterConstants
    {
        public const long WaitImpossible = long.MaxValue;
    }

    /**
     * Utility interface for testability: IRateLimiter instances should use an
     * implementation of this interface to obtain the current time.
     */
    public interface ITimeSource
    {
        /**
         * Current Time in Ticks -- 100ns intervals since 0001-01-01_00:00
         */
        long CurrentTimeTicks();
    }
}