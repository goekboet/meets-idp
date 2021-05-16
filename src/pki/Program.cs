using System.IO;
using Microsoft.IdentityModel.Tokens;
using System;
using Duende.IdentityServer.Configuration;
using System.Text.Json;
using System.Linq;

namespace pki
{
    class Program
    {
        static int Main(string[] args)
        {
            var path = args.Length > 0
                ? args[0]
                : "rotation";

            var keyOrder = 
                Directory.EnumerateFiles(path)
                .Select(x => Int32.Parse(x.Split('.')[1]))
                .OrderByDescending(x => x)
                .ToList();
            
            var nextFilename = $"pkey.{keyOrder.FirstOrDefault() + 1}.jwk";
            var key = CryptoHelper.CreateRsaSecurityKey();
            var jwk = JsonWebKeyConverter.ConvertFromRSASecurityKey(key);
            var json = JsonSerializer.Serialize(jwk);
            
            File.WriteAllText($"{path}/{nextFilename}", json);
            foreach (var retire in keyOrder.Skip(2))
            {
                File.Delete($"{path}/pkey.{retire}.jwk");
            }

            return 1;
        }
    }
}
