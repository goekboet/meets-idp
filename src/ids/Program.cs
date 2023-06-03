using Ids.AspIdentity;
using Ids.Authorization;
using Ids.ChangeUsername;
using Ids.Forgot;
using Ids.Hosting;
using Ids.Identityserver4;
using Ids.Invite;
using Ids.Login;
using Ids.NonLocal;
using Ids.Profile;
using Ids.Register;
using Ids.Unregister;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);
var configuration = builder.Configuration;
configuration.AddJsonFile("configuration.json", 
    optional: true, 
    reloadOnChange: true);

var services = builder.Services;
services.SetupProxying();          
services.SetupDataProtection(configuration);
services.ConfigureAspIdentity(configuration);
services.ConfigureIdentityServer4(configuration);
services.SetupAuthorization();

services.AddAuthentication()
    .AddGithub(configuration);

services.AddControllersWithViews();
services.SetupRegister();
services.SetupUnregister();
services.SetupForgot();
services.SetupLogin();
services.SetupProfile();
services.SetupChangeUsername();
services.SetupInvite();

var app = builder.Build();

app.UseForwardedHeaders();
app.UseExceptionHandler("/error");
app.UseStaticFiles();
app.UseRouting();
app.UseIdentityServer();
app.UseAuthorization();
app.MapDefaultControllerRoute();

app.Run();