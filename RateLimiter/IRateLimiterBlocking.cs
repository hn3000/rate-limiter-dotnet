namespace RateLimiter
{
    interface IRateLimiterBlocking : IRateLimiter
    {
        void WaitForAmount(double amount);
    }
}