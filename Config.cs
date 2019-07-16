// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


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
                    Name = "call",
                    DisplayName = "Call",
                    Description = "Provides access to means of communication on a meetingpoint.",
                    Scopes = 
                    { 
                        new Scope 
                        { 
                            Name = "call", 
                            DisplayName = "Call",
                            Description = "Participate in meetings" 
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
                    ClientId = "hosted_principals",
                    ClientName = "Principals without an own domain.",

                    AllowedGrantTypes = GrantTypes.Code,
                    ClientSecrets = { new Secret("hosted".Sha256()) },

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