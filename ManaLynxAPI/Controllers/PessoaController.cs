using Microsoft.AspNetCore.Mvc;
using ManaLynxAPI.Data;
using ManaLynxAPI.Utils;
using ManaLynxAPI.Models;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using System.IdentityModel.Tokens.Jwt;
using Auth = ManaLynxAPI.Authentication.Auth;
using Roles = ManaLynxAPI.Models.Roles;
using Newtonsoft.Json;

namespace ManaLynxAPI.Controllers
{
    /// <summary>
    /// Pessoa CRUD Routes
    /// </summary>
    [ApiController, Route("[controller]/[action]")]
    public class PessoaController : Controller
    {
        private readonly IPessoaUtils _pessoa;
        private readonly IJWTAuthManager _auth;
        private readonly ApplicationDbContext _db;
        private readonly ILoggerUtils _logger;
        private readonly IAppUtils _app;

        /// <summary>
        /// Default Constructor
        /// </summary>
        /// <param name="db">Database transient</param>
        /// <param name="auth">Authent Utillitaries Transient</param>
        /// <param name="pessoa">Pessoa Utillitaries Transient</param>
        /// <param name="logger">Logger Transient</param>
        /// <param name="app">Generic Utillitaries Transient</param>
        public PessoaController(ApplicationDbContext db, IJWTAuthManager auth, IPessoaUtils pessoa, ILoggerUtils logger, IAppUtils app)
        {
            _db = db;
            _auth = auth;
            _pessoa = pessoa;
            _logger = logger;
            _app = app;
        }


        /// <summary>
        /// Returns the name of the user accessing this route
        /// it verifies the id present in the token
        /// </summary>
        /// <returns></returns>
        [HttpGet, Auth]
        public IActionResult GetPessoaNameByToken()
        {
            var token = Request.Headers.Authorization[0].Replace("Bearer ", "");
            (var reqId, var role) = _auth.GetClaims(token);

            var userName = (from manaUser in _db.ManaUsers
                            join pessoa in _db.Pessoas on manaUser.PessoaId equals pessoa.Id
                            where manaUser.Id == reqId
                            select pessoa.Nome);

            if (userName != null) return Ok(userName); else return BadRequest("Invalid User");
        }


        /// <summary>
        /// Gets Pessoa Information
        /// Everyone accesses their information
        /// Admin has access to all
        /// Gestor access inside its equipa
        /// Agente accesses his clientes information
        /// Cliente only accesses his information
        /// </summary>
        /// <param name="id">Pessoa Information</param>
        /// <returns>Array of Pessoas or Error</returns>
        [HttpGet, Route("{id?}"), Auth]
        public IActionResult Index(int id = 0)
        {
            var token = Request.Headers.Authorization[0].Replace("Bearer ", "");
            var reqId = _app.GetUserId(token);
            var role = _app.GetUserRole(token);
            if (id != 0) return IndexById(id);

            IActionResult result;
            List<Pessoa> pessoas = new();
            Pessoa? pessoa = new();

            switch (role)
            {
                case Roles.Admin:
                    pessoas = _db.Pessoas.ToList();
                    foreach (var p in pessoas)
                    {
                        _pessoa.ValidateModel(p);
                    }
                    if (pessoas is null) return BadRequest();
                    break;

                case Roles.Gestor:
                case Roles.Agente:
                case Roles.Cliente:
                    pessoa = _db.ManaUsers.Where(u => u.Id == reqId).Select(u => u.Pessoa).FirstOrDefault();
                    _pessoa.ValidateModel(pessoa);
                    break;
            }

            if (pessoas.Count == 0 && pessoa is not null)
                pessoas.Add(pessoa);

            if (pessoas.Count > 1)
                _logger.SetLogInfoGetAll(reqId, "Pessoa");
            else
                _logger.SetLogInfoGet(reqId, "Pessoa", id);


            return Ok(pessoas.ToArray());
        }

        private IActionResult IndexById(int id)
        {
            var pessoa = _db.Pessoas.Find(id);
            if (pessoa == null) return NotFound();

            var token = Request.Headers.Authorization[0].Replace("Bearer ", "");
            _logger.SetLogInfoGet(_app.GetUserId(token), "Pessoa", id);

            return Ok(pessoa);
        }

        /// <summary>
        /// Creates Object pessoa
        /// </summary>
        /// <param name="pessoa"></param>
        /// <returns></returns>
        [HttpPost, Auth]
        public IActionResult Add(Pessoa pessoa)
        {
            IActionResult result = BadRequest();
            if (!ModelState.IsValid) return BadRequest();
            var token = Request.Headers.Authorization[0].Replace("Bearer ", "");
            (var reqId, var role) = _auth.GetClaims(token);
            var user = _db.ManaUsers.Find(reqId);
            if (user is null) return BadRequest();
            Pessoa? p = pessoa;
            if (_pessoa.PessoaExists(p)) return BadRequest();

            switch(role)
            {
                case Roles.Admin:
                    if (_pessoa.AddPessoa(p))
                        p = _pessoa.Model;
                    else
                        return BadRequest(error: _pessoa.Error);
                    if (p is not null && p.Id != 0)
                        result = Ok(); 
                    break;

                case Roles.Gestor:
                case Roles.Agente:
                    var agente = _db.Agentes.Where(a => a.PessoaId == user.PessoaId).FirstOrDefault();
                    if (agente is null) return BadRequest("No Object `Agente`");
                    var cliente = agente.Clientes.Where(c => c.Id == p.Clientes.First().Id).FirstOrDefault();
                    // Not Found -- Is Not Lead (has user account) -- Has `Pessoa` object
                    if (cliente is null || cliente.IsLead == 0 || cliente.PessoaId is not null) return BadRequest("Error Inserting");
                    if (_pessoa.AddPessoa(p))
                        p = _pessoa.Model;
                    else
                        return BadRequest(error: _pessoa.Error);
                    if (p is null) return BadRequest("Error Inserting");
                    cliente.PessoaId = p.Id;
                    result = Ok(); break;

                case Roles.Cliente:
                    if (_pessoa.AddPessoa(p))
                        p = _pessoa.Model;
                    else
                        return BadRequest(error: _pessoa.Error);
                    if (p is null) return BadRequest("Error Inserting");
                    user.PessoaId = p.Id;
                    result = Ok(); break;
            }

            var json = JsonConvert.SerializeObject(pessoa, new JsonSerializerSettings() { MaxDepth = 1, ReferenceLoopHandling = ReferenceLoopHandling.Ignore });
            _logger.SetLogInfoPost(_app.GetUserId(token), "Pessoa", json);

            return result;
        }

        /// <summary>
        /// Updates Object Pessoa
        /// </summary>
        /// <param name="pessoa"></param>
        /// <returns></returns>
        [HttpPut, Auth]
        public IActionResult Update(Pessoa pessoa)
        {
            IActionResult result = BadRequest();
            var token = Request.Headers.Authorization[0].Replace("Bearer ", "");
            (var reqId, var role) = _auth.GetClaims(token);
            if (!ModelState.IsValid) return BadRequest();
            Pessoa? p = pessoa;
            var user = _db.ManaUsers.Find(reqId);
            if(user is null) return BadRequest();

            switch (role)
            {
                case Roles.Admin:
                    if (!_pessoa.PessoaExists(p)) return NotFound();
                    _db.Pessoas.Update(p);
                    result = Ok(); break;

                case Roles.Gestor:
                case Roles.Agente:
                    var agente = _db.Agentes.Where(a => a.PessoaId == user.PessoaId).FirstOrDefault();
                    if (agente is null) return BadRequest();
                    var cliente = agente.Clientes.Where(c => c.Id == p.Clientes.First().Id).FirstOrDefault();
                    if (cliente is null || cliente.PessoaId is null) return BadRequest();
                    p.Id = cliente.PessoaId.Value;
                    _db.Pessoas.Update(p);
                    result = Ok();
                    break;

                case Roles.Cliente:
                    if (user is null) return BadRequest();
                    if (user.PessoaId is null) return BadRequest("Object Pessoa not implemented");
                    if (!_pessoa.PessoaExists(p)) return BadRequest();
                    p.Id = user.PessoaId.Value;
                    _db.Pessoas.Update(p);
                    result = Ok(); break;
            }
            _db.SaveChanges();

            var json = JsonConvert.SerializeObject(pessoa, new JsonSerializerSettings() { MaxDepth = 1, ReferenceLoopHandling = ReferenceLoopHandling.Ignore });
            _logger.SetLogInfoPut(_app.GetUserId(token), "Pessoa", json);

            return result;
        }

        /// <summary>
        /// Deletes Pessoa
        /// </summary>
        /// <param name="pessoa"></param>
        /// <returns></returns>
        [HttpDelete, Auth]
        public IActionResult Delete(Pessoa pessoa)
        {
            IActionResult result = BadRequest();
            var token = Request.Headers.Authorization[0].Replace("Bearer ", "");
            (var reqId, var role) = _auth.GetClaims(token);
            var user = _db.ManaUsers.Find(reqId);
            if (user is null) return BadRequest();
            Pessoa? p = pessoa;
            if (!_pessoa.PessoaExists(p)) return BadRequest();

            switch (role)
            {
                case Roles.Admin:
                    _db.Remove(pessoa);
                    result = Ok(); break;

                case Roles.Gestor:
                case Roles.Agente:
                case Roles.Cliente:
                    if (user.PessoaId is null) return BadRequest();
                    pessoa.Id = user.PessoaId.Value;
                    _db.Remove(pessoa);
                    result = Ok(); break;
            }

            _db.SaveChanges();
            _logger.SetLogInfoDelete(_app.GetUserId(token), "Pessoa", pessoa.Id);

            return result;
        }
    }
}
