using Microsoft.Extensions.DependencyInjection;

namespace IdentityServer4.Quickstart.UI
{
    public static class SetupRegisterExtension
    {
        public static void SetupRegister(
            this IServiceCollection c
        )
        {
            c.AddScoped<IAccountRegistration, RegisterAccountInDb>();
            c.AddScoped<IAccountActivation, ActivateAccountInDb>();
            c.AddSingleton<ICodeDistribution, PrintCodeToLogs>();
        }
    }
}