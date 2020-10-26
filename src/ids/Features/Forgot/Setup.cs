using Microsoft.Extensions.DependencyInjection;

namespace Ids.Forgot
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