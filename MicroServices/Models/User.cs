using Microsoft.AspNetCore.Identity;

namespace MicroServices.Models
{
    public class User : IdentityUser<int>
    {
        public string Fullname { get; set; }
    }
}
