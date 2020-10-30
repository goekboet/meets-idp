using Microsoft.Extensions.DependencyInjection;

namespace Ids.ChangeUsername
{
    public static class SetupChangeUsernameExtension
    {
        public static void SetupChangeUsername(
            this IServiceCollection c
        )
        {
            c.AddScoped<IToken, AspIdentityToken>();
            c.AddSingleton<IExchange, PrintCodeToLogs>();
            c.AddScoped<IUsername, AspIdentityChangeUsername>();
        }
    }
}