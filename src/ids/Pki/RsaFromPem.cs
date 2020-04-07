using System;
using System.IO;
using System.Security.Cryptography;
using Microsoft.Extensions.FileProviders;
using Microsoft.IdentityModel.Tokens;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.OpenSsl;
using Org.BouncyCastle.Security;

namespace gateway.Pki
{
    public class IdsSingingCredential
    {
        public int Order { get; set; }
        public Stream Pem {get;set;}
    }

    public static class RsaFromPem
    {
        public static RsaSecurityKey PrivateKeyFromPem(string keyPairPem)
        {
            var pemReader = new PemReader(new StringReader(keyPairPem));
            var keyPair = (AsymmetricCipherKeyPair)pemReader.ReadObject();
            var privateKeyParameters = (RsaPrivateCrtKeyParameters)keyPair.Private; 
            var rsaParameters = DotNetUtilities.ToRSAParameters(privateKeyParameters);
            
            return new RsaSecurityKey(rsaParameters);
        }

        public static RsaSecurityKey PublicKeyFromPem(string publicKeyPem)
        {
            var pemReader = new PemReader(new StringReader(publicKeyPem));
            var publicKeyParameters = (RsaKeyParameters)pemReader.ReadObject();
            var rsaParameters = DotNetUtilities.ToRSAParameters(publicKeyParameters);
            
            return new RsaSecurityKey(rsaParameters);
        }

        public static IdsSingingCredential ToCreds(IFileInfo f)
        {
            var n = f.Name.Split('.');
            if (n.Length != 3)
            {
                throw new ArgumentException();
            }

            return new IdsSingingCredential
            {
                Order = Int16.Parse(n[1]),
                Pem = f.CreateReadStream()
            };
        }
    }
}