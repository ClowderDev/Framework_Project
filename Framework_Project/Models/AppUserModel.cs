
using Microsoft.AspNetCore.Identity;

namespace Framework_Project.Models
{
    public class AppUserModel : IdentityUser
    {
        public string Occuapation { get; set; }
        public string RoleId { get; set; }

        public string Token { get; set; }

    }
}
