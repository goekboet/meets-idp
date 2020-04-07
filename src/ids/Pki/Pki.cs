using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using IdentityServer4;
using IdentityServer4.Models;
using IdentityServer4.Stores;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.FileProviders;
using Microsoft.IdentityModel.Tokens;

namespace gateway.Pki
{
    

    public class SigningKey : ISigningCredentialStore
    {
        public IFileProvider Files {get;}

        public SigningKey(
            IWebHostEnvironment env)
        {
            Files = env.ContentRootFileProvider;
        }

        public static SecurityKey ToSigningKey(IdsSingingCredential creds)
        {
            using var reader = new StreamReader(creds.Pem);
            
            return RsaFromPem.PrivateKeyFromPem(reader.ReadToEnd());
        }

        public Task<SigningCredentials> GetSigningCredentialsAsync()
        {
            var files = Files.GetDirectoryContents("signingcreds");
            var creds = files.Select(RsaFromPem.ToCreds).OrderBy(x => x.Order).ToList();
            
            IdsSingingCredential signingCreds = null;
            if (creds.Count == 0)
            {
                throw new ArgumentException();
            }
            else if (creds.Count() == 1)
            {
                signingCreds = creds[0];
            }
            else
            {
                signingCreds = creds[1];
            }
            var key = ToSigningKey(signingCreds);
            
            return Task.FromResult(new SigningCredentials(key, "RS256"));
        }
    }

    public class ValidationKey : IValidationKeysStore
    {
        IFileProvider Files {get;}
        public ValidationKey(
            IWebHostEnvironment env
        )
        {
            Files = env.ContentRootFileProvider;
        }

        public static SecurityKey ToValidationKey(IdsSingingCredential creds)
        {
            using var reader = new StreamReader(creds.Pem);
            
            return RsaFromPem.PrivateKeyFromPem(reader.ReadToEnd());
        }

        public Task<IEnumerable<SecurityKeyInfo>> GetValidationKeysAsync()
        {
            var files = Files.GetDirectoryContents("signingcreds");
            var creds = files.Select(RsaFromPem.ToCreds).OrderBy(x => x.Order).ToList();

            return Task.FromResult(creds
                .Select(ToValidationKey)
                .Select(x => new SecurityKeyInfo
                {
                    Key = x,
                    SigningAlgorithm = "RS256" 
                }));
        }
    }
}