﻿// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using System;
using Microsoft.IdentityModel.Logging;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Formatting.Elasticsearch;
using Microsoft.Extensions.Configuration;

namespace gateway
{
    public class Program
    {
        public static LoggerConfiguration SwitchLogger(
            string key, 
            LoggerConfiguration logger)
        {
            switch (key)
            {
                case "Console":
                    logger.WriteTo.Console();
                    break;
                case "StdOutJson":
                    logger.WriteTo.Console(new ElasticsearchJsonFormatter());
                    break;
                default:
                    throw new ArgumentOutOfRangeException($"key: {key}");
            }

            return logger;
        }

        public static void Main(string[] args)
        {
            Console.Title = "meets.idgateway";

            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Microsoft.Extensions.Hosting.Host.CreateDefaultBuilder(args)
                .ConfigureAppConfiguration((hostingContext, config) =>
            {
                config.AddJsonFile("configuration.json", 
                    optional: true, 
                    reloadOnChange: true);
            })
                .ConfigureWebHostDefaults(webBuilder =>
        {
            webBuilder
                .UseStartup<Startup>()
                .UseSerilog((context, configuration) =>
                    {
                        IdentityModelEventSource.ShowPII = true;
                        var key = context.Configuration["Serilog:Configuration"];
                        SwitchLogger(key, configuration);
                    });
        });
    }
}