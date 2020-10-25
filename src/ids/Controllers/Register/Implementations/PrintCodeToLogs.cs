using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace IdentityServer4.Quickstart.UI
{
    public class PrintCodeToLogs : ICodeDistribution
    {
        private readonly ILogger<PrintCodeToLogs> _logger;
        public PrintCodeToLogs(
            ILogger<PrintCodeToLogs> logs
        )
        {
            _logger = logs;
        }

        public Task<Result<Unit>> Send(UnverifiedAccount acct)
        {
            _logger.LogInformation($"{acct.Email} {acct.VerificationCode}");
            
            return Task.FromResult<Result<Unit>>(new Ok<Unit>(new Unit()));
        }
    }
}