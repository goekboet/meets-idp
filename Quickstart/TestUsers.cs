// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using IdentityModel;
using IdentityServer4.Test;
using System.Collections.Generic;
using System.Security.Claims;

namespace IdentityServer4.Quickstart.UI
{
    public class TestUsers
    {
        public static List<TestUser> Users = new List<TestUser>
        {
            new TestUser{SubjectId = "1", Username = "dev", Password = "dev", 
                Claims = 
                {
                    new Claim(JwtClaimTypes.Name, "Developer"),
                    new Claim("principal", "dev")
                }
            },
            new TestUser{SubjectId = "2", Username = "cal1", Password = "cal1", 
                Claims = 
                {
                    new Claim(JwtClaimTypes.Name, "Generic caller 1"),
                }
            },
            new TestUser{SubjectId = "3", Username = "cal2", Password = "cal2", 
                Claims = 
                {
                    new Claim(JwtClaimTypes.Name, "Generic caller 2")
                }
            },
            new TestUser{SubjectId = "4", Username = "usr1@hst1", Password = "usr1@hst1", 
                Claims = 
                {
                    new Claim(JwtClaimTypes.Name, "User 1 at Host 1"),
                    new Claim("principal", "host1")
                }
            },
            new TestUser{SubjectId = "4", Username = "usr2@hst1", Password = "usr2@hst1", 
                Claims = 
                {
                    new Claim(JwtClaimTypes.Name, "User 2 at Host 1"),
                    new Claim("principal", "host1")
                }
            },
            new TestUser{SubjectId = "4", Username = "usr1@hst2", Password = "usr1@hst2", 
                Claims = 
                {
                    new Claim(JwtClaimTypes.Name, "User 1 at Host 2"),
                    new Claim("principal", "host2")
                }
            },
            new TestUser{SubjectId = "4", Username = "usr2@hst2", Password = "usr2@hst2", 
                Claims = 
                {
                    new Claim(JwtClaimTypes.Name, "User 2 at Host 2"),
                    new Claim("principal", "host2")
                }
            }
        };
    }
}