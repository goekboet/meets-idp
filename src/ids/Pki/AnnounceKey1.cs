using System.IO;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using IdentityServer4;
using IdentityServer4.Stores;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace gateway.Pki
{
    public class LegacyKeySigningKeyStore : ISigningCredentialStore
    {
        public static string RS256 = IdentityServerConstants.RsaSigningAlgorithm.RS256.ToString();
        IFileProvider Files { get; }
        LegacyKey Key {get;}

        public LegacyKeySigningKeyStore(
            IWebHostEnvironment env,
            IOptionsSnapshot<LegacyKey> k)
        {
            Files = env.ContentRootFileProvider;
            Key = k.Value;
        }

        public async Task<SigningCredentials> GetSigningCredentialsAsync()
        {
            var certFile = Files.GetFileInfo(Key.Path);
            using var ms = new MemoryStream();
            await certFile.CreateReadStream().CopyToAsync(ms);

            var cert = new X509Certificate2(ms.ToArray(), Key.Pass);

            return new SigningCredentials(
                new X509SecurityKey(cert), 
                RS256);
        }
    }
}