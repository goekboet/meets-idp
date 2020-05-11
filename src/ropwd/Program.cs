using System;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using IdentityModel.Client;

namespace ropwd
{
    // {"Email":"5d2f481cdf1a48dd@byappt.com","Password":"26cff80ced5e4ed6"}
    class Program
    {
        static HttpClient Client { get; } = new HttpClient();
        static async Task Main(string[] args)
        {
            Client.BaseAddress = new Uri(args[0]);
            var clientId = "dev";
            var clientSecret = "dev";
            var user = args[1];
            var pwd = args[2];
            var scopes = String.Join(' ', args.Skip(3));

            var disco = await Client.GetDiscoveryDocumentAsync(new DiscoveryDocumentRequest());
            if (disco.IsError)
            {
                Console.WriteLine($"{disco.Error}");
                return;
            }

            var r = await Client.RequestPasswordTokenAsync(new PasswordTokenRequest
            {
                Address = disco.TokenEndpoint,

                ClientId = clientId,
                ClientSecret = clientSecret,
                Scope = scopes,

                UserName = user,
                Password = pwd
            });
            if (r.IsError)
            {
                Console.WriteLine($"{r.Error} {r.ErrorDescription}");
                return;
            }

            Console.WriteLine(r.AccessToken);
        }
    }
}
