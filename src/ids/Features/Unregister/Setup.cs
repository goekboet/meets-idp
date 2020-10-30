using Microsoft.Extensions.DependencyInjection;

namespace Ids.Unregister
{
    public static class SetupUnregisterExtension
    {
        public static void SetupUnregister(
            this IServiceCollection c
        )
        {
            c.AddScoped<IAccountDeletion, AspIdentityUnregister>();
            c.AddSingleton<ICodeDistribution, PrintCodeToLogs>();
        }
    }
}