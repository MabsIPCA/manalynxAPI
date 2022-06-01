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
    public class SinistroVeiculoController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly IAppUtils _appUtils;
        private readonly SinistroUtils _sinistroUtils;
        private readonly ILoggerUtils _logger;

        public SinistroVeiculoController(ApplicationDbContext db, IAppUtils app, ILoggerUtils logger)
        {
            _db = db;
            _appUtils = app;
            _sinistroUtils = new SinistroUtils(db);
            _logger = logger;
        }

        /// <summary>
        /// SinistroVeiculo get Route
        /// This route can only be accessed by authenticad users, any role can access it.
        /// The returned content depends on the role of the user and his permissions to view.
        /// Admins can see any SinistroVeiculo.
        /// Gestores can only see the SinistroVeiculo if it is managed by his Agentes
        /// Agentes can only see the SinistroVeiculo if it is managed by themselves
        /// Cliente can only see the SinistroVeiculo if it is his own.
        /// </summary>
        /// <returns>SinistroVeiculo List, possibly empty</returns>
        [HttpGet, Auth(Roles.Admin, Roles.Gestor, Roles.Agente, Roles.Cliente)]
        public IActionResult Index()
        {
            string bearer = Request.Headers.Authorization[0].Replace("Bearer ", "");
            int? userId = _appUtils.GetUserId(bearer);
            var userRole = _appUtils.GetUserRole(bearer);
            if (userId == null || userRole == null) return BadRequest();

            //do actions according to role
            switch (userRole)
            {
                case Roles.Admin:
                    if (_db.SinistroVeiculos != null)
                    {
                        var adminSin = (from sinistroVeiculo in _db.SinistroVeiculos
                                        select new
                                        {
                                            sinistroVeiculo.Id,
                                            sinistroVeiculo.ApoliceVeiculo,
                                            sinistroVeiculo.Sinistro
                                        }).ToList();

                        _logger.SetLogInfoGetAll(_appUtils.GetUserId(bearer), "SinistroVeiculo");
                        return Ok(adminSin);
                    }
                    return NotFound();
                case Roles.Gestor:
                    int? equipaId = _appUtils.GetEquipaId(userId);
                    if (equipaId == null) return BadRequest();
                    //get sinistrosVeiculos
                    var gestorSinVeiculos = (from sinistroVeiculo in _db.SinistroVeiculos
                                             join apoliceVeiculo in _db.ApoliceVeiculos on sinistroVeiculo.ApoliceVeiculoId equals apoliceVeiculo.Id
                                             join apolice in _db.Apolices on apoliceVeiculo.ApoliceId equals apolice.Id
                                             join agente in _db.Agentes on apolice.AgenteId equals agente.Id
                                             where agente.EquipaId == equipaId
                                             select new
                                             {
                                                 sinistroVeiculo.Id,
                                                 sinistroVeiculo.ApoliceVeiculo,
                                                 sinistroVeiculo.Sinistro
                                             }).ToList();

                    _logger.SetLogInfoGetAll(_appUtils.GetUserId(bearer), "SinistroVeiculo");
                    return Ok(gestorSinVeiculos);
                case Roles.Agente:
                    int? agenteId = _appUtils.GetAgenteId(userId);
                    if (agenteId == null) return BadRequest();
                    //get sinistrosVeiculos
                    var agenteSinVeiculos = (from sinistroVeiculo in _db.SinistroVeiculos
                                             join apoliceVeiculo in _db.ApoliceVeiculos on sinistroVeiculo.ApoliceVeiculoId equals apoliceVeiculo.Id
                                             join apolice in _db.Apolices on apoliceVeiculo.ApoliceId equals apolice.Id
                                             where apolice.AgenteId == agenteId
                                             select new
                                             {
                                                 sinistroVeiculo.Id,
                                                 sinistroVeiculo.ApoliceVeiculo,
                                                 sinistroVeiculo.Sinistro
                                             }).ToList();

                    _logger.SetLogInfoGetAll(_appUtils.GetUserId(bearer), "SinistroVeiculo");
                    return Ok(agenteSinVeiculos);
                case Roles.Cliente:
                    int? clienteId = _appUtils.GetClienteId(userId);
                    if (clienteId == null) return BadRequest();
                    //get sinistrosVeiculos
                    var clienteSinVeiculos = (from sinistroVeiculo in _db.SinistroVeiculos
                                              join apoliceVeiculo in _db.ApoliceVeiculos on sinistroVeiculo.ApoliceVeiculoId equals apoliceVeiculo.Id
                                              join veiculo in _db.Veiculos on apoliceVeiculo.VeiculoId equals veiculo.Id
                                              where veiculo.ClienteId == clienteId
                                              select new
                                              {
                                                  sinistroVeiculo.Id,
                                                  sinistroVeiculo.ApoliceVeiculo,
                                                  sinistroVeiculo.Sinistro
                                              }).ToList();

                    _logger.SetLogInfoGetAll(_appUtils.GetUserId(bearer), "SinistroVeiculo");
                    return Ok(clienteSinVeiculos);
            }
            return NotFound();
        }

        /// <summary>
        /// SinistroVeiculo get Route
        /// This route can only be accessed by authenticad users, any role can access it.
        /// The returned content depends on the role of the user and his permissions to view.
        /// Admins can see any SinistroVeiculo.
        /// Gestores can only see the SinistroVeiculo if it is managed by his Agentes
        /// Agentes can only see the SinistroVeiculo if it is managed by themselves
        /// Cliente can only see the SinistroVeiculo if it is his own.
        /// </summary>
        /// <returns>SinistroVeiculo List, possibly empty</returns>
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
                    if (_db.SinistroVeiculos != null)
                    {
                        var adminSin = (from sinistroVeiculo in _db.SinistroVeiculos
                                        where sinistroVeiculo.Id == Id
                                        select new
                                        {
                                            sinistroVeiculo.Id,
                                            sinistroVeiculo.ApoliceVeiculo,
                                            sinistroVeiculo.Sinistro
                                        }).ToList(); 
                        
                        _logger.SetLogInfoGet(_appUtils.GetUserId(bearer), "SinistroVeiculo", Id);
                        return Ok(adminSin);
                    }
                    return NotFound();
                case Roles.Gestor:
                    int? equipaId = _appUtils.GetEquipaId(userId);
                    if (equipaId == null) return BadRequest();
                    //get sinistrosVeiculos
                    var gestorSinVeiculos = (from sinistroVeiculo in _db.SinistroVeiculos
                                             join apoliceVeiculo in _db.ApoliceVeiculos on sinistroVeiculo.ApoliceVeiculoId equals apoliceVeiculo.Id
                                             join apolice in _db.Apolices on apoliceVeiculo.ApoliceId equals apolice.Id
                                             join agente in _db.Agentes on apolice.AgenteId equals agente.Id
                                             where agente.EquipaId == equipaId && apoliceVeiculo.Id == Id
                                             select new
                                             {
                                                 sinistroVeiculo.Id,
                                                 sinistroVeiculo.ApoliceVeiculo,
                                                 sinistroVeiculo.Sinistro
                                             }).ToList();

                    _logger.SetLogInfoGet(_appUtils.GetUserId(bearer), "SinistroVeiculo", Id);
                    return Ok(gestorSinVeiculos);
                case Roles.Agente:
                    int? agenteId = _appUtils.GetAgenteId(userId);
                    if (agenteId == null) return BadRequest();
                    //get sinistrosVeiculos
                    var agenteSinVeiculos = (from sinistroVeiculo in _db.SinistroVeiculos
                                             join apoliceVeiculo in _db.ApoliceVeiculos on sinistroVeiculo.ApoliceVeiculoId equals apoliceVeiculo.Id
                                             join apolice in _db.Apolices on apoliceVeiculo.ApoliceId equals apolice.Id
                                             where apolice.AgenteId == agenteId && apoliceVeiculo.Id == Id
                                             select new
                                             {
                                                 sinistroVeiculo.Id,
                                                 sinistroVeiculo.ApoliceVeiculo,
                                                 sinistroVeiculo.Sinistro
                                             }).ToList();

                    _logger.SetLogInfoGet(_appUtils.GetUserId(bearer), "SinistroVeiculo", Id);
                    return Ok(agenteSinVeiculos);
                case Roles.Cliente:
                    int? clienteId = _appUtils.GetClienteId(userId);
                    if (clienteId == null) return BadRequest();
                    //get sinistrosVeiculos
                    var clienteSinVeiculos = (from sinistroVeiculo in _db.SinistroVeiculos
                                              join apoliceVeiculo in _db.ApoliceVeiculos on sinistroVeiculo.ApoliceVeiculoId equals apoliceVeiculo.Id
                                              join veiculo in _db.Veiculos on apoliceVeiculo.VeiculoId equals veiculo.Id
                                              where veiculo.ClienteId == clienteId && sinistroVeiculo.Id == Id
                                              select new
                                              {
                                                  sinistroVeiculo.Id,
                                                  sinistroVeiculo.ApoliceVeiculo,
                                                  sinistroVeiculo.Sinistro
                                              }).ToList();

                    _logger.SetLogInfoGet(_appUtils.GetUserId(bearer), "SinistroVeiculo", Id);
                    return Ok(clienteSinVeiculos);
            }
            return NotFound();
        }

        /// <summary>
        /// SinistroVeiculo post Route
        /// This route can only be accessed by authenticad users, any role can access it.
        /// The returned content depends on the role of the user and his permissions to access.
        /// Admins can create any SinistroVeiculo.
        /// Gestores can only create the SinistroVeiculo if it is managed by his Agentes
        /// Agentes can only create the SinistroVeiculo if it is managed by themselves
        /// Cliente can only create the SinistroVeiculo if it is his own.
        /// </summary>
        /// <param name="obj">SinistroVeiculo object</param>
        /// <returns>Updated SinistroVeiculo if create is successful</returns>
        [HttpPost, Auth]
        public IActionResult Create(SinistroVeiculo obj)
        {
            if (ModelState.IsValid)
            {
                string bearer = Request.Headers.Authorization[0].Replace("Bearer ", "");
                int? userId = _appUtils.GetUserId(bearer);
                var userRole = _appUtils.GetUserRole(bearer);
                if (userId == null || userRole == null) return BadRequest();

                //Initializes response variables
                var objectUtils = new SinistroVeiculo();
                var responseUtils = string.Empty;

                //do actions according to role
                switch (userRole)
                {
                    case Roles.Admin:
                        //Calls function from utils
                        (objectUtils, responseUtils) = _sinistroUtils.CreateSinistroVeiculo(obj);

                        if (objectUtils == null) return BadRequest(error: responseUtils);

                        var json = JsonConvert.SerializeObject(objectUtils, new JsonSerializerSettings() { MaxDepth = 1, ReferenceLoopHandling = ReferenceLoopHandling.Ignore });
                        _logger.SetLogInfoPost(_appUtils.GetUserId(bearer), "SinistroVeiculo", json);
                        return Ok(objectUtils);

                    case Roles.Gestor:
                        int? equipaId = _appUtils.GetEquipaId(userId);
                        if (equipaId == null) return BadRequest();

                        //Check if gestor has permission
                        var apVeiculo = _db.ApoliceVeiculos.Find(obj.ApoliceVeiculoId);
                        var apoliceG = _db.Apolices.Find(apVeiculo.ApoliceId);
                        var agenteG = _db.Agentes.Find(apoliceG.AgenteId);
                        if (apVeiculo == null || agenteG.EquipaId != equipaId) return BadRequest();

                        //Calls function from utils
                        (objectUtils, responseUtils) = _sinistroUtils.CreateSinistroVeiculo(obj);

                        if (objectUtils == null) return BadRequest(error: responseUtils);

                        var json2 = JsonConvert.SerializeObject(objectUtils, new JsonSerializerSettings() { MaxDepth = 1, ReferenceLoopHandling = ReferenceLoopHandling.Ignore });
                        _logger.SetLogInfoPost(_appUtils.GetUserId(bearer), "SinistroVeiculo", json2);
                        return Ok(objectUtils);

                    case Roles.Agente:
                        int? agenteId = _appUtils.GetAgenteId(userId);
                        if (agenteId == null) return BadRequest();

                        //Check if agente has permission
                        var apAgenteVeiculo = _db.ApoliceVeiculos.Find(obj.ApoliceVeiculoId);
                        var apoliceA = _db.Apolices.Find(apAgenteVeiculo.ApoliceId);
                        if (apAgenteVeiculo == null || apoliceA.AgenteId != agenteId) return BadRequest();

                        //Calls function from utils
                        (objectUtils, responseUtils) = _sinistroUtils.CreateSinistroVeiculo(obj);

                        if (objectUtils == null) return BadRequest(error: responseUtils);

                        var json3 = JsonConvert.SerializeObject(objectUtils, new JsonSerializerSettings() { MaxDepth = 1, ReferenceLoopHandling = ReferenceLoopHandling.Ignore });
                        _logger.SetLogInfoPost(_appUtils.GetUserId(bearer), "SinistroVeiculo", json3);
                        return Ok(objectUtils);

                    case Roles.Cliente:
                        int? clienteId = _appUtils.GetClienteId(userId);
                        if (clienteId == null) return BadRequest();

                        //Check if cliente has permission
                        var apClienteVeiculo = _db.ApoliceVeiculos.Find(obj.ApoliceVeiculoId);
                        var veiculoCliente = _db.Veiculos.Find(apClienteVeiculo.VeiculoId);
                        if ((apClienteVeiculo == null) || (veiculoCliente.ClienteId != clienteId)) return BadRequest();

                        //Calls function from utils
                        (objectUtils, responseUtils) = _sinistroUtils.CreateSinistroVeiculo(obj);

                        if (objectUtils == null) return BadRequest(error: responseUtils);

                        var json4 = JsonConvert.SerializeObject(objectUtils, new JsonSerializerSettings() { MaxDepth = 1, ReferenceLoopHandling = ReferenceLoopHandling.Ignore });
                        _logger.SetLogInfoPost(_appUtils.GetUserId(bearer), "SinistroVeiculo", json4);
                        return Ok(objectUtils);
                }
                return BadRequest();
            }
            return BadRequest();
        }
    }
}
