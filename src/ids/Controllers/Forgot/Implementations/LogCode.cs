using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace IdentityServer4.Quickstart.UI
{
    public class LogCode : IExchange
    {
        private readonly ILogger<LogCode> _logger;

        public LogCode(
            ILogger<LogCode> logger
        )
        {
            _logger = logger;
        }

        public Task<Result<Unit>> Exchange(ResetToken t)
        {
            _logger.LogInformation($"{t.Email} {t.Token}");

            return Task.FromResult<Result<Unit>>(new Ok<Unit>(new Unit()));
        }
    }
}