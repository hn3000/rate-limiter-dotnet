using System.Threading;
using System.Threading.Tasks;

namespace RateLimiter
{
    public interface IRateLimiterAsync : IRateLimiter
    {
        Task<bool> ConsumeAmountWhenAvailable(double amount = 1.0,
            CancellationToken cancel = default(CancellationToken));
    }
}
