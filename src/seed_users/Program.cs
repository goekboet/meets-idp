using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;

namespace seed_users
{
    public record UserSeed(
        string id,
        string username, 
        string pwd, 
        string hashedPwd,
        string securityStamp);

    class Program
    {
        static Random Rng { get; } = new Random();
        static MD5 Md5 {get; } = MD5.Create();
        const string chars = "abcdefghijklmnopqrstuvxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";

        static char GetRandomChar => chars[Rng.Next(chars.Length)];

        static string RandomString(int length)
        {
            var q = 
                from _ in Enumerable.Range(0, length)
                select GetRandomChar;
               
            return new string(q.ToArray());
        }
        
        static PasswordHasher<IdentityUser> HashFunction { get; } =
            new PasswordHasher<IdentityUser>();

        static string RandomHash()
        {
            var uuid = Guid.NewGuid().ToByteArray();

            var hashedBs = Md5.ComputeHash(uuid);
            var sb = new StringBuilder();
            for (int i = 0; i < hashedBs.Length; i++)
            {
                sb.Append(hashedBs[i].ToString("x2"));
            }

            return sb.ToString();
        }

        static UserSeed RandomUserSeed(int i)
        {
            var id = Guid.NewGuid().ToString();
            var user = new IdentityUser() { UserName = $"testuser-{i}@email.com" };
            var pwd = RandomString(16);
            var pwdHash = HashFunction.HashPassword(user, pwd);
            var stamp = RandomHash();

            return new UserSeed(id, user.UserName, pwd, pwdHash, stamp);
        }

        static string ToCsv(UserSeed u) => $"{u.id},{u.username},{u.pwd},{u.hashedPwd},{u.securityStamp}";
        
        static async Task Main(string[] args)
        {
            var ts = DateTimeOffset.Now.ToUnixTimeMilliseconds().ToString();

            var q = 
                from i in Enumerable.Range(0, 10000)
                select ToCsv(RandomUserSeed(i));

            await File.WriteAllLinesAsync($"out-{ts}.csv", q.ToArray());
        }
    }
}
