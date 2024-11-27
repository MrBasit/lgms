using System.ComponentModel.DataAnnotations;
namespace LGMS.Data.Models.Authentication
{
    public class UserLogin
    {
        [Required(ErrorMessage = "Username is required")]
        public String Username { get; set; }

        [Required(ErrorMessage = "Password is required")]
        public string Password { get; set; }
    }
}
