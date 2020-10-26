using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Ids.Forgot
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