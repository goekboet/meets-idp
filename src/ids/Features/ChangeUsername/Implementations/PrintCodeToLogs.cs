using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace Ids.ChangeUsername
{
    public class PrintCodeToLogs : IExchange
    {
        private readonly ILogger<PrintCodeToLogs> _logger;
        public PrintCodeToLogs(
            ILogger<PrintCodeToLogs> logs
        )
        {
            _logger = logs;
        }

        public Task<Result<string>> Exchange(
            HttpResponse r,
            ResetToken t)
        {
            r.OnCompleted(() => 
            {
                _logger.LogInformation($"{t.Email} {t.Token}");
                return Task.CompletedTask;
            });
            
            return Task.FromResult<Result<string>>(new Ok<string>(t.Token));
        }
    }
}