﻿// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Elastic.Apm.NetCoreAll;

using Ids.AspIdentity;
using Ids.Pki;
using Ids.Forgot;
using Ids.Login;
using Ids.Identityserver4;
using Ids.Hosting;
using Ids.Profile;
using Ids.Register;
using Ids.Unregister;

namespace Ids
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
            services.SetupProxying();          
            services.SetupDataProtection(Configuration);
            services.ConfigureAspIdentity(Configuration);
            services.ConfigureIdentityServer4(Configuration);
            
            services.AddControllersWithViews();
            services.SetupKeyStore();
            services.SetupRegister();
            services.SetupUnregister();
            services.SetupForgot();
            services.SetupLogin();
            services.SetupProfile();
        }

        public void Configure(
            IApplicationBuilder app)
        {
            if (!Configuration.GetValue<bool>("ElasticApm:OptOut"))
            {
                app.UseAllElasticApm(Configuration);
            }
            
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