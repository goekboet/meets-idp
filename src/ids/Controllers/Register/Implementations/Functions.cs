using Microsoft.AspNetCore.Identity;
using System.Linq;

namespace IdentityServer4.Quickstart.UI
{
    public static class RegisterFunctions
    {
        public static Result<T> ToAppError<T>(
            this IdentityResult r
        )
        {
            var reasons = 
                from e in r.Errors
                select $"{e.Code}: {e.Description}";

            var msg = string.Join(", ", reasons);

            return new Error<T>(msg);
        }
    }
}