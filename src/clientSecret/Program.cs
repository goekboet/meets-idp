using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;

namespace s2sha256
{
    class Program
    {
        static string Show(ICollection<string> g) => string.Join(", ", g);

        public static string ComputeSHA256Hash(string text)
        {
            using var sha256 = new SHA256Managed();
            var bs = Encoding.UTF8.GetBytes(text);
            var hash = sha256.ComputeHash(bs);

            return Convert.ToBase64String(hash);
        }

        static int Main(string[] args)
        {

            if (args.Length < 1)
            {
                Console.Error.WriteLine("Useage: s2sha256 ss. Requires 1 or more argument(s).");
                return 1;
            }
            foreach (var s in args)
            {
                Console.WriteLine($"{s} {ComputeSHA256Hash(s)}");
            }
            Console.WriteLine();


            return 0;
        }
    }
}
