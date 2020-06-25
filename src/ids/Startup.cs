// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using System.Security.Cryptography.X509Certificates;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.DataProtection;
using System.IO;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Ids.Features.EmailUsername;
using gateway.Pki;
using Microsoft.AspNetCore.Http;

namespace gateway
{
    public class Startup
    {
        public IConfiguration Configuration { get; }
        public IWebHostEnvironment Env {get;}

        public Startup(
            IConfiguration configuration,
            IWebHostEnvironment env)
        {
            Configuration = configuration;
            Env = env;
        }

        public void ConfigureServices(
            IServiceCollection services)
        {
            services.Configure<ForwardedHeadersOptions>(options =>
            {
                options.ForwardedHeaders = ForwardedHeaders.XForwardedFor |
                    ForwardedHeaders.XForwardedProto;
                
                // Per default kestrel only forwards proxy-headers from localhost. You need to add
                // ip-numbers into these lists or empty them to forward all.
                options.KnownNetworks.Clear();
                options.KnownProxies.Clear();
            });

            if (Configuration["Dataprotection:Type"] == "Docker")
            {
                services.AddDataProtection()
                    .PersistKeysToFileSystem(
                        new DirectoryInfo(Configuration["Dataprotection:KeyPath"])
                    )
                    .ProtectKeysWithCertificate(
                        new X509Certificate2(
                            Configuration["Dataprotection:CertPath"],
                            Configuration["Dataprotection:CertPass"]
                        )
                    );
            }

            services.AddDbContext<UsersDb>(options =>
                options.UseNpgsql(
                            Configuration.GetConnectionString("Users"),
                            b => b.MigrationsAssembly("ids")));

            services.AddIdentity<IdsUser, IdentityRole>(options => 
                {
                    options.Password.RequireDigit = false;
                    options.Password.RequireNonAlphanumeric = false;
                    options.Password.RequireUppercase = false; 
                    options.User.RequireUniqueEmail = true;
                })
                .AddEntityFrameworkStores<UsersDb>()
                .AddDefaultTokenProviders();

            services.ConfigureApplicationCookie(opts =>
            {
                opts.Cookie.Name = "sso";
                opts.Cookie.IsEssential = true;
                opts.Cookie.SameSite = SameSiteMode.Lax;
            });

            var builder = services.AddIdentityServer(options =>
            {
                Configuration.GetSection("IdentityServerOptions").Bind(options);
            })
                .AddAspNetIdentity<IdsUser>()
                .AddResourceOwnerValidator<MapUsernameToEmail>()
                .AddInMemoryIdentityResources(Config.GetIdentityResources())
                .AddInMemoryApiResources(Configuration.GetSection("Apis"))
                .AddInMemoryApiScopes(Configuration.GetSection("ApiScopes"))
                .AddInMemoryClients(Configuration.GetSection("Clients"))
                .AddOperationalStore(options => {
                    options.ConfigureDbContext = builder => 
                    {
                        builder.UseNpgsql(
                            Configuration.GetConnectionString("Grants"),
                            b => b.MigrationsAssembly("ids"));

                    };
                    options.EnableTokenCleanup = true;
                });

            services.SetupKeyStore();
            
            services.AddControllersWithViews();

            services.AddAuthentication()
                .AddOpenIdConnect("Okta", "Okta", opts =>
                {
                    Configuration.GetSection("Okta").Bind(opts);
                    opts.Scope.Add("email");
                    opts.SaveTokens = true;
                })
                .AddGitHub("Github", opts => 
                {
                    Configuration.Bind("Github", opts);
                    opts.Scope.Add("read:user");
                    opts.Scope.Add("user:email");
                    opts.SaveTokens = true;
                });

            
        }

        public void Configure(
            IApplicationBuilder app)
        {
            app.UseForwardedHeaders();
            
            if (Env.EnvironmentName == "Development")
            {
                app.UseDeveloperExceptionPage();
            }
            
            app.UseStaticFiles();
            app.UseRouting();

            app.UseIdentityServer();

            app.UseAuthorization();
            app.UseEndpoints(endpoints =>
            {
               endpoints.MapDefaultControllerRoute();
            });
        }
    }
}