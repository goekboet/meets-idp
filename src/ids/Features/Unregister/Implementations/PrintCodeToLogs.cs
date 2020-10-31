using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace Ids.Unregister
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
            ActiveAccount acct)
        {
            r.OnCompleted(() => 
            {
                _logger.LogInformation($"{acct.Email} {acct.Code}");
                return Task.CompletedTask;
            });
            
            return Task.FromResult<Result<string>>(new Ok<string>(acct.Code));
        }
    }
}