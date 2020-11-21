using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;

namespace seed_users
{
    public record UserSeed(
        string id,
        string username, 
        string pwd, 
        string hashedPwd);

    class Program
    {
        static Random Rng { get; } = new Random();
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

        static UserSeed RandomUserSeed(int i)
        {
            var id = Guid.NewGuid().ToString();
            var user = new IdentityUser() { UserName = $"testuser-{i}@email.com" };
            var pwd = RandomString(16);
            var pwdHash = HashFunction.HashPassword(user, pwd);

            return new UserSeed(id, user.UserName, pwd, pwdHash);
        }

        static string ToCsv(UserSeed u) => $"{u.id},{u.username},{u.pwd},{u.hashedPwd}";
        
        static async Task Main(string[] args)
        {
            var ts = DateTimeOffset.Now.ToUnixTimeMilliseconds().ToString();

            var q = 
                from i in Enumerable.Range(0, 100000)
                select ToCsv(RandomUserSeed(i));

            await File.WriteAllLinesAsync($"out-{ts}.csv", q.ToArray());
        }
    }
}
