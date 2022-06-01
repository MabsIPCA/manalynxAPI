using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Text;
using System.Security.Cryptography;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using ManaLynxAPI.Data;
using ManaLynxAPI.Models;
using ManaLynxAPI.Utils;

namespace ManaLynxAPI.Controllers
{
    public interface IJWTAuthManager
    {
        ManaUser? ValidateUser(AuthRequest login);
        bool UserExists(ManaUser user);
        string GenerateTokenString(ManaUser user, DateTime expires, Claim[]? claims = null);
        Roles? GetRole(string roleName);
        (int?, Roles?) GetClaims(string token);
    }

    public class JWTAuthManager : Controller, IJWTAuthManager
    {
        private readonly ApplicationDbContext _db;
        private readonly ILoginCredentialUtils _login;
        private readonly IConfiguration _config;
        private readonly string tokenKey;

        public JWTAuthManager(ApplicationDbContext db, IConfiguration c, ILoginCredentialUtils l)
        {
            _config = c;                            //para aceder a appsettings.json
            tokenKey = _config["Jwt:Key"];
            _db = db;
            _login = l;
        }

        public string GenerateTokenString(ManaUser user, DateTime expires, Claim[]? claims = null)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(tokenKey));
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims ?? new Claim[]
                {
                    new Claim("Id", user.Id.ToString()),
                    new Claim(ClaimTypes.Name, user.Username),
                    new Claim(ClaimTypes.Email, user.Email),
                    new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                    new Claim(ClaimTypes.Role, user.UserRole.ToString()),
                }),
                Issuer = _config["Jwt:Issuer"],
                Audience = _config["Jwt:Audience"],
                Expires = expires.AddMinutes(60),
                //NotBefore = expires,
                SigningCredentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256Signature),
            };

            return tokenHandler.WriteToken(tokenHandler.CreateToken(tokenDescriptor));
        }

        #region Utils

        public ManaUser? ValidateUser(AuthRequest logRequest)
        {
            if (string.IsNullOrEmpty(logRequest.Username) || string.IsNullOrEmpty(logRequest.Password)) return null;
            var user = _db.ManaUsers.Where(u => u.Username.Equals(logRequest.Username)).Select(u => u).FirstOrDefault();
            if (user is null || !user.Username.Equals(logRequest.Username, StringComparison.Ordinal)) return null;


            var login = _db.LoginCredentials.Find(user.LoginCredential);
            if (login is null) return null;
            var hash = _login.HashPassword(login.ManaSalt, logRequest.Password);


            if (hash.Equals(login.ManaHash))
            { 
                return user;
            }
            return null;
        }

        public bool UserExists(ManaUser user)
        {
            var usr = _db.ManaUsers.Where(u => u.Email.Equals(user.Username) || u.Username.Equals(user.Username)).FirstOrDefault();
            if(usr is null) return false;
            return true;
        }

        public bool UserNameExists(string username)
        {
            var usr = _db!.ManaUsers.Where(u => u.Username.Equals(username)).FirstOrDefault();
            if (usr is null) return false;
            return true;
        }

        public Roles? GetRole(string roleName)
        {
            foreach(Roles role in Enum.GetValues(typeof(Roles)))
            {
                if (role.ToString()!.Equals(roleName))
                    return role;
            }
            return null;
        }

        public (int?, Roles?) GetClaims(string token)
        {
            var jwtSecurityToken = new JwtSecurityTokenHandler().ReadJwtToken(token);
            var reqIdToken = jwtSecurityToken.Claims.First(claim => claim.Type == "Id").Value;
            if (int.TryParse(reqIdToken, out int reqId) is false) return (null, null);
            var roleName = jwtSecurityToken.Claims.First(claim => claim.Type == "role").Value;
            var roleToken = GetRole(roleName);
            return (reqId, roleToken);
        }
        #endregion
    }

}
