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
    [ApiController, Route("[controller]")]
    public class SinistroPessoalController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly IAppUtils _appUtils;
        private readonly SinistroUtils _sinistroUtils;
        private readonly ILoggerUtils _logger;

        public SinistroPessoalController(ApplicationDbContext db, IAppUtils app, ILoggerUtils logger)
        {
            _db = db;
            _appUtils = app;
            _sinistroUtils = new SinistroUtils(_db);
            _logger = logger;
        }

        /// <summary>
        /// SinistroPessoal index Route
        /// This route can only be accessed by authenticad users, any role can access it.
        /// The returned content depends on the role of the user and his permissions to view.
        /// Admins can see any SinistroPessoal.
        /// Gestores can only see the SinistroPessoal if it is managed by his Agentes
        /// Agentes can only see the SinistroPessoal if it is managed by themselves
        /// Cliente can only see the SinistroPessoal if it is his own.
        /// </summary>
        /// <returns>SinistroPessoal List, possibly empty</returns>
        [HttpGet, Auth(Roles.Admin, Roles.Gestor, Roles.Agente, Roles.Cliente)]
        public IActionResult Index()
        {
            var token = Request.Headers.Authorization[0].Replace("Bearer ", "");
            string bearer = Request.Headers.Authorization[0].Replace("Bearer ", "");
            int? userId = _appUtils.GetUserId(bearer);
            var userRole = _appUtils.GetUserRole(bearer);
            if (userId == null || userRole == null) return BadRequest();

            //do actions according to role
            switch (userRole)
            {
                case Roles.Admin:
                    if (_db.SinistroPessoals != null)
                    {
                        var adminSin = (from sinistroPessoal in _db.SinistroPessoals
                                        select new
                                        {
                                            sinistroPessoal.Id,
                                            sinistroPessoal.ApolicePessoal,
                                            sinistroPessoal.Sinistro
                                        }).ToList();

                        _logger.SetLogInfoGetAll(_appUtils.GetUserId(token), "SinistroPessoal");
                        return Ok(adminSin);
                    }
                    return NotFound();
                case Roles.Gestor:
                    int? equipaId = _appUtils.GetEquipaId(userId);
                    if (equipaId == null) return BadRequest();
                    //get sinistrosPessoais
                    var gestorSinPessoals = (from sinistroPessoal in _db.SinistroPessoals
                                             join apolicePessoal in _db.ApolicePessoals on sinistroPessoal.ApolicePessoalId equals apolicePessoal.Id
                                             join apolice in _db.Apolices on apolicePessoal.ApoliceId equals apolice.Id
                                             join agente in _db.Agentes on apolice.AgenteId equals agente.Id
                                             where agente.EquipaId == equipaId
                                             select new
                                             {
                                                 sinistroPessoal.Id,
                                                 sinistroPessoal.ApolicePessoal,
                                                 sinistroPessoal.Sinistro
                                             }).ToList();

                    _logger.SetLogInfoGetAll(_appUtils.GetUserId(token), "SinistroPessoal");
                    return Ok(gestorSinPessoals);
                case Roles.Agente:
                    int? agenteId = _appUtils.GetAgenteId(userId);
                    if (agenteId == null) return BadRequest();
                    //get sinistrosPessoais
                    var agenteSinPessoals = (from sinistroPessoal in _db.SinistroPessoals
                                             join apolicePessoal in _db.ApolicePessoals on sinistroPessoal.ApolicePessoalId equals apolicePessoal.Id
                                             join apolice in _db.Apolices on apolicePessoal.ApoliceId equals apolice.Id
                                             where apolice.AgenteId == agenteId
                                             select new
                                             {
                                                 sinistroPessoal.Id,
                                                 sinistroPessoal.ApolicePessoal,
                                                 sinistroPessoal.Sinistro
                                             }).ToList();

                    _logger.SetLogInfoGetAll(_appUtils.GetUserId(token), "SinistroPessoal");
                    return Ok(agenteSinPessoals);
                case Roles.Cliente:
                    int? clienteId = _appUtils.GetClienteId(userId);
                    if (clienteId == null) return BadRequest();
                    //get sinistrosPessoais
                    var clienteSinPessoals = (from sinistroPessoal in _db.SinistroPessoals
                                              join apolicePessoal in _db.ApolicePessoals on sinistroPessoal.ApolicePessoalId equals apolicePessoal.Id
                                              where apolicePessoal.ClienteId == clienteId
                                              select new
                                              {
                                                  sinistroPessoal.Id,
                                                  sinistroPessoal.ApolicePessoal,
                                                  sinistroPessoal.Sinistro
                                              }).ToList();

                    _logger.SetLogInfoGetAll(_appUtils.GetUserId(token), "SinistroPessoal");
                    return Ok(clienteSinPessoals);
            }
            return NotFound();
        }

        /// <summary>
        /// SinistroPessoal get Route
        /// This route can only be accessed by authenticad users, any role can access it.
        /// The returned content depends on the role of the user and his permissions to view.
        /// Admins can see any SinistroPessoal.
        /// Gestores can only see the SinistroPessoal if it is managed by his Agentes
        /// Agentes can only see the SinistroPessoal if it is managed by themselves
        /// Cliente can only see the SinistroPessoal if it is his own.
        /// </summary>
        /// <returns>SinistroPessoal List, possibly empty</returns>
        [HttpGet("{Id}"), Auth]
        public IActionResult ViewById(int? Id)
        {
            if (Id == null || Id == 0)
            {
                return NotFound();
            }
            string bearer = Request.Headers.Authorization[0].Replace("Bearer ", "");
            int? userId = _appUtils.GetUserId(bearer);
            var userRole = _appUtils.GetUserRole(bearer);
            if (userId == null || userRole == null) return BadRequest();

            //do actions according to role
            switch (userRole)
            {
                case Roles.Admin:
                    if (_db.SinistroPessoals != null)
                    {
                        var adminSin = (from sinistroPessoal in _db.SinistroPessoals
                                        where sinistroPessoal.Id == Id
                                        select new
                                        {
                                            sinistroPessoal.Id,
                                            sinistroPessoal.ApolicePessoal,
                                            sinistroPessoal.Sinistro
                                        }).ToList();

                        _logger.SetLogInfoGet(_appUtils.GetUserId(bearer), "SinistroPessoal", Id);
                        return Ok(adminSin);
                    }
                    return NotFound();
                case Roles.Gestor:
                    int? equipaId = _appUtils.GetEquipaId(userId);
                    if (equipaId == null) return BadRequest();
                    //get sinistrosPessoais
                    var gestorSinPessoals = (from sinistroPessoal in _db.SinistroPessoals
                                             join apolicePessoal in _db.ApolicePessoals on sinistroPessoal.ApolicePessoalId equals apolicePessoal.Id
                                             join apolice in _db.Apolices on apolicePessoal.ApoliceId equals apolice.Id
                                             join agente in _db.Agentes on apolice.AgenteId equals agente.Id
                                             where agente.EquipaId == equipaId && sinistroPessoal.Id == Id
                                             select new
                                             {
                                                 sinistroPessoal.Id,
                                                 sinistroPessoal.ApolicePessoal,
                                                 sinistroPessoal.Sinistro
                                             }).ToList();

                    _logger.SetLogInfoGet(_appUtils.GetUserId(bearer), "SinistroPessoal", Id);
                    return Ok(gestorSinPessoals);
                case Roles.Agente:
                    int? agenteId = _appUtils.GetAgenteId(userId);
                    if (agenteId == null) return BadRequest();
                    //get sinistrosPessoais
                    var agenteSinPessoals = (from sinistroPessoal in _db.SinistroPessoals
                                             join apolicePessoal in _db.ApolicePessoals on sinistroPessoal.ApolicePessoalId equals apolicePessoal.Id
                                             join apolice in _db.Apolices on apolicePessoal.ApoliceId equals apolice.Id
                                             where apolice.AgenteId == agenteId && sinistroPessoal.Id == Id
                                             select new
                                             {
                                                 sinistroPessoal.Id,
                                                 sinistroPessoal.ApolicePessoal,
                                                 sinistroPessoal.Sinistro
                                             }).ToList();

                    _logger.SetLogInfoGet(_appUtils.GetUserId(bearer), "SinistroPessoal", Id);
                    return Ok(agenteSinPessoals);
                case Roles.Cliente:
                    int? clienteId = _appUtils.GetClienteId(userId);
                    if (clienteId == null) return BadRequest();
                    //get sinistrosPessoais
                    var clienteSinPessoals = (from sinistroPessoal in _db.SinistroPessoals
                                              join apolicePessoal in _db.ApolicePessoals on sinistroPessoal.ApolicePessoalId equals apolicePessoal.Id
                                              where apolicePessoal.ClienteId == clienteId && sinistroPessoal.Id == Id
                                              select new
                                              {
                                                  sinistroPessoal.Id,
                                                  sinistroPessoal.ApolicePessoal,
                                                  sinistroPessoal.Sinistro
                                              }).ToList();

                    _logger.SetLogInfoGet(_appUtils.GetUserId(bearer), "SinistroPessoal", Id);
                    return Ok(clienteSinPessoals);
            }
            return NotFound();
        }

        /// <summary>
        /// SinistroPessoal post Route
        /// This route can only be accessed by authenticad users, any role can access it.
        /// The returned content depends on the role of the user and his permissions to access.
        /// Admins can create any SinistroPessoal.
        /// Gestores can only create the SinistroPessoal if it is managed by his Agentes
        /// Agentes can only create the SinistroPessoal if it is managed by themselves
        /// Cliente can only create the SinistroPessoal if it is his own.
        /// </summary>
        /// <param name="obj">SinistroPessoal object</param>
        /// <returns>Updated SinistroPessoal if create is successful</returns>
        [HttpPost, Auth(Roles.Admin, Roles.Gestor, Roles.Agente, Roles.Cliente)]
        public IActionResult Create(SinistroPessoal obj)
        {
            if (ModelState.IsValid)
            {
                string bearer = Request.Headers.Authorization[0].Replace("Bearer ", "");
                int? userId = _appUtils.GetUserId(bearer);
                var userRole = _appUtils.GetUserRole(bearer);
                if (userId == null || userRole == null) return BadRequest();

                //Initializes response variables
                var objectUtils = new SinistroPessoal();
                var responseUtils = string.Empty;

                //do actions according to role
                switch (userRole)
                {
                    case Roles.Admin:

                        //Calls function from utils
                        (objectUtils, responseUtils) = _sinistroUtils.CreateSinistroPessoal(obj);

                        if (objectUtils == null) return BadRequest(error: responseUtils);

                        var json = JsonConvert.SerializeObject(objectUtils, new JsonSerializerSettings() { MaxDepth = 1, ReferenceLoopHandling = ReferenceLoopHandling.Ignore });
                        _logger.SetLogInfoPost(_appUtils.GetUserId(bearer), "SinistroPessoal", json);

                        return Ok(objectUtils);

                    case Roles.Gestor:
                        int? equipaId = _appUtils.GetEquipaId(userId);
                        if (equipaId == null) return BadRequest();

                        //Check if gestor has permission
                        var apPessoal = _db.ApolicePessoals.Find(obj.ApolicePessoalId);
                        var apoliceG = _db.Apolices.Find(apPessoal.ApoliceId);
                        var agenteG = _db.Agentes.Find(apoliceG.AgenteId);
                        if (apPessoal == null || agenteG.EquipaId != equipaId) return BadRequest();

                        //Calls function from utils
                        (objectUtils, responseUtils) = _sinistroUtils.CreateSinistroPessoal(obj);

                        if (objectUtils == null) return BadRequest(error: responseUtils);

                        var json2 = JsonConvert.SerializeObject(objectUtils, new JsonSerializerSettings() { MaxDepth = 1, ReferenceLoopHandling = ReferenceLoopHandling.Ignore });
                        _logger.SetLogInfoPost(_appUtils.GetUserId(bearer), "SinistroPessoal", json2);

                        return Ok(objectUtils);

                    case Roles.Agente:
                        int? agenteId = _appUtils.GetAgenteId(userId);
                        if (agenteId == null) return BadRequest();

                        //Check if agente has permission
                        var apAgentePessoal = _db.ApolicePessoals.Find(obj.ApolicePessoalId);
                        var apoliceA = _db.Apolices.Find(apAgentePessoal.ApoliceId);
                        if (apAgentePessoal == null || apoliceA.AgenteId != agenteId) return BadRequest();

                        //Calls function from utils
                        (objectUtils, responseUtils) = _sinistroUtils.CreateSinistroPessoal(obj);

                        if (objectUtils == null) return BadRequest(error: responseUtils);

                        var json3 = JsonConvert.SerializeObject(objectUtils, new JsonSerializerSettings() { MaxDepth = 1, ReferenceLoopHandling = ReferenceLoopHandling.Ignore });
                        _logger.SetLogInfoPost(_appUtils.GetUserId(bearer), "SinistroPessoal", json3);

                        return Ok(objectUtils);

                    case Roles.Cliente:
                        int? clienteId = _appUtils.GetClienteId(userId);
                        if (clienteId == null) return BadRequest();

                        //Check if cliente has permission
                        var apClientePessoal = _db.ApolicePessoals.Find(obj.ApolicePessoalId);
                        if (apClientePessoal == null || apClientePessoal.ClienteId != clienteId) return BadRequest();

                        //Calls function from utils
                        (objectUtils, responseUtils) = _sinistroUtils.CreateSinistroPessoal(obj);

                        if (objectUtils == null) return BadRequest(error: responseUtils);

                        var json4 = JsonConvert.SerializeObject(objectUtils, new JsonSerializerSettings() { MaxDepth = 1, ReferenceLoopHandling = ReferenceLoopHandling.Ignore });
                        _logger.SetLogInfoPost(_appUtils.GetUserId(bearer), "SinistroPessoal", json4);

                        return Ok(objectUtils);
                }
                return BadRequest();
            }
            return BadRequest();
        }
    }
}
