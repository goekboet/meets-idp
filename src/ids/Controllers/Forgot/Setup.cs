using Microsoft.Extensions.DependencyInjection;

namespace IdentityServer4.Quickstart.UI
{
    public static class SetupForgotExtension
    {
        public static void SetupForgot(
            this IServiceCollection c
        )
        {
            c.AddScoped<IToken, AspIdentityResetToken>();
            c.AddSingleton<IExchange, LogCode>();
            c.AddScoped<IReset, AspIdentityResetPassword>();
        }
    }
}