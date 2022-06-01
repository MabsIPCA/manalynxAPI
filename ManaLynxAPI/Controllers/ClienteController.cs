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
    /// Cliente Routes Controller.
    /// Everyone Must be Authenticated, Authorization is managed in-route.
    /// Admin is allowed to manipulate all data.
    /// Gestor is allowed to manipulate all team's leads.
    /// Agente is allowed to manipulate all his leads.
    /// Cliente is allowed to manipulate himself.
    /// Everyone is prevented from injections.
    /// </summary>
    [ApiController, Route("[controller]/[action]")]
    public class ClienteController : Controller
    {
        private readonly IPessoaUtils _pessoa;
        private readonly IClienteUtils _cliente;
        private readonly ApplicationDbContext _db;
        private readonly ILoggerUtils _logger;
        private readonly IAppUtils _app;

        /// <summary>
        /// Default Constructor
        /// </summary>
        /// <param name="db">Sets Db Transient</param>
        /// <param name="pessoa">Sets Pessoa Utillitaries Transient</param>
        /// <param name="cliente">Sets Cliente Utillitaries Transient</param>
        /// <param name="logger">Sets Logger Transient</param>
        /// <param name="app">Sets Application Generic Utillitaries Transient</param>
        public ClienteController(ApplicationDbContext db, IPessoaUtils pessoa, IClienteUtils cliente, ILoggerUtils logger, IAppUtils app)
        {
            _db = db;
            _pessoa = pessoa;
            _cliente = cliente;
            _logger = logger;
            _app = app;
        }

        /// <summary>
        /// Get Cliente Information given <paramref name="id"/>.
        /// If <paramref name="id"/> = 0 get self information.
        /// </summary>
        /// <param name="id">Cliente Id</param>
        /// <returns></returns>
        [HttpGet, Route("{id}"), Auth]
        public IActionResult Index(int id = 0)
        {
            IActionResult result = BadRequest(new { error = "Nothing Happened" });

            // Request Info
            var token = Request.Headers.Authorization[0].Replace("Bearer ", string.Empty);
            var reqId = _app.GetUserId(token);
            var role = _app.GetUserRole(token);
            if (reqId is null || role is null) return BadRequest(new { error = "Auth Header Invalid" });
            if (_db.ManaUsers.Find(reqId) is null) return BadRequest();

            int? agenteId;
            Cliente? cliente = new();
            List<Cliente>? clientes = new();

            switch (role.Value)
            {
                case Roles.Admin:
                    {
                        if (id == 0)
                        {
                            clientes = _db.Clientes.ToList();
                            foreach (var c in clientes)
                            {
                                c.Pessoa = _db.Pessoas.Find(c.PessoaId);
                                if (c.Pessoa is null) return BadRequest(error: "Cliente Object Is Not Consistent, Lacks Pessoa.");
                                _cliente.ValidateModel(c);
                                _pessoa.ValidateModel(c.Pessoa);
                            }
                            result = Ok(clientes);
                        }
                        else
                            result = IndexById(id);
                        break;
                    }

                case Roles.Gestor:
                    result = GestorIndex(reqId.Value); break;

                case Roles.Agente:
                    {
                        if (id != 0)
                        {
                            result = IndexById(id); break;
                        }
                        agenteId = _app.GetAgenteId(reqId);
                        if (agenteId is null) return Ok(new { error = "No Agente Found" });
                        var agente = _db.Agentes.Find(agenteId)!;
                        clientes = _db.Clientes.Where(c => c.AgenteId == agente.Id).ToList();
                        if (clientes is null) return Ok(new { error = "Agente has no Clientes" });
                        foreach(var c in clientes)
                        {
                            c.Pessoa = _db.Pessoas.Find(c.PessoaId);
                            if (c.Pessoa is null) return BadRequest(error: "Cliente Object Is Not Consistent, Lacks Pessoa.");
                            _cliente.ValidateModel(c);
                            _pessoa.ValidateModel(c.Pessoa);
                        }
                        result = Ok(clientes); break;
                    }

                case Roles.Cliente:
                        var pessoa = _db.ManaUsers.Where(u => u.Id == reqId).Select(u => u.Pessoa).FirstOrDefault();
                        if (pessoa is null) return Ok(new { error = "Failed to find Pessoa." });
                        cliente = _db.Clientes.Where(c => c.PessoaId == pessoa.Id).FirstOrDefault();
                        if (cliente is null) return Ok(new { error = "Failed to find Cliente." });
                        _cliente.ValidateModel(cliente);
                        clientes.Add(cliente);
                        _pessoa.ValidateModel(cliente.Pessoa);
                        result = Ok(new[] { cliente }); break;
            }

            // Logging
            if(id == 0 && role != Roles.Cliente)
                _logger.SetLogInfoGetAll(reqId, "Cliente");
            else
                _logger.SetLogInfoGet(reqId, "Cliente", id);

            return result;
        }

        #region Index Specifics
        private IActionResult IndexById(int id)
        {
            List<Cliente>? clientes = new();
            var cliente = _db.Clientes.Find(id);
            if (cliente is null) return Ok(new { error = $"Cliente of Id {id} does not exist." });
            cliente.Pessoa = _db.Pessoas.Find(cliente.PessoaId);
            if (cliente.Pessoa is null) return BadRequest(error: "Cliente Object Is Not Consistent, Lacks Pessoa.");
            _cliente.ValidateModel(cliente);
            _pessoa.ValidateModel(cliente.Pessoa);
            clientes.Add(cliente);
            return Ok(clientes);
        }

        private IActionResult GestorIndex(int reqId)
        {
            List<object> all = new();
            bool skip = true;

            var equipaId = _app.GetEquipaId(reqId);
            if (equipaId is null) return Ok(new { error = "Equipa not found" });
            var agentes = _app.GetEquipaAgentes(equipaId);
            if (agentes is null) return Ok(new { error = "Equipa is Empty" });
            foreach (var agenteId in agentes)
            {
                var clientes = (from a in _db.Agentes
                                where a.Id == agenteId
                                select a.Clientes).FirstOrDefault()!.ToList();
                if (clientes is null || clientes.Count == 0) continue;
                var pessoa = _db.Pessoas.Find(clientes[0].PessoaId);
                if (pessoa is null) return BadRequest(error: "Cliente Object Is Not Consistent, Lacks Pessoa.");
                var obj = new { nome = pessoa.Nome, cliente = clientes[0] };
                var lst = new[] { obj };
                foreach (var cliente in clientes)
                {
                    if (skip)
                    {
                        skip = false; continue;
                    }
                    _cliente.ValidateModel(cliente);
                    obj = new { nome = pessoa.Nome, cliente = cliente };
                    lst.Append(obj);
                    lst.GetType();
                }
                skip = true;
                all.Add(lst);
            }

            return Ok(all);
        }
        #endregion

        /// <summary>
        /// Adds Cliente Entry to Db.
        /// Accepts Pessoa Injection Only!
        /// </summary>
        /// <param name="cliente">Information About Cliente.</param>
        /// <returns>Response body with added Information.</returns>
        [HttpPost, Auth]
        public IActionResult Create(Cliente cliente)
        {
            if(!ModelState.IsValid) return BadRequest(ModelState);
            var token = Request.Headers.Authorization[0].Replace("Bearer ", string.Empty);
            var reqId = _app.GetUserId(token);
            var role = _app.GetUserRole(token);
            if (reqId is null || role is null) return BadRequest(new { error = "Auth Header Invalid" });
            if (_db.ManaUsers.Find(reqId) is null) return BadRequest();


            if (cliente.Pessoa is null) return BadRequest(new { error = "Cliente creation asks for Pessoa Information." });

            if(role == Roles.Admin)
            {
                if (_cliente.AddCliente(cliente))
                {
                    cliente = _cliente.Model!;
                }
                else
                {
                    return BadRequest(error: _cliente.Error);
                }
            }

            _cliente.ValidateModel(cliente);

            switch (role)
            {
                case Roles.Gestor:
                case Roles.Agente:
                    // Find User Data
                    var agenteId = _app.GetAgenteId(reqId.Value);
                    if (agenteId is null) return BadRequest();
                    cliente.AgenteId = agenteId;
                    if (_cliente.AddCliente(cliente))
                    {
                        cliente = _cliente.Model!;
                    }
                    else
                    {
                        return BadRequest(error: _cliente.Error);
                    }
                    break;

                case Roles.Cliente:
                    cliente.IsLead = 0;
                    if (_cliente.AddCliente(cliente))
                    {
                        cliente = _cliente.Model!;
                    }
                    else
                    {
                        return BadRequest(error: _cliente.Error);
                    }
                    break;
            }

            var json = JsonConvert.SerializeObject(cliente, new JsonSerializerSettings() { MaxDepth = 1, ReferenceLoopHandling = ReferenceLoopHandling.Ignore });
            _logger.SetLogInfoPost(reqId, "Cliente", json);

            return Ok(cliente);
        }

        /// <summary>
        /// Updates Cliente Entry in Db.
        /// Does not Accept Pessoa Injection!
        /// </summary>
        /// <param name="cliente">Cliente Information Only.</param>
        /// <returns>Response Body With Updated Information.</returns>
        [HttpPost, Auth]
        public IActionResult Update(Cliente cliente)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            var token = Request.Headers.Authorization[0].Replace("Bearer ", string.Empty);
            var reqId = _app.GetUserId(token);
            var role = _app.GetUserRole(token);
            if (reqId is null || role is null) return BadRequest(new { error = "Auth Header Invalid" });
            if (_db.ManaUsers.Find(reqId) is null) return BadRequest();

            _cliente.ValidateModel(cliente);

            Cliente? clienteDB;

            switch (role)
            {
                case Roles.Admin:
                    _cliente.UpdateCliente(cliente); break;

                case Roles.Gestor:
                    var equipaId = _app.GetEquipaId(reqId);
                    if (equipaId is null) return BadRequest(error: "There is no Equipa. Contact Support.");
                    var agentes = _app.GetEquipaAgentes(equipaId);
                    if (agentes is null) return BadRequest(error: "There is no Agentes in Equipa!");
                    foreach(var id in agentes)
                    {
                        var agente = _db.Agentes.Find(id);
                        if(agente is null) continue;
                        foreach(var c in agente.Clientes)
                        {
                            if(c is null) continue;
                            if(c.AgenteId == id)
                            {
                                _cliente.UpdateCliente(cliente);
                                goto result;
                            }
                        }
                    }
                    break;

                case Roles.Agente:
                    // Find user data
                    var agenteId = _app.GetAgenteId(reqId);
                    if (agenteId is null) return Ok(new { error = "You are not registered, this incident will be reported." });
                    // Check entry existance
                    clienteDB = _db.Clientes.Where(c => c.Id == cliente.Id && c.AgenteId == agenteId).FirstOrDefault();
                    if(clienteDB is null) return BadRequest(error: "This Cliente does not exist or it is not a Lead of yours.");
                    // Check data
                    _cliente.UpdateCliente(cliente);
                    break;

                case Roles.Cliente:
                    var user = _db.ManaUsers.Find(reqId);
                    if (user is null) return BadRequest();
                    cliente.PessoaId = user.PessoaId;
                    clienteDB = _db.Clientes.Find(cliente.Id);
                    if (clienteDB is null) return BadRequest("The `cliente` does not exist!");
                    // Check data
                    _cliente.UpdateCliente(cliente);
                    break;
            }

            result:
            var json = JsonConvert.SerializeObject(cliente, new JsonSerializerSettings() { MaxDepth = 1, ReferenceLoopHandling = ReferenceLoopHandling.Ignore });
            _logger.SetLogInfoPut(_app.GetUserId(token), "Cliente", json);

            if(_cliente.Error is not null)
            {
                return BadRequest(error: _cliente.Error);
            }

            return Ok(_cliente.Model);
        }

        /// <summary>
        /// Delete Cliente Entry in Db.
        /// Deletes Recursively.
        /// </summary>
        /// <param name="clienteId">Cliente Id</param>
        /// <returns>Success Status.</returns>
        [HttpPost, Auth]
        public IActionResult Delete(int clienteId)
        {
            IActionResult result = BadRequest();
            if (!ModelState.IsValid) return BadRequest(ModelState);
            var token = Request.Headers.Authorization[0].Replace("Bearer ", string.Empty);
            var reqId = _app.GetUserId(token);
            var role = _app.GetUserRole(token);
            if (reqId is null || role is null) return BadRequest(new { error = "Auth Header Invalid" });

            Cliente? cliente = null;

            switch (role)
            {
                case Roles.Admin:
                    cliente = _db.Clientes.Find(clienteId);
                    if (cliente is null) return BadRequest(error: "No Cliente Found");
                    break;

                case Roles.Gestor:
                    var equipaId = _app.GetEquipaId(reqId);
                    if (equipaId is null) return BadRequest(error: "There is no Equipa. Contact Support.");
                    var agentes = _app.GetEquipaAgentes(equipaId);
                    if (agentes is null) return BadRequest(error: "There is no Agentes in Equipa!");
                    foreach (var id in agentes)
                    {
                        var agente = _db.Agentes.Find(id);
                        if (agente is null) continue;
                        foreach (var c in agente.Clientes)
                        {
                            if (c is null) continue;
                            if (c.Id == id)
                            {
                                if (!DeleteCliente(c)) return BadRequest(error: "Deletion of Pessoa Failed. Contact Support.");
                                goto result;
                            }
                        }
                    }
                    break;

                case Roles.Agente:
                    var agenteId = _app.GetAgenteId(reqId);
                    if (agenteId is null) return BadRequest(error: "Agente Not Found. Contact Support.");
                    cliente = _db.Clientes.Where(c => c.Id == clienteId && c.AgenteId == agenteId).FirstOrDefault();
                    if (cliente is null) return BadRequest(error: "No Cliente Found.Contact Support.");
                    if (!DeleteCliente(cliente)) return BadRequest(error: "Deletion of Pessoa Failed. Contact Support.");
                    break;

                case Roles.Cliente:
                    var user = _db.ManaUsers.Find(reqId);
                    if (user is null) return BadRequest(error: "User does not Exist. Contact Support.");
                    cliente = _db.Clientes.Find(user.PessoaId);
                    if (cliente is null) return BadRequest(error: "No Cliente Found. Contact Support.");
                    if (!DeleteCliente(cliente)) return BadRequest(error: "Deletion of Pessoa Failed. Contact Support.");
                    break;
            }

            result:
            if (cliente is null) return BadRequest(error: "Something went wrong.");
            cliente.Pessoa = null;
            _db.Clientes.Update(cliente);
            if(_db.SaveChanges() == 0) return BadRequest(error: "Error Saving Changes");

            _logger.SetLogInfoDelete(reqId, "Clientes", clienteId);

            return Ok(new { success = true });
        }

        #region Utils

        private bool DeleteCliente(Cliente cliente)
        {
            cliente.Profissao = null;
            cliente.ProfissaoRisco = null;
            return _pessoa.RemovePessoa(cliente.Pessoa);
        }
        #endregion
    }
}
