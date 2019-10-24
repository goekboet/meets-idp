// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
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
    }
}