using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IdentityServer4;
using IdentityServer4.Models;
using IdentityServer4.Stores;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.FileProviders;
using Microsoft.IdentityModel.Tokens;
using JsonWebKey = Microsoft.IdentityModel.Tokens.JsonWebKey;

namespace gateway.Pki
{


    public class FullyMigratedSigningKeyStore : ISigningCredentialStore
    {
        public static string RS256 = IdentityServerConstants.RsaSigningAlgorithm.RS256.ToString();
        public IFileProvider Files { get; }

        public FullyMigratedSigningKeyStore(
            IWebHostEnvironment env)
        {
            Files = env.ContentRootFileProvider;
        }

        public async Task<SigningCredentials> GetSigningCredentialsAsync()
        {
            async Task<SigningCredentials> FromFile(IFileInfo file)
            {
                using var ms = new MemoryStream();
                await file
                    .CreateReadStream()
                    .CopyToAsync(ms);

                var json = Encoding.UTF8.GetString(ms.ToArray());
                var jwk = new JsonWebKey(json);
                
                return new SigningCredentials(
                    jwk,
                    RS256);
            }

            var files = Files.GetDirectoryContents("rotation")
                .OrderByDescending(x => Int32.Parse(x.Name.Split('.')[1]))
                .ToList();

            switch (files.Count())
            {
                case 0:
                    throw new ArgumentOutOfRangeException("There must be at least 1 key in the keystore.");
                case 1:
                    return await FromFile(files[0]);
                default:
                    return await FromFile(files[1]);
            }
        }
    }

    public class FullyMigratedKeysetStore : IValidationKeysStore
    {
        public static string RS256 = IdentityServerConstants.RsaSigningAlgorithm.RS256.ToString();
        IFileProvider Files { get; }

        public FullyMigratedKeysetStore(
            IWebHostEnvironment env
        )
        {
            Files = env.ContentRootFileProvider;
        }

        public async Task<IEnumerable<SecurityKeyInfo>> GetValidationKeysAsync()
        {
            async Task<SecurityKeyInfo> FromFile(IFileInfo file)
            {
                using var ms = new MemoryStream();
                await file
                    .CreateReadStream()
                    .CopyToAsync(ms);

                var json = Encoding.UTF8.GetString(ms.ToArray());
                var jwk = new JsonWebKey(json);

                return new SecurityKeyInfo
                {
                    Key = jwk,
                    SigningAlgorithm = RS256
                };
            }

            var files =
                Files.GetDirectoryContents("rotation")
                .Select(FromFile);

            return await Task.WhenAll(files);
        }
    }
}