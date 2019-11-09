// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using IdentityServer4.Quickstart.UI;
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
                            b => b.MigrationsAssembly("gateway")));

            services.AddIdentity<IdsUser, IdentityRole>(options => 
                {
                    options.Password.RequireDigit = false;
                    options.Password.RequireNonAlphanumeric = false;
                    options.Password.RequireUppercase = false; 
                    options.User.RequireUniqueEmail = true;
                })
                .AddEntityFrameworkStores<UsersDb>()
                .AddDefaultTokenProviders();

            var builder = services.AddIdentityServer(options =>
            {
                Configuration.GetSection("IdentityServerOptions").Bind(options);
                // options.Events.RaiseErrorEvents = true;
                // options.Events.RaiseInformationEvents = true;
                // options.Events.RaiseFailureEvents = true;
                // options.Events.RaiseSuccessEvents = true;
                //options.Discovery = new IdentityServer4.Configuration.DiscoveryOptions
            })
                .AddAspNetIdentity<IdsUser>()
                .AddInMemoryIdentityResources(Config.GetIdentityResources())
                .AddInMemoryApiResources(Config.GetApis())
                .AddInMemoryClients(Configuration.GetSection("clients"))
                .AddOperationalStore(options => {
                    options.ConfigureDbContext = builder => 
                    {
                        builder.UseNpgsql(
                            Configuration.GetConnectionString("Grants"),
                            b => b.MigrationsAssembly("gateway"));

                    };
                    options.EnableTokenCleanup = true;
                });

            
            
            services.AddControllersWithViews();

            services.AddAuthentication()
                .AddOpenIdConnect("proper", opts =>
                {
                    Configuration.GetSection("Proper").Bind(opts);
                });

            if (Env.EnvironmentName == "Development")
            {
                builder.AddDeveloperSigningCredential();
            }
            else
            {
                var keymaterial = Configuration.GetSection("Keymaterial");
                
                var cert = new X509Certificate2(
                    keymaterial.GetValue<string>("Path"), 
                    keymaterial.GetValue<string>("Pass"));

                builder.AddSigningCredential(cert);
            }
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