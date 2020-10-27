using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;

namespace Ids.Logout
{
    public class LogoutInput
    {
        [MaxLength(2048)]
        public string LogoutId { get; set;}
    }
}