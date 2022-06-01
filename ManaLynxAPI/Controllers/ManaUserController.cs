using Microsoft.AspNetCore.Mvc;
using ManaLynxAPI.Data;
using ManaLynxAPI.Models;
using Microsoft.AspNetCore.Authorization;
using System.IdentityModel.Tokens.Jwt;
using Auth = ManaLynxAPI.Authentication.Auth;
using Roles = ManaLynxAPI.Models.Roles;
using ManaLynxAPI.Utils;
using Newtonsoft.Json;


namespace ManaLynxAPI.Controllers
{
    /// <summary>
    /// Controller that manages authentication-related requests
    /// </summary>
    [ApiController, Route("[controller]/[action]")]
    public class ManaUserController : Controller
    {
        private IJWTAuthManager _authManager;
        private ILoginCredentialUtils _login;
        private ApplicationDbContext _db;
        private readonly ILoggerUtils _logger;
        private readonly IAppUtils _app;
        private readonly IManaUserUtils _user;

        /// <summary>
        /// Default Constructor
        /// </summary>
        /// <param name="db">Db Context Transient</param>
        /// <param name="a">JWTAuth Utillitaries Transient</param>
        /// <param name="l">LoginCredential Utillitaries Transient</param>
        /// <param name="logger">Logger Transient</param>
        /// <param name="app">Generic Utillitaries Transient</param>
        /// <param name="user">ManaUser Utillitaries Transient</param>
        public ManaUserController(ApplicationDbContext db, IJWTAuthManager a, ILoginCredentialUtils l, ILoggerUtils logger, IAppUtils app, IManaUserUtils user)
        {
            _db = db;
            _authManager = a;
            _login = l;
            _logger = logger;
            _app = app;
            _user = user;
        }

        /// <summary>
        /// Get All Users on Database
        /// Only accessible by Admins
        /// </summary>
        /// <returns>UserId, Username, UserRole</returns>
        [HttpGet, Auth(Roles.Admin)]
        public IActionResult Index()
        {
            try
            {
                var user = _db.ManaUsers.Select(x => new { x.Id, x.Username, x.UserRole }).ToList();

                if (user is not null)
                {
                    var jwt = Request.Headers.Authorization[0].Replace("Bearer ", "");
                    var handler = new JwtSecurityTokenHandler();
                    var token = handler.ReadJwtToken(jwt);
                    foreach (var claim in token.Claims.ToList())
                    {

                    }
                }

                var token_ = Request.Headers.Authorization[0].Replace("Bearer ", "");
                _logger.SetLogInfoGetAll(_app.GetUserId(token_), "ManaUser");

                return Ok(user);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        #region Authentication

        /// <summary>
        /// Registers Agentes to system
        /// </summary>
        /// <param name="register">New User Information</param>
        /// <returns>Created User or Error</returns>
        [HttpPost, Auth(Roles.Admin, Roles.Gestor)]
        public IActionResult RegisterRole(RegisterRoleRequest register)
        {
            if (_user.AddRole(register) && _user.Error is null)
            {
                return Ok(_user.Model);
            }

            return BadRequest(error: _user.Error);
        }

        /// <summary>
        /// Register route for new Clientes
        /// Creates null Pessoa and DadoClinico entries in DB
        /// </summary>
        /// <param name="register">User Information</param>
        /// <returns>User Created or Error</returns>
        [HttpPost, AllowAnonymous]
        public IActionResult Register(RegisterRequest register)
        {
            if (_user.AddCliente(register))
            {
                return Ok(_user.Model);
            }

            return BadRequest(error:_user.Error);
        }

        /// <summary>
        /// Login route for whoever
        /// </summary>
        /// <param name="login">login information</param>
        /// <returns>Token and TokenLifeTime</returns>
        [HttpPost, AllowAnonymous]
        public IActionResult Login(AuthRequest login)
        {
            var user = _authManager.ValidateUser(login);
            if (user == null)
                return BadRequest(error: "User Not Found.");
            var token = _authManager.GenerateTokenString(user, DateTime.UtcNow);

            return Ok(new AuthResponse
            {
                Name = login.Username,
                Token = token
            });
        }

        #endregion

        /// <summary>
        /// Stores Information Needed in order to Update User
        /// </summary>
        public record UpdateObject
        {
            /// <summary>
            /// Updates User Entrie
            /// </summary>
            public RegisterRequest? User { get; set; } = null;
            /// <summary>
            /// Updates User Password
            /// </summary>
            public string? Password { get; set; } = null;
        }

        /// <summary>
        /// Updates User Information
        /// </summary>
        /// <param name="obj">Update Information</param>
        /// <returns>User Model or Error</returns>
        [HttpPut, Auth]
        public IActionResult Update(UpdateObject obj)
        {
            var token = Request.Headers.Authorization[0].Replace("Bearer ", "");
            var reqId = _app.GetUserId(token);
            if(reqId is null)
            {
                return BadRequest(error: "Invalid Token.");
            }

            if(obj.User is null || obj.Password is null)
            {
                return BadRequest(error: "Invalid Update Object");
            }

            if (_user.Update(obj.User, obj.Password, reqId.Value))
            {
                return Ok(_user.Model);
            }

            var json = JsonConvert.SerializeObject(obj, new JsonSerializerSettings() { MaxDepth = 1, ReferenceLoopHandling = ReferenceLoopHandling.Ignore });
            _logger.SetLogInfoPut(reqId, "ManaUser", json);

            return BadRequest(error:_user.Error);
        }

        /// <summary>
        /// Deletes User from System
        /// </summary>
        /// <param name="id">User Id</param>
        /// <returns>Status or Error</returns>
        [HttpDelete("{id}"), Auth(Roles.Admin)]
        public IActionResult Delete(int id)
        {
            var usr = _db.ManaUsers.Find(id);
            if (usr == null)
            {
                return BadRequest(error: "User not found");
            }

            _db.ManaUsers.Remove(usr);
            if (_db.SaveChanges() == 0)
            {
                return BadRequest(error:"Error Saving Changes!");
            }

            var token = Request.Headers.Authorization[0].Replace("Bearer ", "");
            _logger.SetLogInfoDelete(_app.GetUserId(token), "ManaUser", id);

            return Ok();
        }
    }
}
