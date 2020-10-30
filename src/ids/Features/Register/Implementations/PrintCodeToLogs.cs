using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace Ids.ResetPassword
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

        public Task<Result<string>> Send(
            HttpResponse r,
            UnverifiedAccount acct)
        {
            r.OnCompleted(() => 
            {
                _logger.LogInformation($"{acct.UserId} {acct.VerificationCode}");
                return Task.CompletedTask;
            });
            
            return Task.FromResult<Result<string>>(new Ok<string>(acct.VerificationCode));
        }
    }
}