﻿// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

// dev    7yYOmqPGc68kDReiZgSANhqOCB0f/soqXtDjIZ/BhWc=
// trial  eYkemAdH/70h5pApc5Tv52T6Vtfjd1D4AIefuy00Vxo=
// proper hKyHSyMqAZ4Jkd2uUOtVd3+4NvmND+jv2ncuWSsqktk=
// meets  TZyfD1CybAcT1+jyyTw5VsRFXo7cM5R7gI45CowrrwQ=

using IdentityServer4.Models;
using System.Collections.Generic;

namespace gateway
{
    public static class Config
    {
        public static IEnumerable<IdentityResource> GetIdentityResources()
        {
            return new IdentityResource[]
            {
                new IdentityResources.OpenId(),
                new IdentityResources.Profile(),
                new IdentityResource
                {
                    Name = "principal",
                    DisplayName = "Principal",
                    Description = "Representing a principal.",
                    UserClaims = new [] { "name", "principal" }
                }
            };
        }

        public static IEnumerable<ApiResource> GetApis()
        {
            return new ApiResource[]
            {
                new ApiResource
                {
                    Name = "https://broker.ego",
                    DisplayName = "Meets broker api",
                    Description = "Provides access to means of communication on a meetingpoint.",
                    Scopes = 
                    { 
                        new Scope 
                        { 
                            Name = "bookings", 
                            DisplayName = "Manage my",
                            Description = "Add, remove and list bookings" 
                        }
                    }
                }
            };
        }

        public static IEnumerable<Client> GetClients()
        {
            return new[]
            {
                // MVC client using hybrid flow
                new Client
                {
                    ClientId = "dev",
                    ClientName = "Developer Account",

                    AllowedGrantTypes = GrantTypes.ResourceOwnerPassword,
                    ClientSecrets = { new Secret("dev".Sha256()) },

                    AllowOfflineAccess = true,
                    AllowedScopes = { "openid", "profile", "principal", "call" }
                },
                new Client
                {
                    ClientId = "trials",
                    ClientName = "Principals without an own domain.",

                    AllowedGrantTypes = GrantTypes.Code,
                    ClientSecrets = { new Secret("trials".Sha256()) },

                    RedirectUris = { "http://localhost:5020/signin-oidc" },
                    FrontChannelLogoutUri = "http://localhost:5020/signout-oidc",
                    PostLogoutRedirectUris = { "http://localhost:5020/signout-callback-oidc" },

                    AllowOfflineAccess = true,
                    AllowedScopes = { "openid", "profile", "principal"},
                    EnableLocalLogin = true
                },
                new Client
                {
                    ClientId = "proper_principal",
                    ClientName = "Principals with their own domain.",

                    AllowedGrantTypes = GrantTypes.Code,
                    ClientSecrets = { new Secret("proper".Sha256()) },

                    RedirectUris = { "http://localhost:5030/signin-oidc" },
                    FrontChannelLogoutUri = "http://localhost:5030/signout-oidc",
                    PostLogoutRedirectUris = { "http://localhost:5030/signout-callback-oidc" },

                    AllowOfflineAccess = true,
                    AllowedScopes = { "openid", "profile", "principal"},
                    EnableLocalLogin = false,
                    IdentityProviderRestrictions = new [] { "proper" }
                },
                new Client
                {
                    ClientId = "callers",
                    ClientName = "The callers",

                    AllowedGrantTypes = GrantTypes.Hybrid,
                    ClientSecrets = { new Secret("callers".Sha256()) },

                    RedirectUris = { "http://localhost:5010/signin-oidc" },
                    FrontChannelLogoutUri = "http://localhost:5010/signout-oidc",
                    PostLogoutRedirectUris = { "http://localhost:5010/signout-callback-oidc" },

                    AllowOfflineAccess = true,
                    AllowedScopes = { "openid", "profile", "call" }
                }
            };
        }
    }
}