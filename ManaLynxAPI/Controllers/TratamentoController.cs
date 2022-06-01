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
    public class TratamentoController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly IAppUtils _appUtils;
        private readonly ILoggerUtils _logger;
        private readonly ITratamentoUtils _trUtils;

        public TratamentoController(ApplicationDbContext db, IAppUtils app, ILoggerUtils logger, ITratamentoUtils tratamento)
        {
            _db = db;
            _appUtils = app;
            _logger = logger;
            _trUtils = tratamento;
        }

        /// <summary>
        /// DadoClinico index Route
        /// This route can only be accessed by authenticad users, any role can access it.
        /// The returned content depends on the role of the user and his permissions to view.
        /// Admins can see any Tratamento.
        /// Gestores can only see the Tratamento if it is managed by his Agentes
        /// Agentes can only see the Tratamento if it is managed by themselves
        /// Cliente can only see the Tratamento if it is his own.
        /// </summary>
        /// <returns>Tratamento List, possibly empty</returns>
        [HttpGet, Auth]
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
                    {
                        var result = (from cliente in _db.Clientes
                                      join dadoClinico in _db.DadoClinicos on cliente.DadoClinicoId equals dadoClinico.Id
                                      join tratamento in _db.Tratamentos on dadoClinico.Id equals tratamento.DadoClinicoId
                                      select new
                                      {
                                          clienteId = cliente.Id,
                                          tratamento
                                      }).ToList();

                        _logger.SetLogInfoGetAll(_appUtils.GetUserId(bearer), "Tratamento");
                        return Ok(result);
                    }
                case Roles.Gestor:
                    {
                        int? equipaId = _appUtils.GetEquipaId(userId);
                        if (equipaId == null) return BadRequest();

                        var result = (from cliente in _db.Clientes
                                      join dadoClinico in _db.DadoClinicos on cliente.DadoClinicoId equals dadoClinico.Id
                                      join tratamento in _db.Tratamentos on dadoClinico.Id equals tratamento.DadoClinicoId
                                      join agente in _db.Agentes on cliente.AgenteId equals agente.Id
                                      where agente.EquipaId == equipaId
                                      select new
                                      {
                                          clienteId = cliente.Id,
                                          tratamento
                                      }).ToList();

                        _logger.SetLogInfoGetAll(_appUtils.GetUserId(bearer), "Tratamento");
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
                                      join tratamento in _db.Tratamentos on dadoClinico.Id equals tratamento.DadoClinicoId
                                      where cliente.AgenteId == agenteId
                                      select new
                                      {
                                          clienteId = cliente.Id,
                                          tratamento
                                      }).ToList();

                        _logger.SetLogInfoGetAll(_appUtils.GetUserId(bearer), "Tratamento");
                        return Ok(result);
                    }
                case Roles.Cliente:
                    {
                        int? clienteId = _appUtils.GetClienteId(userId);
                        if (clienteId == null) return BadRequest();

                        var result = (from cliente in _db.Clientes
                                      join dadoClinico in _db.DadoClinicos on cliente.DadoClinicoId equals dadoClinico.Id
                                      join tratamento in _db.Tratamentos on dadoClinico.Id equals tratamento.DadoClinicoId
                                      where cliente.Id == clienteId
                                      select new
                                      {
                                          tratamento
                                      }).ToList();

                        _logger.SetLogInfoGetAll(_appUtils.GetUserId(bearer), "Tratamento");
                        return Ok(result);
                    }
            }
            return NotFound();
        }

        /// <summary>
        /// DadoClinico ByClientId Route
        /// This route can only be accessed by authenticad users, any role can access it.
        /// The returned content depends on the role of the user and his permissions to view.
        /// Admins can see any Tratamento.
        /// Gestores can only see the Tratamento if it is managed by his Agentes
        /// Agentes can only see the Tratamento if it is managed by themselves
        /// Cliente can only see the Tratamento if it is his own.
        /// </summary>
        /// <param name="Id">id of client</param>
        /// <returns>Tratamento List, possibly empty</returns>
        [HttpGet("{Id}"), Auth]
        public IActionResult ByClientId(int? Id)
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
                                      join tratamento in _db.Tratamentos on dadoClinico.Id equals tratamento.DadoClinicoId
                                      where cliente.Id == Id
                                      select new
                                      {
                                          clienteId = cliente.Id,
                                          tratamento
                                      }).ToList();

                        _logger.SetLogInfoGetAll(_appUtils.GetUserId(bearer), "Tratamento");
                        return Ok(result);
                    }
                case Roles.Gestor:
                    {
                        int? equipaId = _appUtils.GetEquipaId(userId);
                        if (equipaId == null) return BadRequest();

                        var result = (from cliente in _db.Clientes
                                      join dadoClinico in _db.DadoClinicos on cliente.DadoClinicoId equals dadoClinico.Id
                                      join tratamento in _db.Tratamentos on dadoClinico.Id equals tratamento.DadoClinicoId
                                      join agente in _db.Agentes on cliente.AgenteId equals agente.Id
                                      where agente.EquipaId == equipaId
                                      where cliente.Id == Id
                                      select new
                                      {
                                          clienteId = cliente.Id,
                                          tratamento
                                      }).ToList();

                        _logger.SetLogInfoGetAll(_appUtils.GetUserId(bearer), "Tratamento");
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
                                      join tratamento in _db.Tratamentos on dadoClinico.Id equals tratamento.DadoClinicoId
                                      where cliente.AgenteId == agenteId
                                      where cliente.Id == Id
                                      select new
                                      {
                                          clienteId = cliente.Id,
                                          tratamento
                                      }).ToList();

                        _logger.SetLogInfoGetAll(_appUtils.GetUserId(bearer), "Tratamento");
                        return Ok(result);
                    }
            }
            return NotFound();
        }

        /// <summary>
        /// DadoClinico create Route
        /// This route can only be accessed by authenticad users, any role can access it.
        /// The returned content depends on the role of the user and his permissions to post.
        /// Admins can see post Tratamento.
        /// Gestores can only post the Tratamento if it is managed by his Agentes
        /// Agentes can only post the Tratamento if it is managed by themselves
        /// Cliente can only post the Tratamento if it is his own.
        /// </summary>
        /// <param name="obj">Tratamento object</param>
        /// <returns>Updated Tratamento if update is successful, if not, return the sent object.</returns>
        [HttpPost("create"), Auth]
        public IActionResult Create(Tratamento obj)
        {
            string bearer = Request.Headers.Authorization[0].Replace("Bearer ", "");
            int? userId = _appUtils.GetUserId(bearer);
            var userRole = _appUtils.GetUserRole(bearer);
            if (userId == null || userRole == null) return BadRequest();

            //Initializes response variables
            var objectUtils = new Tratamento();
            var responseUtils = string.Empty;

            switch (userRole)
            {
                case Roles.Admin:
                    {
                        if (obj != null){

                            //Calls function from utils
                            (objectUtils, responseUtils) = _trUtils.CreateTratamento(obj);

                            if (objectUtils == null) return BadRequest(error: responseUtils);

                            var json = JsonConvert.SerializeObject(objectUtils, new JsonSerializerSettings() { MaxDepth = 1, ReferenceLoopHandling = ReferenceLoopHandling.Ignore });
                            _logger.SetLogInfoPost(_appUtils.GetUserId(bearer), "Tratamento", json);

                            return Ok(objectUtils);
                        }
                        else return BadRequest(obj);
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
                            if (obj != null)
                            {

                                var createObj = new Tratamento();

                                //Assigns variables to the createObj
                                createObj.NomeTratamento = obj.NomeTratamento;
                                createObj.Frequencia = obj.Frequencia;
                                createObj.UltimaToma = obj.UltimaToma;
                                createObj.DadoClinicoId = obj.DadoClinicoId;

                                //Updates DadoClinico with the data given
                                _db.Tratamentos.Add(createObj);
                                _db.SaveChanges();

                                var json = JsonConvert.SerializeObject(createObj, new JsonSerializerSettings() { MaxDepth = 1, ReferenceLoopHandling = ReferenceLoopHandling.Ignore });
                                _logger.SetLogInfoPost(_appUtils.GetUserId(bearer), "Tratamento", json);

                                return Ok(createObj);
                            }
                            else return BadRequest(obj);
                        }

                        return BadRequest();
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
                            if (obj != null)
                            {

                                var createObj = new Tratamento();

                                //Assigns variables to the createObj
                                createObj.NomeTratamento = obj.NomeTratamento;
                                createObj.Frequencia = obj.Frequencia;
                                createObj.UltimaToma = obj.UltimaToma;
                                createObj.DadoClinicoId = obj.DadoClinicoId;

                                //Updates DadoClinico with the data given
                                _db.Tratamentos.Add(createObj);
                                _db.SaveChanges();

                                var json = JsonConvert.SerializeObject(createObj, new JsonSerializerSettings() { MaxDepth = 1, ReferenceLoopHandling = ReferenceLoopHandling.Ignore });
                                _logger.SetLogInfoPost(_appUtils.GetUserId(bearer), "Tratamento", json);

                                return Ok(createObj);
                            }
                            else return BadRequest(obj);
                        }

                        return BadRequest();
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
                            if (obj != null)
                            {

                                var createObj = new Tratamento();

                                //Assigns variables to the createObj
                                createObj.NomeTratamento = obj.NomeTratamento;
                                createObj.Frequencia = obj.Frequencia;
                                createObj.UltimaToma = obj.UltimaToma;
                                createObj.DadoClinicoId = obj.DadoClinicoId;

                                //Updates DadoClinico with the data given
                                _db.Tratamentos.Add(createObj);
                                _db.SaveChanges();

                                var json = JsonConvert.SerializeObject(createObj, new JsonSerializerSettings() { MaxDepth = 1, ReferenceLoopHandling = ReferenceLoopHandling.Ignore });
                                _logger.SetLogInfoPost(_appUtils.GetUserId(bearer), "Tratamento", json);

                                return Ok(createObj);
                            }
                            else return BadRequest(obj);
                        }
                        return BadRequest();
                    }
            }
            return NotFound();
        }

        /// <summary>
        /// DadoClinico delete Route
        /// This route can only be accessed by authenticad users, any role can access it.
        /// The returned content depends on the role of the user and his permissions to post.
        /// Admins can delete Tratamento.
        /// Gestores can only delete the Tratamento if it is managed by his Agentes
        /// Agentes can only delete the Tratamento if it is managed by themselves
        /// Cliente can only delete the Tratamento if it is his own.
        /// </summary>
        /// <param name="id">TratamentoId to delete</param>
        /// <returns>Tratamento if delete is successful, if not, return badRequest.</returns>
        [HttpDelete("delete"), Auth]
        public IActionResult Delete(int Id)
        {
            string bearer = Request.Headers.Authorization[0].Replace("Bearer ", "");
            int? userId = _appUtils.GetUserId(bearer);
            var userRole = _appUtils.GetUserRole(bearer);
            if (userId == null || userRole == null) return BadRequest();

            //do actions according to role
            switch (userRole)
            {
                case Roles.Admin:
                    {
                        var obj = _db.Tratamentos.Find(Id);
                        if (obj == null)
                        {
                            return NotFound();
                        }
                        _db.Tratamentos.Remove(obj);
                        _db.SaveChanges();

                        _logger.SetLogInfoDelete(_appUtils.GetUserId(bearer), "Tratamento", Id);
                        return Ok(obj);

                    }
                case Roles.Gestor:
                    {
                        int? equipaId = _appUtils.GetEquipaId(userId);
                        if (equipaId == null) return BadRequest();

                        var result = (from cliente in _db.Clientes
                                      join dadoClinico in _db.DadoClinicos on cliente.DadoClinicoId equals dadoClinico.Id
                                      join tratamento in _db.Tratamentos on dadoClinico.Id equals tratamento.DadoClinicoId
                                      join agente in _db.Agentes on cliente.AgenteId equals agente.Id
                                      where agente.EquipaId == equipaId
                                      select new
                                      {
                                          tratamento.Id
                                      }).ToList();

                        if (result.Count == 0)
                        {
                            var obj = _db.Tratamentos.Find(Id);
                            if (obj == null)
                            {
                                return NotFound();
                            }
                            _db.Tratamentos.Remove(obj);
                            _db.SaveChanges();

                            _logger.SetLogInfoDelete(_appUtils.GetUserId(bearer), "Tratamento", Id);
                            return Ok(obj);
                        }
                        return BadRequest();
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
                                      join tratamento in _db.Tratamentos on dadoClinico.Id equals tratamento.DadoClinicoId
                                      where cliente.AgenteId == agenteId
                                      select new
                                      {
                                          tratamento.Id
                                      }).ToList();

                        if (result.Count == 0)
                        {
                            var obj = _db.Tratamentos.Find(Id);
                            if (obj == null)
                            {
                                return NotFound();
                            }
                            _db.Tratamentos.Remove(obj);
                            _db.SaveChanges();

                            _logger.SetLogInfoDelete(_appUtils.GetUserId(bearer), "Tratamento", Id);
                            return Ok(obj);
                        }
                        return BadRequest();
                    }
                case Roles.Cliente:
                    {
                        int? clienteId = _appUtils.GetClienteId(userId);
                        if (clienteId == null) return BadRequest();

                        var result = (from cliente in _db.Clientes
                                      join dadoClinico in _db.DadoClinicos on cliente.DadoClinicoId equals dadoClinico.Id
                                      join tratamento in _db.Tratamentos on dadoClinico.Id equals tratamento.DadoClinicoId
                                      where cliente.Id == clienteId
                                      select new
                                      {
                                          tratamento.Id
                                      }).ToList();

                        if (result.Count == 0)
                        {
                            var obj = _db.Tratamentos.Find(Id);
                            if (obj == null)
                            {
                                return NotFound();
                            }
                            _db.Tratamentos.Remove(obj);
                            _db.SaveChanges();

                            _logger.SetLogInfoDelete(_appUtils.GetUserId(bearer), "Tratamento", Id);
                            return Ok(obj);
                        }
                        return BadRequest();
                    }
            }
            return NotFound();
        }
    }
}
