using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Security.Cryptography;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Text;
using ManaLynxAPI.Data;
using ManaLynxAPI.Models;
using Newtonsoft.Json;

namespace ManaLynxAPI.Utils
{
    public interface ILoginCredentialUtils
    {
        LoginCredential? GenerateLoginCredentials(RegisterRequest register);
        string GetSalt();
        string HashPassword(string salt, string password);
    }

    public class LoginCredentialUtils : ILoginCredentialUtils
    {
        private readonly ApplicationDbContext _db;
        private readonly IConfiguration _config;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="db"></param>
        /// <param name="c"></param>
        public LoginCredentialUtils(ApplicationDbContext db, IConfiguration c)
        {
            _db = db;
            _config = c;
        }

        #region Methods
        public LoginCredential? GenerateLoginCredentials(RegisterRequest register)
        {
            var login = new LoginCredential();
            if (_db == null) return null;
            try
            {
                login.ManaSalt = GetSalt();
                login.ManaHash = HashPassword(login.ManaSalt, register.Password);
                if (login.ManaHash.Equals(string.Empty)) return null;
                _db.LoginCredentials.Add(login);
                _db.SaveChanges();
                login.Id = _db.LoginCredentials.Where(x => x.ManaHash.Equals(login.ManaHash)).Select(x => x.Id).FirstOrDefault();
            }
            catch
            {
                return null;
            }

            return login;
        }

        public string GetSalt()
        {
            var digits = 64;
            var random = new Random();
            byte[] buffer = new byte[digits / 2];
            random.NextBytes(buffer);
            string result = string.Concat(buffer.Select(x => x.ToString("X2")).ToArray());
            if (digits % 2 == 0)
                return result;
            return result + random.Next(16).ToString("X");
        }

        public string HashPassword(string salt, string password)
        {
            var saltBytes = Convert.FromBase64String(salt);
            //int nIterations;
            //int nHash;
            var success = int.TryParse(_config["Jwt:nIterations"], out int nIterations);
            if (!success) return string.Empty;
            success = int.TryParse(_config["Jwt:nIterations"], out int nHash);
            if (!success) return string.Empty;
            using var rfc2898DeriveBytes = new Rfc2898DeriveBytes(password, saltBytes, nIterations);
            return Convert.ToBase64String(rfc2898DeriveBytes.GetBytes(nHash));
        }
        #endregion
    }
}
