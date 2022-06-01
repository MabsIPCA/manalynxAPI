using Microsoft.AspNetCore.Authorization;
using Auth = ManaLynxAPI.Authentication.Auth;
using Roles = ManaLynxAPI.Models.Roles;
using ManaLynxAPI.Data;
using ManaLynxAPI.Models;
using ManaLynxAPI.Utils;
using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;
using Newtonsoft.Json;

namespace ManaLynxAPI.Controllers
{

    [Authorize]
    [ApiController, Route("[controller]")]
    public class TransacaoController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly ILoggerUtils _logger;
        private readonly IAppUtils _app;

        public TransacaoController(ApplicationDbContext db, ILoggerUtils logger, IAppUtils app)
        {
            _db = db;
            _logger = logger;
            _app = app;
        }

        /// <summary>
        /// Transacao index Route
        /// This route can only be accessed by authenticad users, any role can access it.
        /// The returned content depends on the role of the user and his permissions to view.
        /// Admins can see all the data in the database.
        /// Gestores can only see Transacoes from ApolicesSaude managed by his Agentes
        /// Agentes can only see Transacoes from ApolicesSaude managed by himselves
        /// Cliente can only see his Transacoes from his ApolicesSaude
        /// </summary>
        /// <returns>Transacao List, possibly empty</returns>
        [HttpGet, Auth(Roles.Admin, Roles.Gestor, Roles.Agente, Roles.Cliente)]
        public IActionResult Index()
        {
            string bearer = Request.Headers.Authorization[0].Replace("Bearer ", "");
            int? userId = _app.GetUserId(bearer);
            var userRole = _app.GetUserRole(bearer);
            if (userId == null || userRole == null) return BadRequest("Invalid user");

            switch (userRole)
            {
                case Roles.Admin:
                    if (_db.Transacaos != null)
                    {
                        var objTransacao = (from tran in _db.Transacaos
                                            select new
                                            {
                                                tran.Id,
                                                tran.Descricao,
                                                tran.DataTransacao,
                                                tran.Montante,
                                                tran.ApoliceSaude,
                                            }).ToList();

                        _logger.SetLogInfoGetAll(userId, "Transacao");

                        return Ok(objTransacao);
                    }
                    else return NotFound();

                case Roles.Gestor:
                    int? equipaId = _app.GetEquipaId(userId);
                    if (equipaId == null) return BadRequest("Inalid gestor");

                    var objGestorTransacao = (from tran in _db.Transacaos
                                              join apSaude in _db.ApoliceSaudes on tran.ApoliceSaudeId equals apSaude.Id
                                              join apolice in _db.Apolices on apSaude.ApoliceId equals apolice.Id
                                              join agente in _db.Agentes on apolice.AgenteId equals agente.Id
                                              where agente.EquipaId == equipaId
                                              select new
                                              {
                                                  tran.Id,
                                                  tran.Descricao,
                                                  tran.DataTransacao,
                                                  tran.Montante,
                                                  tran.ApoliceSaude,
                                              }).ToList();

                    var token = Request.Headers.Authorization[0].Replace("Bearer ", "");
                    _logger.SetLogInfoGetAll(_app.GetUserId(token), "Transacao");
                    return Ok(objGestorTransacao);

                case Roles.Agente:
                    int? agenteId = _app.GetAgenteId(userId);
                    if (agenteId == null) return BadRequest("Invalid Agente");

                    var objAgenteTransacao = (from tran in _db.Transacaos
                                              join apSaude in _db.ApoliceSaudes on tran.ApoliceSaudeId equals apSaude.Id
                                              join apolice in _db.Apolices on apSaude.ApoliceId equals apolice.Id
                                              where apolice.AgenteId == agenteId
                                              select new
                                              {
                                                  tran.Id,
                                                  tran.Descricao,
                                                  tran.DataTransacao,
                                                  tran.Montante,
                                                  tran.ApoliceSaude,
                                              }).ToList();

                    _logger.SetLogInfoGetAll(userId, "Transacao");
                    return Ok(objAgenteTransacao);

                case Roles.Cliente:
                    int? clienteId = _app.GetClienteId(userId);
                    if (clienteId == null) return BadRequest("Invalid Cliente");

                    var objClienteTransacao = (from tran in _db.Transacaos
                                               join apSaude in _db.ApoliceSaudes on tran.ApoliceSaudeId equals apSaude.Id
                                               where apSaude.ClienteId == clienteId
                                               select new
                                               {
                                                   tran.Id,
                                                   tran.Descricao,
                                                   tran.DataTransacao,
                                                   tran.Montante,
                                                   tran.ApoliceSaude,
                                               }).ToList();

                    _logger.SetLogInfoGetAll(userId, "Transacao");
                    return Ok(objClienteTransacao);

                default: return BadRequest();
            }

        }

        /// <summary>
        /// Transacao Get by Id Route
        /// This route can only be accessed by authenticad users, any role can access it.
        /// The returned content depends on the role of the user and his permissions to view.
        /// Admins can see any Trasacao.
        /// Gestores can only see the Trasacao if it ApoliceSaude is managed by his Agentes
        /// Agentes can only see the Transacao if it ApoliceSaude is managed by himselves
        /// Cliente can only see the Transacao if it ApoliceSaude is his own.
        /// </summary>
        /// <param name="Id">TransacaoId to get</param>
        /// <returns>Transacao List, size one or zero</returns>
        [HttpGet("{Id}"), Auth(Roles.Admin)]
        public IActionResult ViewById(int? Id)
        {
            if (Id == null || Id == 0)
            {
                return NotFound();
            }

            string bearer = Request.Headers.Authorization[0].Replace("Bearer ", "");
            int? userId = _app.GetUserId(bearer);
            var userRole = _app.GetUserRole(bearer);
            if (userId == null || userRole == null) return BadRequest("Invalid user");

            switch (userRole)
            {
                case Roles.Admin:
                    var objTransacao = (from tran in _db.Transacaos
                                        where tran.Id == Id
                                        select new
                                        {
                                            tran.Id,
                                            tran.Descricao,
                                            tran.DataTransacao,
                                            tran.Montante,
                                            tran.ApoliceSaude,
                                        }).ToList();

                    if (objTransacao == null)
                    {
                        return NotFound();
                    }

                    _logger.SetLogInfoGet(userId, "Transacao", Id);
                    return Ok(objTransacao);

                case Roles.Gestor:
                    int? equipaId = _app.GetEquipaId(userId);
                    if (equipaId == null) return BadRequest("Inalid gestor");

                    var objGestorTransacao = (from tran in _db.Transacaos
                                              join apSaude in _db.ApoliceSaudes on tran.ApoliceSaudeId equals apSaude.Id
                                              join apolice in _db.Apolices on apSaude.ApoliceId equals apolice.Id
                                              join agente in _db.Agentes on apolice.AgenteId equals agente.Id
                                              where tran.Id == Id && agente.EquipaId == equipaId
                                              select new
                                              {
                                                  tran.Id,
                                                  tran.Descricao,
                                                  tran.DataTransacao,
                                                  tran.Montante,
                                                  tran.ApoliceSaude,
                                              }).ToList();
                    if (objGestorTransacao == null)
                    {
                        return NotFound();
                    }

                    _logger.SetLogInfoGet(userId, "Transacao", Id);
                    return Ok(objGestorTransacao);

                case Roles.Agente:
                    int? agenteId = _app.GetAgenteId(userId);
                    if (agenteId == null) return BadRequest("Invalid Agente");

                    var objAgenteTransacao = (from tran in _db.Transacaos
                                              join apSaude in _db.ApoliceSaudes on tran.ApoliceSaudeId equals apSaude.Id
                                              join apolice in _db.Apolices on apSaude.ApoliceId equals apolice.Id
                                              where tran.Id == Id && apolice.AgenteId == agenteId
                                              select new
                                              {
                                                  tran.Id,
                                                  tran.Descricao,
                                                  tran.DataTransacao,
                                                  tran.Montante,
                                                  tran.ApoliceSaude,
                                              }).ToList();

                    if (objAgenteTransacao == null)
                    {
                        return NotFound();
                    }

                    _logger.SetLogInfoGet(userId, "Transacao", Id);

                    return Ok(objAgenteTransacao);

                case Roles.Cliente:
                    int? clienteId = _app.GetClienteId(userId);
                    if (clienteId == null) return BadRequest("Invalid Cliente");

                    var objClienteTransacao = (from tran in _db.Transacaos
                                               join apSaude in _db.ApoliceSaudes on tran.ApoliceSaudeId equals apSaude.Id
                                               where tran.Id == Id && apSaude.ClienteId == clienteId
                                               select new
                                               {
                                                   tran.Id,
                                                   tran.Descricao,
                                                   tran.DataTransacao,
                                                   tran.Montante,
                                                   tran.ApoliceSaude,
                                               }).ToList();
                    if (objClienteTransacao == null)
                    {
                        return NotFound();
                    }

                    _logger.SetLogInfoGet(userId, "Transacao", Id);
                    return Ok(objClienteTransacao);

                default: return BadRequest();
            }
        }

        /// <summary>
        /// Transacao Create Route
        /// This route can only be accessed by admin users.
        /// </summary>
        /// <param name="obj">Transacao Object</param>
        /// <returns>If sucessfull return created TransacaoId</returns>
        [HttpPost, Auth(Roles.Admin)]
        public IActionResult Create(Transacao obj)
        {
            if (ModelState.IsValid)
            {
                var createObj = new Transacao();

                //Assigns variables to the createObj
                createObj.Descricao = obj.Descricao;
                createObj.DataTransacao = obj.DataTransacao;
                createObj.ApoliceSaudeId = obj.ApoliceSaudeId;
                createObj.Montante = obj.Montante;

                _db.Transacaos.Add(createObj);
                _db.SaveChanges();

                var json = JsonConvert.SerializeObject(createObj, new JsonSerializerSettings() { MaxDepth = 1, ReferenceLoopHandling = ReferenceLoopHandling.Ignore });

                var token = Request.Headers.Authorization[0].Replace("Bearer ", "");
                _logger.SetLogInfoPost(_app.GetUserId(token), "Transacao", json);

                return Ok(createObj.Id);
            }
            return BadRequest("Invalid Object");
        }

        /// <summary>
        /// Transacao Update Route
        /// This route can only be accessed by admin users.
        /// </summary>
        /// <param name="Id">TransacaoId to Update</param>
        /// <param name="obj">Transacao Object</param>
        /// <returns>Updated Transacao if update is successful</returns>
        [HttpPut("{Id}"), Auth(Roles.Admin)]
        public IActionResult Edit(int Id, Transacao obj)
        {
            if (ModelState.IsValid)
            {
                var updateObj = _db.Transacaos.Find(Id);

                if (updateObj != null)
                {
                    //Assigns variables to the updateObj
                    if (obj.Descricao != null) updateObj.Descricao = obj.Descricao;
                    if (obj.DataTransacao != null) updateObj.DataTransacao = obj.DataTransacao;
                    if (obj.ApoliceSaudeId != null) updateObj.ApoliceSaudeId = obj.ApoliceSaudeId;
                    if (obj.Montante != null) updateObj.Montante = obj.Montante;

                    //Updates Seguros with the data given
                    _db.Transacaos.Update(updateObj);
                    _db.SaveChanges();

                    var json = JsonConvert.SerializeObject(updateObj, new JsonSerializerSettings() { MaxDepth = 1, ReferenceLoopHandling = ReferenceLoopHandling.Ignore });

                    var token = Request.Headers.Authorization[0].Replace("Bearer ", "");
                    _logger.SetLogInfoPut(_app.GetUserId(token), "Transacao", json);

                    return Ok(updateObj);
                }
                else return NotFound(obj);

            }
            return View(obj);
        }

        /// <summary>
        /// Transacao Delete Route
        /// This route can only be accessed by admin users.
        /// </summary>
        /// <param name="Id">TransacaoId to delete</param>
        /// <returns>Ok if successful</returns>
        [HttpDelete("{Id}"), Auth(Roles.Admin)]
        public IActionResult Delete(int Id)
        {
            var obj = _db.Transacaos.Find(Id);
            if (obj == null)
            {
                return NotFound();
            }
            _db.Transacaos.Remove(obj);
            _db.SaveChanges();

            var token = Request.Headers.Authorization[0].Replace("Bearer ", "");
            _logger.SetLogInfoDelete(_app.GetUserId(token), "Transacao", Id);

            return Ok();
        }
    }
}
