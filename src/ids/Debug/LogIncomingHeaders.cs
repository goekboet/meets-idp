using System;
using System.Linq;
using System.Text;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Logging;

namespace Microsoft.AspNetcore.Builder
{
    public static class IApplicationBuilderExtensions
    {
        /// <summary>
        /// Logs all request headers
        /// </summary>
        /// <param name="app"><see cref="IApplicationBuilder"/></param>
        /// <param name="loggerFactory"<see cref="ILoggerFactory"/></param>
        public static void LogRequestHeaders(
            this IApplicationBuilder app, 
            ILoggerFactory loggerFactory)
        {
            var logger = loggerFactory.CreateLogger("Request.Headers");
            app.Use(async (context, next) =>
            {
                var builder = new StringBuilder(Environment.NewLine);
                builder.AppendLine($"Method: {context.Request.Method}");
                builder.AppendLine($"Path: {context.Request.Path}");
                foreach (var cookie in context.Request.Headers)
                {
                    builder.AppendLine($"{cookie.Key}:{cookie.Value}");
                }
                logger.LogInformation(builder.ToString());
                await next.Invoke();
            });
        }
    }
}