using System.ComponentModel.DataAnnotations;

namespace Interview_Project.Models.DTOs.Account
{
    public class LoginDto
    {
        [Required]
        public string UserName { get; set; }
        [Required]
        public string Password { get; set; }
    }

    public class GoogleLoginDto
    {
       
        public string IdToken { get; set; }
        
    }
}
