using System.IO;
using System.Security.Cryptography;
using Microsoft.IdentityModel.Tokens;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.OpenSsl;
using Org.BouncyCastle.Security;
using Org.BouncyCastle.Asn1.Pkcs;

namespace pki
{
    public class RsaPemService
    {
        public static RsaSecurityKey PrivateKeyFromPem(string keyPairPem)
        {
            PemReader pemReader = new PemReader(new StringReader(keyPairPem));
            AsymmetricCipherKeyPair keyPair = (AsymmetricCipherKeyPair)pemReader.ReadObject();
            RsaPrivateCrtKeyParameters privateKeyParameters = (RsaPrivateCrtKeyParameters)keyPair.Private; 
            RSAParameters rsaParameters = DotNetUtilities.ToRSAParameters(privateKeyParameters);
            return new RsaSecurityKey(rsaParameters);
        }

        public static RsaSecurityKey PublicKeyFromPem(string publicKeyPem)
        {
            PemReader pemReader = new PemReader(new StringReader(publicKeyPem));
            RsaKeyParameters publicKeyParameters = (RsaKeyParameters)pemReader.ReadObject();
            RSAParameters rsaParameters = DotNetUtilities.ToRSAParameters(publicKeyParameters);
            return new RsaSecurityKey(rsaParameters);
        }
    }
}
