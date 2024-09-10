using Microsoft.AspNetCore.Identity;

namespace MicroServicePatient.Models
{
    public class User : IdentityUser<int>
    {
        public string Fullname { get; set; }
        public string Role { get; set; }
    }
}
