using System.ComponentModel.DataAnnotations;

namespace desco_report_server.DTOs.Auth
{
    public class LoginDto
    {
        [Required(ErrorMessage = "Username or email is required")]
        public string UsernameOrEmail { get; set; } = string.Empty;

        [Required(ErrorMessage = "Password is required")]
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;

        public bool RememberMe { get; set; }
    }
}