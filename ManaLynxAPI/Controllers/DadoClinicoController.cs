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
    public class DadoClinicoController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly IAppUtils _appUtils;
        private readonly IDadoClinicoUtils _dcUtils;
        private readonly ILoggerUtils _logger;

        public DadoClinicoController(ApplicationDbContext db, IAppUtils app, ILoggerUtils logger, IDadoClinicoUtils dcUtils)
        {
            _db = db;
            _appUtils = app;
            _logger = logger;
            _dcUtils = dcUtils;
        }

        /// <summary>
        /// DadoClinico index Route
        /// This route can only be accessed by authenticad users, any role can access it.
        /// The returned content depends on the role of the user and his permissions to view.
        /// Admins can see any DadoClinico.
        /// Gestores can only see the DadoClinico if it is managed by his Agentes
        /// Agentes can only see the DadoClinico if it is managed by themselves
        /// Cliente can only see the DadoClinico if it is his own.
        /// </summary>
        /// <returns>DadoClinico List, possibly empty</returns>
        [HttpGet, Auth]
        public IActionResult Index()
        {
            string bearer = Request.Headers.Authorization[0].Replace("Bearer ", "");
            int? userId = _appUtils.GetUserId(bearer);
            var userRole = _appUtils.GetUserRole(bearer);
            if (userId == null || userRole == null) return BadRequest();

            Console.Write(userRole);

            //do actions according to role
            switch (userRole)
            {
                case Roles.Admin:
                    {
                         var result = (from cliente in _db.Clientes
                                       join dadoClinico in _db.DadoClinicos on cliente.DadoClinicoId equals dadoClinico.Id
                                       select new
                                       {
                                            clienteId = cliente.Id,
                                            dadoClinicoId = dadoClinico.Id,
                                            dadoClinico.Altura,
                                            dadoClinico.Peso,
                                            dadoClinico.Tensao
                                       }).ToList();

                        _logger.SetLogInfoGetAll(_appUtils.GetUserId(bearer), "DadoClinico");
                        return Ok(result);
                    }
                case Roles.Gestor:
                    {
                        int? equipaId = _appUtils.GetEquipaId(userId);
                        if (equipaId == null) return BadRequest();

                        var result = (from cliente in _db.Clientes
                                      join dadoClinico in _db.DadoClinicos on cliente.DadoClinicoId equals dadoClinico.Id
                                      join agente in _db.Agentes on cliente.AgenteId equals agente.Id
                                      where agente.EquipaId == equipaId
                                      select new
                                      {
                                          clienteId = cliente.Id,
                                          dadoClinicoId = dadoClinico.Id,
                                          dadoClinico.Altura,
                                          dadoClinico.Peso,
                                          dadoClinico.Tensao
                                      }).ToList();

                        _logger.SetLogInfoGetAll(_appUtils.GetUserId(bearer), "DadoClinico");
                        return Ok(result);
                    }
                case Roles.Agente:
                    {
                        int? agenteId = _appUtils.GetAgenteId(userId);
                        if (agenteId == null)
                        {
                            return BadRequest();
                        }

                        var result = (from cliente in _db.Clientes
                                      join dadoClinico in _db.DadoClinicos on cliente.DadoClinicoId equals dadoClinico.Id
                                      where cliente.AgenteId == agenteId
                                      select new
                                      {
                                          clienteId = cliente.Id,
                                          dadoClinicoId = dadoClinico.Id,
                                          dadoClinico.Altura,
                                          dadoClinico.Peso,
                                          dadoClinico.Tensao
                                      }).ToList();

                        _logger.SetLogInfoGetAll(_appUtils.GetUserId(bearer), "DadoClinico");
                        return Ok(result);
                    }
                case Roles.Cliente:
                    {
                        int? clienteId = _appUtils.GetClienteId(userId);
                        if (clienteId == null) return BadRequest();

                        var result = (from cliente in _db.Clientes
                                      join dadoClinico in _db.DadoClinicos on cliente.DadoClinicoId equals dadoClinico.Id
                                      where cliente.Id == clienteId
                                      select new
                                      {
                                          dadoClinicoId = dadoClinico.Id,
                                          dadoClinico.Altura,
                                          dadoClinico.Peso,
                                          dadoClinico.Tensao
                                      }).ToList();

                        _logger.SetLogInfoGetAll(_appUtils.GetUserId(bearer), "DadoClinico");
                        return Ok(result);
                    }
            }
            return NotFound();
        }

        /// <summary>
        /// DadoClinico IndexById Route
        /// This route can only be accessed by authenticad users, any role can access it.
        /// The returned content depends on the role of the user and his permissions to view.
        /// Admins can see any DadoClinico.
        /// Gestores can only see the DadoClinico if it is managed by his Agentes
        /// Agentes can only see the DadoClinico if it is managed by themselves
        /// Cliente can only see the DadoClinico if it is his own.
        /// </summary>
        /// <param name="Id">DadoClinicoId to get</param>
        /// <returns>DadoClinico List, size one or zero</returns>
        [HttpGet("{Id}"), Auth]
        public IActionResult IndexById(int? Id)
        {
            string bearer = Request.Headers.Authorization[0].Replace("Bearer ", "");
            int? userId = _appUtils.GetUserId(bearer);
            var userRole = _appUtils.GetUserRole(bearer);
            if (userId == null || userRole == null) return BadRequest();
            if (Id == null || Id == 0) return BadRequest();

            //do actions according to role
            switch (userRole)
            {
                case Roles.Admin:
                    {
                        var result = (from cliente in _db.Clientes
                                      join dadoClinico in _db.DadoClinicos on cliente.DadoClinicoId equals dadoClinico.Id
                                      where dadoClinico.Id == Id
                                      select new
                                      {
                                          clienteId = cliente.Id,
                                          dadoClinicoId = dadoClinico.Id,
                                          dadoClinico.Altura,
                                          dadoClinico.Peso,
                                          dadoClinico.Tensao
                                      }).ToList();

                        _logger.SetLogInfoGet(_appUtils.GetUserId(bearer), "DadoClinico", Id);
                        return Ok(result);
                    }
                case Roles.Gestor:
                {
                    int? equipaId = _appUtils.GetEquipaId(userId);
                    if (equipaId == null) return BadRequest();
                        var result = (from cliente in _db.Clientes
                                      join dadoClinico in _db.DadoClinicos on cliente.DadoClinicoId equals dadoClinico.Id
                                      join agente in _db.Agentes on cliente.AgenteId equals agente.Id
                                      where agente.EquipaId == equipaId
                                      where dadoClinico.Id == Id
                                      select new
                                      {
                                          clienteId = cliente.Id,
                                          dadoClinicoId = dadoClinico.Id,
                                          dadoClinico.Altura,
                                          dadoClinico.Peso,
                                          dadoClinico.Tensao
                                      }).ToList();

                        _logger.SetLogInfoGet(_appUtils.GetUserId(bearer), "DadoClinico", Id);
                        return Ok(result);
                    }
                case Roles.Agente:
                {
                    int? agenteId = _appUtils.GetAgenteId(userId);
                    if (agenteId == null)
                    {
                        return BadRequest();
                    }

                    var result = (from cliente in _db.Clientes
                                    join dadoClinico in _db.DadoClinicos on cliente.DadoClinicoId equals dadoClinico.Id
                                    where cliente.AgenteId == agenteId
                                    where dadoClinico.Id == Id
                                  select new
                                  {
                                      clienteId = cliente.Id,
                                      dadoClinicoId = dadoClinico.Id,
                                      dadoClinico.Altura,
                                      dadoClinico.Peso,
                                      dadoClinico.Tensao
                                  }
                                    ).ToList();

                        _logger.SetLogInfoGet(_appUtils.GetUserId(bearer), "DadoClinico", Id);
                        return Ok(result);
                }
            }
            return NotFound();
        }


        /// <summary>
        /// DadoClinico put Route
        /// This route can only be accessed by authenticad users, any role can access it.
        /// The returned content depends on the role of the user and his permissions to access.
        /// Admins can update any DadoClinico.
        /// Gestores can only update the DadoClinico if it is managed by his Agentes
        /// Agentes can only update the DadoClinico if it is managed by themselves
        /// Cliente can only update the DadoClinico if it is his own.
        /// </summary>
        /// <param name="obj">DadoClinico object</param>
        /// <returns>Updated DadoClinico if update is successful, if not, return the sent object</returns>
        [HttpPut("edit"), Auth]
        public IActionResult Edit(DadoClinico obj)
        {
            string bearer = Request.Headers.Authorization[0].Replace("Bearer ", "");
            int? userId = _appUtils.GetUserId(bearer);
            var userRole = _appUtils.GetUserRole(bearer);
            if (userId == null || userRole == null) return BadRequest();

            //Initializes response variables
            var objectUtils = new DadoClinico();
            var responseUtils = string.Empty;

            //do actions according to role
            switch (userRole)
            {
                case Roles.Admin:
                {
                    if (ModelState.IsValid)
                    {
                        //Calls function from utils
                        (objectUtils, responseUtils) = _dcUtils.UpdateDadoClinico(obj);

                        if (objectUtils == null) return BadRequest(error: responseUtils);

                        //Logs
                        var json = JsonConvert.SerializeObject(objectUtils, new JsonSerializerSettings() { MaxDepth = 1, ReferenceLoopHandling = ReferenceLoopHandling.Ignore });
                        _logger.SetLogInfoPut(_appUtils.GetUserId(bearer), "DadoClinico", json);

                        return Ok(objectUtils);

                    }
                    return BadRequest(error: "Model state invalid");
                }
                case Roles.Gestor:
                {
                    int? equipaId = _appUtils.GetEquipaId(userId);
                    if (equipaId == null) return BadRequest();

                    var result = (from cliente in _db.Clientes
                                    join dadoClinico in _db.DadoClinicos on cliente.DadoClinicoId equals dadoClinico.Id
                                    join agente in _db.Agentes on cliente.AgenteId equals agente.Id
                                    where agente.EquipaId == equipaId
                                    select new
                                    {
                                        cliente.Id,
                                        dadoClinico
                                    }).ToList();

                    if (result.Count() > 0)
                    {
                        if (ModelState.IsValid)
                        {
                            //Calls function from utils
                            (objectUtils, responseUtils) = _dcUtils.UpdateDadoClinico(obj);

                            if (objectUtils == null) return BadRequest(error: responseUtils);

                            //Logs
                            var json = JsonConvert.SerializeObject(objectUtils, new JsonSerializerSettings() { MaxDepth = 1, ReferenceLoopHandling = ReferenceLoopHandling.Ignore });
                            _logger.SetLogInfoPut(_appUtils.GetUserId(bearer), "DadoClinico", json);

                            return Ok(objectUtils);
                        }
                        return BadRequest(error: "Model State Invalid");
                    }
                    return BadRequest(error: "There is no Gestor associated with that Agente");
                }
                case Roles.Agente:
                {
                    int? agenteId = _appUtils.GetAgenteId(userId);
                    if (agenteId == null)
                    {
                        return BadRequest();
                    }

                    var result = (from cliente in _db.Clientes
                                    join dadoClinico in _db.DadoClinicos on cliente.DadoClinicoId equals dadoClinico.Id
                                    where cliente.AgenteId == agenteId
                                    select new
                                    {
                                        dadoClinico
                                    }).ToList();

                    if (result.Count() > 0)
                    {
                        if (ModelState.IsValid)
                        {
                            //Calls function from utils
                            (objectUtils, responseUtils) = _dcUtils.UpdateDadoClinico(obj);

                            if (objectUtils == null) return BadRequest(error: responseUtils);

                            //Logs
                            var json = JsonConvert.SerializeObject(objectUtils, new JsonSerializerSettings() { MaxDepth = 1, ReferenceLoopHandling = ReferenceLoopHandling.Ignore });
                            _logger.SetLogInfoPut(_appUtils.GetUserId(bearer), "DadoClinico", json);

                            return Ok(objectUtils);
                        }
                        return BadRequest(error: "Model State Invalid");
                    }
                    return BadRequest(error: "This Agente isn't associated with this Cliente");
                }
                case Roles.Cliente:
                {
                    int? clienteId = _appUtils.GetClienteId(userId);
                    if (clienteId == null) return BadRequest();

                    var result = (from cliente in _db.Clientes
                                    join dadoClinico in _db.DadoClinicos on cliente.DadoClinicoId equals dadoClinico.Id
                                    where cliente.Id == clienteId
                                    select new
                                    {
                                        cliente.Id,
                                        dadoClinico
                                    }).ToList();

                    if (result.Count() > 0)
                    {
                        if (ModelState.IsValid)
                        {
                            //Calls function from utils
                            (objectUtils, responseUtils) = _dcUtils.UpdateDadoClinico(obj);

                            if (objectUtils == null) return BadRequest(error: responseUtils);

                            //Logs
                            var json = JsonConvert.SerializeObject(objectUtils, new JsonSerializerSettings() { MaxDepth = 1, ReferenceLoopHandling = ReferenceLoopHandling.Ignore });
                            _logger.SetLogInfoPut(_appUtils.GetUserId(bearer), "DadoClinico", json);

                            return Ok(objectUtils);
                        }
                        return BadRequest(error: "Model State Invalid");
                    }
                    return BadRequest(error: "This Client doesn't have DadoClinico");
                }
            }
            return NotFound();
        }
    }
}