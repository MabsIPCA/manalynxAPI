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
    /// Contacto Routes Controller.
    /// Everyone Must be Authenticated, Authorization is managed in-route.
    /// Admin is allowed to manipulate all data.
    /// Gestor is allowed to manipulate all team's leads.
    /// Agente is allowed to manipulate all his leads.
    /// Cliente is allowed to manipulate himself.
    /// Everyone is prevented from injections.
    /// </summary>
    [ApiController, Auth, Route("[controller]/[action]")]
    public class ContactoController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly ILoggerUtils _logger;
        private readonly IAppUtils _app;

        /// <summary>
        /// Default Constructor for ContactoController
        /// </summary>
        /// <param name="db">Sets DbContext</param>
        /// <param name="logger">Sets logger with its Transient</param>
        /// <param name="app">Sets app generic utillitaries with its Transient</param>
        public ContactoController(ApplicationDbContext db, ILoggerUtils logger, IAppUtils app)
        {
            _db = db;
            _logger = logger;
            _app = app;
        }

        /// <summary>
        /// Gets Contacto Entries.
        /// If id != 0 returns only the related set of Contactos.
        /// </summary>
        /// <param name="id">Pessoa Id</param>
        /// <returns></returns>
        [HttpGet, Route("{id}"), Auth]
        public IActionResult Index(int id = 0)
        {
            IActionResult result = NoContent();
            var token = Request.Headers.Authorization[0].Replace("Bearer ", "");
            var reqId = _app.GetUserId(token);
            var roleToken = _app.GetUserRole(token);
            if (reqId is null || roleToken is null) return BadRequest();

            List<Contacto> contactos = new();

            switch (roleToken)
            {
                case Roles.Admin:
                    if (id != 0) return IndexById(id);
                    var pessoaId = _db.ManaUsers.Where(u => u.Id == reqId).Select(u => u.PessoaId).FirstOrDefault();
                    if (pessoaId is null) return BadRequest(error: "pessoa not found");
                    contactos = _db.Contactos.Where(c => c.PessoaId == pessoaId).ToList();
                    if (contactos is null) result = BadRequest();
                    _logger.SetLogInfoGetAll(reqId, "Contacto");
                    result = Ok(contactos);
                    break;

                case Roles.Gestor:
                    if(id == 0)
                    {
                        result = IndexById(reqId.Value); break;
                    }
                    result = GestorIndex(reqId.Value, id); break;

                case Roles.Agente:
                    if (id == 0)
                    {
                        result = IndexById(reqId.Value); break;
                    }
                    result = AgenteIndex(reqId.Value, id); break;

                case Roles.Cliente:
                    result = IndexById(reqId.Value); break;

                default: break;
            }

            return result;
        }

        #region Index Specifics
        private IActionResult IndexById(int id)
        {
            var contactos = _db.Contactos.Where(p => p.PessoaId == id).ToList();
            if (contactos == null) return NotFound();

            var token = Request.Headers.Authorization[0].Replace("Bearer ", "");
            _logger.SetLogInfoGet(_app.GetUserId(token), "Contacto", id);

            return Ok(contactos.ToArray());
        }

        private IActionResult AgenteIndex(int reqId, int id)
        {
            var agenteId = _app.GetAgenteId(reqId);

            var pessoaId = _db.Clientes.Where(c => c.AgenteId == agenteId && c.IsLead == 1 && c.PessoaId == id)
                                      .Select(c => c.PessoaId).FirstOrDefault();
            if(pessoaId is null) return Ok(new {error = "The Cliente\\Pessoa was not found" });
            var contactoIds = _db.Contactos.Where(c => c.PessoaId == pessoaId).Select(c => c.Id).ToList();

            var contact = _db.Contactos.Where(c => c.Id == contactoIds[0]).Select(c => new
            {
                c.PessoaId,
                c.Tipo,
                c.Valor
            });
            var lst = new[] { contact }.ToList();
            for (int x = 1; x < contactoIds.Count; x++)
            {
                contact = _db.Contactos.Where(c => c.Id == contactoIds[x]).Select(c => new
                {
                    c.PessoaId,
                    c.Tipo,
                    c.Valor
                });
                lst.Add(contact);
            }

            var token = Request.Headers.Authorization[0].Replace("Bearer ", "");
            _logger.SetLogInfoGet(reqId, "Contacto", id);

            return Ok(lst);
        }

        /// <summary>
        /// Finds `Lead` with <paramref name="id"/>
        /// Related to Gestor through Equipa of Agentes
        /// </summary>
        /// <param name="reqId"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        private IActionResult GestorIndex(int reqId, int id)
        {
            List<int> contactoIds = new();
            // Find Equipa
            var equipaId = _app.GetEquipaId(reqId);
            if (equipaId is null) return Ok(new
            {
                error = "No Equipa was found."
            });
            // Find Agentes in Equipa
            var agentes = _app.GetEquipaAgentes(equipaId);
            if (agentes is null) return Ok(new
            {
                error = "Your Equipa is empty."
            });
            // Loop through Agentes in Equipa
            foreach(var agente in agentes)
            {
                var clientes = (from a in _db.Agentes
                         where a.Id == agente
                         select a.Clientes).FirstOrDefault();
                if (clientes is null) continue;

                // Loop through Clientes of Agente
                foreach(var cliente in clientes)
                {
                    // Find Requested Lead and Collect Contacto Ids
                    if(cliente.IsLead == 1 && cliente.PessoaId == id)
                    {
                        var x = (from contacto in _db.Contactos
                                 where contacto.PessoaId == cliente.PessoaId
                                 select contacto.Id).FirstOrDefault();
                        contactoIds.Add(x);
                        goto result;
                    }
                }
            }

            // Add Contacto Information
            result:
            var contact = _db.Contactos.Where(c => c.Id == contactoIds[0]).Select(c => new
            {
                c.PessoaId,
                c.Tipo,
                c.Valor
            });
            var lst = new[] { contact }.ToList();
            for (int x = 1; x < contactoIds.Count(); x++)
            {
                contact = _db.Contactos.Where(c => c.Id == contactoIds[x]).Select(c => new
                {
                    c.PessoaId,
                    c.Tipo,
                    c.Valor
                });
                lst.Add(contact);
            }

            _logger.SetLogInfoGet(reqId, "Contacto", id);

            return Ok(lst);
        }
        #endregion

        /// <summary>
        /// Add DB Entry Contacto related to a pessoa.
        /// </summary>
        /// <param name="contacto"></param>
        /// <returns>The added object information.</returns>
        [HttpPost, Auth]
        public IActionResult Create(Contacto contacto)
        {
            // Validate ModelState and Expected Contacto state
            if (!ModelState.IsValid && !ContactoValid(contacto)) return BadRequest(new { error = "Bad request body!" });

            // Request Info
            var token = Request.Headers.Authorization[0].Replace("Bearer ", "");
            var reqId = _app.GetUserId(token);
            var roleToken = _app.GetUserRole(token);
            if (reqId is null || roleToken is null) return BadRequest(new { error = "Invalid Auth Token Information!" });

            Pessoa? pessoa;

            switch (roleToken)
            {
                case Roles.Admin:
                    if (_db.Pessoas.Find(contacto.PessoaId) is null) return Ok(new { error = "No Pessoa with given Id." });
                    break;

                case Roles.Gestor:
                    // Find Equipa
                    var equipaId = _app.GetEquipaId(reqId);
                    if (equipaId is null) return Ok(new { error = "Failed to find Equipa." });
                    // Find Equipa Agentes
                    var agentes = _app.GetEquipaAgentes(equipaId);
                    if (agentes is null) return Ok(new { error = "Equipa is empty." });

                    foreach (var id in agentes)
                    {
                        var agente = _db.Agentes.Find(id);
                        if(agente is null) continue;
                        foreach(var cliente in agente.Clientes)
                        {
                            if (cliente is null || cliente.IsLead == 0) continue;
                            if (cliente.PessoaId == contacto.PessoaId)
                            {
                                if (cliente.Pessoa is null) 
                                    return Ok(new { error = "Cliente Model Invalid: Lacks Pessoa." });
                                goto submit;
                            }
                        }
                    }
                    return Ok(new { error = "There is no Cliente you can update." });

                case Roles.Agente:
                    var agenteId = _app.GetAgenteId(reqId);
                    pessoa = _db.Clientes.Where(c => c.AgenteId == agenteId && c.IsLead == 1 && c.PessoaId == contacto.PessoaId)
                                    .Select(c => c.Pessoa).FirstOrDefault();
                    if (pessoa is null) return Ok(new { error = "The Cliente\\Pessoa was not found" });
                    break;

                case Roles.Cliente:
                    pessoa = _db.ManaUsers.Where(u => u.Id == reqId).Select(u => u.Pessoa).FirstOrDefault();
                    if (pessoa is null) return BadRequest(new { error = "Pessoa is null" });
                    if (_db.Clientes.Where(c => c.IsLead == 1 && c.PessoaId == pessoa.Id).FirstOrDefault() is not null) 
                        return Ok(new { error = "Can't Request as Lead. Ask Support." });
                    contacto.PessoaId = pessoa.Id;
                    break;
            }

            // Label used to immediatly exit Gestor loop
            submit: 
            // Save Changes
            contacto.Pessoa = null;
            contacto.Id = 0;
            _db.Contactos.Add(contacto);
            if (_db.SaveChanges() == 0) return BadRequest(new { error = "Error Saving Changes!" });
            
            // Logging
            var json = JsonConvert.SerializeObject(contacto, new JsonSerializerSettings() { MaxDepth = 1, ReferenceLoopHandling = ReferenceLoopHandling.Ignore });
            _logger.SetLogInfoPost(reqId, "Contacto", json);

            // Request Body Info
            contacto.Pessoa = null;
            if (roleToken != Roles.Admin)
                contacto.Id = 0;
            return Ok(new
            {
                message = "Contacto Added Successfully.",
                contacto,
            });
        }
        
        /// <summary>
        /// Update Entry on DB
        /// </summary>
        /// <param name="contacto"></param>
        /// <returns>Response body with updated <paramref name="contacto"/> updated values.</returns>
        [HttpPost, Auth]
        public IActionResult Update(Contacto contacto)
        {
            IActionResult result = BadRequest(new { error = "Nothing Happened!" });
            // Validate ModelState and Expected Contacto state
            if (!ModelState.IsValid && !ContactoValid(contacto)) return BadRequest(new { error = "Bad request body!" });
            if (contacto.Id == 0 || _db.Contactos.Find(contacto.Id) is null) return BadRequest(new { error = "Contacto does not exist." });

            // Get Token Claims
            var token = Request.Headers.Authorization[0].Replace("Bearer ", "");
            var reqId = _app.GetUserId(token);
            var roleToken = _app.GetUserRole(token);
            if (reqId is null || roleToken is null) return BadRequest(new { error = "Bad auth header information." });

            // Find Pessoa
            Pessoa? pessoa;

            switch (roleToken)
            {
                case Roles.Admin:
                    if (_db.Pessoas.Find(contacto.PessoaId) is null) return Ok(new { error = "No Pessoa with given Id." });
                    break;

                case Roles.Gestor:
                    // Find Equipa
                    var equipaId = _app.GetEquipaId(reqId);
                    if (equipaId is null) return Ok(new { error = "Failed to find Equipa." });
                    // Find Equipa Agentes
                    var agentes = _app.GetEquipaAgentes(equipaId);
                    if (agentes is null) return Ok(new { error = "Equipa is empty." });

                    // Loop through Equipa -> Agentes
                    foreach (var id in agentes)
                    {
                        var agente = _db.Agentes.Find(id);
                        if (agente is null) continue;
                        // Loop through Agente -> Clientes
                        foreach (var cliente in agente.Clientes)
                        {
                            // Find Cliente
                            // Can't be Lead
                            if (cliente is null || cliente.IsLead == 0) continue;
                            if (cliente.PessoaId == contacto.PessoaId)
                            {
                                if (cliente.Pessoa is null) 
                                    return Ok(new { error = "Cliente Model Invalid: Lacks Pessoa." });
                                goto submit;
                            }
                        }
                    }
                    return Ok(new { error = "There is no Cliente you can update." });

                case Roles.Agente:
                    var agenteId = _app.GetAgenteId(reqId);
                    pessoa = _db.Clientes.Where(c => c.AgenteId == agenteId && c.IsLead == 1 && c.PessoaId == contacto.PessoaId)
                                    .Select(c => c.Pessoa).FirstOrDefault();
                    if (pessoa is null) return Ok(new { error = "The Cliente\\Pessoa was not found" });
                    break;

                case Roles.Cliente:
                    pessoa = _db.ManaUsers.Where(u => u.Id == reqId).Select(u => u.Pessoa).FirstOrDefault();
                    if (pessoa is null) return BadRequest(new { error = "Pessoa not found. Contact Support." });
                    if (_db.Clientes.Where(c => c.IsLead == 1 && c.PessoaId == pessoa.Id).FirstOrDefault() is not null)
                        return Ok(new { error = "Can't Request as Lead. Ask Support." });
                    contacto.PessoaId = pessoa.Id;
                    break;
            }

        // Label used to immediatly exit Gestor loop
        submit:
            // Save Changes
            var contactoDB = _db.Contactos.Find(contacto.Id);
            if (contactoDB is null) return BadRequest(error: "contacto does not exist");
            contactoDB.Tipo = contacto.Tipo;
            contactoDB.Valor = contacto.Valor;
            _db.Contactos.Update(contactoDB);
            if (_db.SaveChanges() == 0) return Ok(new { error = "Error Saving Changes!" });

            // Logging
            var json = JsonConvert.SerializeObject(contacto, new JsonSerializerSettings() { MaxDepth = 1, ReferenceLoopHandling = ReferenceLoopHandling.Ignore });
            _logger.SetLogInfoPut(reqId, "Contacto", json);

            // Request Body Info
            contacto.Pessoa = null;
            if (roleToken != Roles.Admin)
                contacto.Id = 0;
            return Ok(new
            {
                message = "Contacto Updated Successfully.",
                contacto,
            });
        }

        /// <summary>
        /// Delete Contacto with Given <paramref name="contactoId"/>.
        /// </summary>
        /// <param name="contactoId"></param>
        /// <returns>Body with result information.</returns>
        [HttpDelete, Route("{contactoId}"), Auth]
        public IActionResult Delete(int contactoId)
        {
            // Validate ModelState and Expected Contacto state
            if (!ModelState.IsValid) return BadRequest(new { error = "Bad request body!" });

            // Request Info
            var token = Request.Headers.Authorization[0].Replace("Bearer ", "");
            var reqId = _app.GetUserId(token);
            var roleToken = _app.GetUserRole(token);
            if (reqId is null || roleToken is null) return BadRequest(new { error = "Invalid Auth Token Information!" });

            Contacto? toDelete = null;
            Pessoa? pessoa;

            switch (roleToken)
            {
                case Roles.Admin:
                    toDelete = _db.Contactos.Find(contactoId);
                    if (toDelete == null) return Ok(new { error = "Contacto not found!" });
                    break;

                case Roles.Gestor:
                    // Find Equipa
                    var equipaId = _app.GetEquipaId(reqId);
                    if (equipaId is null) return Ok(new { error = "Failed to find Equipa." });
                    // Find Equipa Agentes
                    var agentes = _app.GetEquipaAgentes(equipaId);
                    if (agentes is null) return Ok(new { error = "Equipa is empty." });

                    foreach (var id in agentes)
                    {
                        var agente = _db.Agentes.Find(id);
                        if (agente is null) continue;
                        foreach (var cliente in agente.Clientes)
                        {
                            if (cliente is null || cliente.IsLead == 0) continue;
                            if (cliente.Pessoa is null) continue;
                            foreach(var contacto in cliente.Pessoa.Contactos)
                            {
                                if (contacto is null) continue;
                                if(contacto.Id == contactoId)
                                {
                                    toDelete = contacto;
                                    goto submit;
                                }
                            }
                        }
                    }
                    return Ok(new { error = "Deletion Failed. No Access." });

                case Roles.Agente:
                    var agenteId = _app.GetAgenteId(reqId);
                    toDelete = _db.Contactos.Find(contactoId);
                    if (toDelete == null) return Ok(new { error = "Contacto not found!" });
                    pessoa = _db.Clientes.Where(c => c.AgenteId == agenteId && c.IsLead == 1 && c.PessoaId == toDelete.PessoaId)
                                    .Select(c => c.Pessoa).FirstOrDefault();
                    if (pessoa is null) return Ok(new { error = "The Cliente\\Pessoa was not found" });
                    break;

                case Roles.Cliente:
                    pessoa = _db.ManaUsers.Where(u => u.Id == reqId).Select(u => u.Pessoa).FirstOrDefault();
                    if (pessoa is null) return Ok(new { error = "Pessoa not found. Contact Support." });
                    toDelete = _db.Contactos.Where(c => c.PessoaId == pessoa.Id && c.Id == contactoId).FirstOrDefault();
                    if (toDelete == null) return Ok(new { error = "Contacto not found!" });
                    break;
            }

            submit:
            // Save Changes
            if (toDelete is null) return Ok(new { error = "Nothing Done."});
            _db.Contactos.Remove(toDelete); 
            if (_db.SaveChanges() == 0) return Ok(new { error = "Error Saving Changes!" });

            // Logging
            _logger.SetLogInfoDelete(reqId, "Contacto", contactoId);

            return Ok(new
            {
                message = "Delete Successful",
                contactoId
            });
        }

        #region Utillitaries
        private static bool TipoExists(string tipo)
        {
            foreach(var x in Enum.GetValues(typeof(TipoContacto)))
            {
                if(tipo.Equals(x.ToString())) return true;
            }
            return false;
        }

        private static bool ContactoValid(Contacto contacto)
        {
            if (contacto is null) return false;
            if (contacto.Pessoa is not null) return false;
            if (contacto.PessoaId == 0) return false;
            if (!TipoExists(contacto.Tipo)) return false;
            if (contacto.Valor is null || contacto.Valor.Length <= 0) return false;
            return true;
        }
        #endregion
    }
}
