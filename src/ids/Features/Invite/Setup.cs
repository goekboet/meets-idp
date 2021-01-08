using Microsoft.Extensions.DependencyInjection;

namespace Ids.Invite
{
    public static class Setup
    {
        public static void SetupInvite(
            this IServiceCollection s
        )
        {
            s.AddScoped<IInvitation, AspIdentityInvitation>();
        }
    }
}