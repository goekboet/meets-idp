using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using IdentityServer4;
using IdentityServer4.Models;
using IdentityServer4.Stores;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using JsonWebKey = Microsoft.IdentityModel.Tokens.JsonWebKey;

namespace gateway.Pki
{
    public class AnnounceLegacyKeyWithMigratedKeystore : IValidationKeysStore
    {
        public static string RS256 = IdentityServerConstants.RsaSigningAlgorithm.RS256.ToString();
        IFileProvider Files { get; }
        LegacyKey Key { get; }

        public AnnounceLegacyKeyWithMigratedKeystore(
            IWebHostEnvironment env,
            IOptionsSnapshot<LegacyKey> k
        )
        {
            Files = env.ContentRootFileProvider;
            Key = k.Value;
        }

        public async Task<IEnumerable<SecurityKeyInfo>> GetValidationKeysAsync()
        {
            async Task<SecurityKeyInfo> IncludeLegacyKey()
            {
                using var ms = new MemoryStream();
                await Files.GetFileInfo(Key.Path)
                    .CreateReadStream()
                    .CopyToAsync(ms);

                var cert = new X509Certificate2(ms.ToArray(), Key.Pass);

                return new SecurityKeyInfo
                {
                    Key = new X509SecurityKey(cert),
                    SigningAlgorithm = RS256
                };
            }

            async Task<SecurityKeyInfo> FromRotationKeyStore(IFileInfo file)
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

            var keyset =
                Files.GetDirectoryContents("rotation")
                .Select(FromRotationKeyStore)
                .Concat(new[] { IncludeLegacyKey() });

            return await Task.WhenAll(keyset);
        }
    }
}