using System.ComponentModel.DataAnnotations;
namespace LGMS.Data.Models.Authentication
{
    public class RegisterUser
    {
        [Required(ErrorMessage = "Field is required")]
        public string? Username { get; set; }

        [Required(ErrorMessage = "Field is required")]
        public string? Email { get; set; }

        [Required(ErrorMessage = "Field is required")]
        public string? Password { get; set; }
    }
}
