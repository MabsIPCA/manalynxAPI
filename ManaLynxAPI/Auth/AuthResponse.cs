using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ManaLynxAPI.Models
{
    public class AuthResponse
    {
        public string Name { get; set; }
        public string Token { get; set; }

        public DateTime Expiration { get; set; }

        public AuthResponse()
        {
            Name = string.Empty;
            Token = string.Empty;
            Expiration = DateTime.MinValue;
        }

        public AuthResponse(string user, string token)
        {
            Name = user;
            Token = token;
            Expiration = DateTime.Now.AddMinutes(120);
        }

        public AuthResponse(AuthRequest user, string token, DateTime expires)
        {
            Name = user.Username;
            Token = token;
            Expiration = expires;
        }
    }
}
