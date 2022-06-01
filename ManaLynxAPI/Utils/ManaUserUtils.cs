using ManaLynxAPI.Data;
using ManaLynxAPI.Models;

namespace ManaLynxAPI.Utils
{
    /// <summary>
    /// ManaUser Utillitaries Interface
    /// </summary>
    public interface IManaUserUtils
    {
        string? Error { get; }
        ManaUser? Model { get; }
        bool AddCliente(RegisterRequest register);
        bool AddRole(RegisterRoleRequest register);
        bool Update(RegisterRequest user, string newPassword, int userId);
    }
    /// <summary>
    /// ManaUser Utillitaries Implementation
    /// </summary>
    public class ManaUserUtils : IManaUserUtils
    {
        private readonly ApplicationDbContext _db;
        private readonly ILoginCredentialUtils _login;
        private readonly IClienteUtils _cliente;

        public string? Error { get; private set; }
        public ManaUser? Model { get; private set; }

        public ManaUserUtils(ApplicationDbContext db, ILoginCredentialUtils login, IClienteUtils cliente)
        {
            _db = db;
            _login = login;
            _cliente = cliente;
        }

        public bool AddCliente(RegisterRequest register)
        {
            if (string.IsNullOrEmpty(register.Username) || string.IsNullOrEmpty(register.Email) || string.IsNullOrEmpty(register.Password))
            {
                Error = "Request lacks one of the following for a successful route: Username, Email, Password.";
                return false;
            }

            Model = new ManaUser(register);

            if (UserExists(Model))
            {
                Error = "Either Username or Email address is already present in the system.";
                return false;
            }

            var login = _login.GenerateLoginCredentials(register);
            if(login is null)
            {
                Error = "Failed Generating Credentials";
                return false;
            }

            Model.UserRole = Roles.Cliente.ToString();
            _cliente.AddCliente(new() { IsLead = 0 });
            if(_cliente.Model is null && !string.IsNullOrEmpty(_cliente.Error))
            {
                Error = "Pessoa Exception: " + _cliente.Error;
                return false;
            }
            Model.Pessoa = _cliente.Model.Pessoa;
            Model.PessoaId = _cliente.Model!.PessoaId;
            Model.LoginCredential = login.Id;
            _db.ManaUsers.Add(Model);
            if (_db.SaveChanges() == 0)
            {
                Error = "Error Saving Changes.";
                return false;
            }

            Model.LoginCredential = null;
            Model.LoginCredentialNavigation = null;

            Error = null;
            return true;
        }

        public bool AddRole(RegisterRoleRequest register)
        {
            if (string.IsNullOrEmpty(register.Username) || string.IsNullOrEmpty(register.Email)
                || string.IsNullOrEmpty(register.Password) || string.IsNullOrEmpty(register.UserRole))
            {
                Model = null;
                Error = "Request lacks one of the following for a successful route: Username, Email, UserRole, Password.";
                return false;
            }

            Model = new(register);

            if (UserExists(Model))
            {
                Model = null;
                Error = "Either Username or Email address is already present in the system.";
                return false;
            }

            var reg = new RegisterRequest { Username = register.Username, Password = register.Password };

            var login = _login.GenerateLoginCredentials(reg);

            if (login is null)
            {
                Model = null;
                Error = "Failed Generating Credentials";
                return false;
            }

            Model.PessoaId = register.PessoaId;
            Model.LoginCredential = login.Id;

            _db.ManaUsers.Add(Model);
            if (_db.SaveChanges() == 0)
            {
                Model = null;
                Error = "Error Saving Changes.";
                return false;
            }

            Model.Pessoa = null;
            Model.LoginCredential = null;
            Model.LoginCredentialNavigation = null;

            Error = null;
            return true;
        }

        public bool Update(RegisterRequest user, string newPassword, int userId)
        {
            // Check Existance
            Model = _db.ManaUsers.Find(userId);
            if (Model is null)
            {
                Error = "User not present in the system.";
                return false;
            }

            //// Find User
            //User = _db.ManaUsers.Where(u => u.Email.Equals(user.Email)).FirstOrDefault();
            //if (User is null)
            //{
            //    Error = "Failed retrieving User from db";
            //    return false;
            //}

            // Validate Old Password
            var login = _db.LoginCredentials.Find(Model.LoginCredential); if (login is null)
            {
                Error = "Failed Retrieving Credentials";
                return false;
            }
            var oldHash = login!.ManaHash;
            var oldSalt = login!.ManaSalt;
            var hash = _login.HashPassword(oldSalt, user.Password);
            if (!hash.Equals(oldHash))
            {
                Error = "Old Password does not Match.";
                return false;
            }

            // Generate New Password
            var salt = _login.GetSalt();
            hash = _login.HashPassword(salt, newPassword);
            login.ManaSalt = salt;
            login.ManaHash = hash;
            _db.LoginCredentials.Update(login);
            if(_db.SaveChanges() == 0)
            {
                Error = "Error Saving Changes. Credentials.";
                return false;
            }
            

            Model.LoginCredential = login.Id;
            Model.Email = user.Email;
            _db.ManaUsers.Update(Model);

            if (_db.SaveChanges() == 0)
            {
                Error = "Error Saving Changes. User.";
                return false;
            }

            Model.Id = 0;
            Model.LoginCredential = null;
            Model.LoginCredentialNavigation = null;

            Error = null;
            return true;
        }

        /// <summary>
        /// Validates Login Object Information
        /// Checks for Password and Username
        /// </summary>
        /// <param name="logRequest">login information</param>
        /// <returns>returns logged user or null</returns>
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

        private bool UserExists(ManaUser user)
        {
            var usr = _db.ManaUsers.Where(u => u.Email.Equals(user.Username) || u.Username.Equals(user.Username)).FirstOrDefault();
            if (usr is null) return false;
            return true;
        }
    }
}
