using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text.Json;
using Duende.IdentityServer;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Ids.NonLocal
{
    public static class Setup
    {
        public static AuthenticationBuilder AddGithub(
            this AuthenticationBuilder auth,
            IConfiguration conf 
        )
        {
            auth.AddOAuth("github", opts => {
                conf.GetSection("Github").Bind(opts);
                opts.SignInScheme = IdentityServerConstants.ExternalCookieAuthenticationScheme;
                opts.CallbackPath = new("/github-oauth");
                opts.AuthorizationEndpoint = "https://github.com/login/oauth/authorize";
                opts.TokenEndpoint = "https://github.com/login/oauth/access_token";
                opts.UserInformationEndpoint = "https://api.github.com/user";

                opts.Scope.Add("read:user");
                opts.Scope.Add("user:email");

                opts.ClaimActions.MapJsonKey(ClaimTypes.NameIdentifier, "id");
                opts.ClaimActions.MapJsonKey(ClaimTypes.Name, "name");
                opts.ClaimActions.MapJsonKey(ClaimTypes.Email, "email");

                opts.Events.OnCreatingTicket = async ctx =>
                {
                    var request = new HttpRequestMessage(HttpMethod.Get, ctx.Options.UserInformationEndpoint);
                    request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                    request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", ctx.AccessToken);
                    
                    var response = await ctx.Backchannel.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, ctx.HttpContext.RequestAborted);
                    response.EnsureSuccessStatusCode();
                    
                    var body = await response.Content.ReadAsByteArrayAsync();
                    var d = JsonDocument.Parse(body);
                    ctx.RunClaimActions(d.RootElement);
                };
            });

            return auth;
        }
    }
}