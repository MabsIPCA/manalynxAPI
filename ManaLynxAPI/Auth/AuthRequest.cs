using System.Text.Json;
using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ManaLynxAPI.Models
{
    public class AuthRequest
    {
        [Required]
        public string Username { get; set; } = string.Empty;
        [Required]
        public string Password { get; set; } = string.Empty;
    }

    [NotMapped]
    public class RegisterRequest : AuthRequest
    {
        [Required]
        public string Email { get; set; } = string.Empty;
        public string UserRole { get; set; } = string.Empty;
    }

    public class RegisterRoleRequest : ManaUser
    {
        public string Password { get; set; } = string.Empty;
    }
}
