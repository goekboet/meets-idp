using IdentityServer4.Models;

namespace Ids.Home
{
    public class ErrorDescription
    {
        public ErrorDescription(string error)
        {
            Error = new ErrorMessage { Error = error };
        }

        public ErrorMessage Error { get; set; }
    }
}