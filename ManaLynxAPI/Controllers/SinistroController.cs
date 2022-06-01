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
    public class SinistroController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly IAppUtils _appUtils;
        private readonly ILoggerUtils _logger;
        private readonly ISinistroUtils _siUtils;


        public SinistroController(ApplicationDbContext db, IAppUtils app, ILoggerUtils logger, ISinistroUtils sinistro)
        {
            _db = db;
            _appUtils = app;
            _logger = logger;
            _siUtils = sinistro;
        }

        /// <summary>
        /// Sinistro index Route
        /// This route can only be accessed by authenticad users, any role can access it.
        /// The returned content depends on the role of the user and his permissions to view.
        /// Admins can see any Sinistro.
        /// Gestores can only see the Sinistro if it is managed by his Agentes
        /// Agentes can only see the Sinistro if it is managed by themselves
        /// Cliente can only see the Sinistro if it is his own.
        /// </summary>
        /// <returns>Sinistro List, possibly empty</returns>
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
                    if (_db.Sinistros != null)
                    {
                        var adminSin = (from sinistro in _db.Sinistros
                                        select new
                                        {
                                            sinistro.Id,
                                            sinistro.Descricao,
                                            sinistro.DataSinistro,
                                            sinistro.Estado,
                                            sinistro.Reembolso,
                                            sinistro.Valido,
                                            sinistro.Deferido,
                                            sinistro.Provas,
                                            sinistro.RelatorioPeritagems,
                                            sinistro.SinistroPessoals,
                                            sinistro.SinistroVeiculos
                                        }).ToList();
                        _logger.SetLogInfoGetAll(_appUtils.GetUserId(token), "Sinistro");
                        return Ok(adminSin);
                    }
                    return NotFound();
                case Roles.Gestor:
                    int? equipaId = _appUtils.GetEquipaId(userId);
                    if (equipaId == null) return BadRequest();
                    //get sinistrosPessoais
                    var gestorSinPessoals = (from sinistro in _db.Sinistros
                                             join sinistroPessoal in _db.SinistroPessoals on sinistro.Id equals sinistroPessoal.SinistroId
                                             join apolicePessoal in _db.ApolicePessoals on sinistroPessoal.ApolicePessoalId equals apolicePessoal.Id
                                             join apolice in _db.Apolices on apolicePessoal.ApoliceId equals apolice.Id
                                             join agente in _db.Agentes on apolice.AgenteId equals agente.Id
                                             where agente.EquipaId == equipaId
                                             select new
                                             {
                                                 sinistro.Id,
                                                 sinistro.Descricao,
                                                 sinistro.DataSinistro,
                                                 sinistro.Estado,
                                                 sinistro.Reembolso,
                                                 sinistro.Valido,
                                                 sinistro.Deferido,
                                                 sinistro.Provas,
                                                 sinistro.RelatorioPeritagems,
                                                 sinistro.SinistroPessoals,
                                                 sinistro.SinistroVeiculos
                                             }).ToList();
                    //get sinistrosVeiculos
                    var gestorSinVeiculos = (from sinistro in _db.Sinistros
                                             join sinistroVeiculo in _db.SinistroVeiculos on sinistro.Id equals sinistroVeiculo.SinistroId
                                             join apoliceVeiculo in _db.ApoliceVeiculos on sinistroVeiculo.ApoliceVeiculoId equals apoliceVeiculo.Id
                                             join apolice in _db.Apolices on apoliceVeiculo.ApoliceId equals apolice.Id
                                             join agente in _db.Agentes on apolice.AgenteId equals agente.Id
                                             where agente.EquipaId == equipaId
                                             select new
                                             {
                                                 sinistro.Id,
                                                 sinistro.Descricao,
                                                 sinistro.DataSinistro,
                                                 sinistro.Estado,
                                                 sinistro.Reembolso,
                                                 sinistro.Valido,
                                                 sinistro.Deferido,
                                                 sinistro.Provas,
                                                 sinistro.RelatorioPeritagems,
                                                 sinistro.SinistroPessoals,
                                                 sinistro.SinistroVeiculos
                                             }).ToList();
                    //join the two lists of sinistros types
                    gestorSinVeiculos.ForEach(item => gestorSinPessoals.Add(item));

                    _logger.SetLogInfoGetAll(_appUtils.GetUserId(token), "Sinistro");
                    return Ok(gestorSinPessoals);
                case Roles.Agente:
                    int? agenteId = _appUtils.GetAgenteId(userId);
                    if (agenteId == null) return BadRequest();
                    //get sinistrosPessoais
                    var agenteSinPessoals = (from sinistro in _db.Sinistros
                                             join sinistroPessoal in _db.SinistroPessoals on sinistro.Id equals sinistroPessoal.SinistroId
                                             join apolicePessoal in _db.ApolicePessoals on sinistroPessoal.ApolicePessoalId equals apolicePessoal.Id
                                             join apolice in _db.Apolices on apolicePessoal.ApoliceId equals apolice.Id
                                             where apolice.AgenteId == agenteId
                                             select new
                                             {
                                                 sinistro.Id,
                                                 sinistro.Descricao,
                                                 sinistro.DataSinistro,
                                                 sinistro.Estado,
                                                 sinistro.Reembolso,
                                                 sinistro.Valido,
                                                 sinistro.Deferido,
                                                 sinistro.Provas,
                                                 sinistro.RelatorioPeritagems,
                                                 sinistro.SinistroPessoals,
                                                 sinistro.SinistroVeiculos
                                             }).ToList();
                    //get sinistrosVeiculos
                    var agenteSinVeiculos = (from sinistro in _db.Sinistros
                                             join sinistroVeiculo in _db.SinistroVeiculos on sinistro.Id equals sinistroVeiculo.SinistroId
                                             join apoliceVeiculo in _db.ApoliceVeiculos on sinistroVeiculo.ApoliceVeiculoId equals apoliceVeiculo.Id
                                             join apolice in _db.Apolices on apoliceVeiculo.ApoliceId equals apolice.Id
                                             where apolice.AgenteId == agenteId
                                             select new
                                             {
                                                 sinistro.Id,
                                                 sinistro.Descricao,
                                                 sinistro.DataSinistro,
                                                 sinistro.Estado,
                                                 sinistro.Reembolso,
                                                 sinistro.Valido,
                                                 sinistro.Deferido,
                                                 sinistro.Provas,
                                                 sinistro.RelatorioPeritagems,
                                                 sinistro.SinistroPessoals,
                                                 sinistro.SinistroVeiculos
                                             }).ToList();
                    //join the two lists of sinistros types
                    agenteSinVeiculos.ForEach(item => agenteSinPessoals.Add(item));

                    _logger.SetLogInfoGetAll(_appUtils.GetUserId(token), "Sinistro");
                    return Ok(agenteSinPessoals);
                case Roles.Cliente:
                    int? clienteId = _appUtils.GetClienteId(userId);
                    if (clienteId == null) return BadRequest();
                    //get sinistrosPessoais
                    var clienteSinPessoals = (from sinistro in _db.Sinistros
                                             join sinistroPessoal in _db.SinistroPessoals on sinistro.Id equals sinistroPessoal.SinistroId
                                             join apolicePessoal in _db.ApolicePessoals on sinistroPessoal.ApolicePessoalId equals apolicePessoal.Id
                                             where apolicePessoal.ClienteId == clienteId
                                              select new
                                             {
                                                 sinistro.Id,
                                                 sinistro.Descricao,
                                                 sinistro.DataSinistro,
                                                 sinistro.Estado,
                                                 sinistro.Reembolso,
                                                 sinistro.Valido,
                                                 sinistro.Deferido,
                                                 sinistro.Provas,
                                                 sinistro.RelatorioPeritagems,
                                                 sinistro.SinistroPessoals,
                                                 sinistro.SinistroVeiculos
                                             }).ToList();
                    //get sinistrosVeiculos
                    var clienteSinVeiculos = (from sinistro in _db.Sinistros
                                             join sinistroVeiculo in _db.SinistroVeiculos on sinistro.Id equals sinistroVeiculo.SinistroId
                                             join apoliceVeiculo in _db.ApoliceVeiculos on sinistroVeiculo.ApoliceVeiculoId equals apoliceVeiculo.Id
                                             join veiculo in _db.Veiculos on apoliceVeiculo.VeiculoId equals veiculo.Id
                                             where veiculo.ClienteId == clienteId
                                             select new
                                             {
                                                 sinistro.Id,
                                                 sinistro.Descricao,
                                                 sinistro.DataSinistro,
                                                 sinistro.Estado,
                                                 sinistro.Reembolso,
                                                 sinistro.Valido,
                                                 sinistro.Deferido,
                                                 sinistro.Provas,
                                                 sinistro.RelatorioPeritagems,
                                                 sinistro.SinistroPessoals,
                                                 sinistro.SinistroVeiculos
                                             }).ToList();
                    //join the two lists of sinistros types
                    clienteSinVeiculos.ForEach(item => clienteSinPessoals.Add(item));

                    _logger.SetLogInfoGetAll(_appUtils.GetUserId(token), "Sinistro");
                    return Ok(clienteSinPessoals);
            }
            return NotFound();
        }

        /// <summary>
        /// Sinistro get Route
        /// This route can only be accessed by authenticad users, any role can access it.
        /// The returned content depends on the role of the user and his permissions to view.
        /// Admins can see any Sinistro.
        /// Gestores can only see the Sinistro if it is managed by his Agentes
        /// Agentes can only see the Sinistro if it is managed by themselves
        /// Cliente can only see the Sinistro if it is his own.
        /// </summary>
        /// <returns>Sinistro List, possibly empty</returns>
        [HttpGet("{id}"), Auth(Roles.Admin, Roles.Gestor, Roles.Agente, Roles.Cliente)]
        public IActionResult Get(int? id)
        {
            if (id == null || id == 0)
            {
                return NotFound();
            }

            var token = Request.Headers.Authorization[0].Replace("Bearer ", "");
            string bearer = Request.Headers.Authorization[0].Replace("Bearer ", "");
            int? userId = _appUtils.GetUserId(bearer);
            var userRole = _appUtils.GetUserRole(bearer);
            if (userId == null || userRole == null) return BadRequest();

            //do actions according to role
            switch (userRole)
            {
                case Roles.Admin:
                    if (_db.Sinistros != null)
                    {
                        var adminSin = (from sinistro in _db.Sinistros
                                        where sinistro.Id == id
                                        select new
                                        {
                                            sinistro.Id,
                                            sinistro.Descricao,
                                            sinistro.DataSinistro,
                                            sinistro.Estado,
                                            sinistro.Reembolso,
                                            sinistro.Valido,
                                            sinistro.Deferido,
                                            sinistro.Provas,
                                            sinistro.RelatorioPeritagems,
                                            sinistro.SinistroPessoals,
                                            sinistro.SinistroVeiculos
                                        }).ToList();

                        _logger.SetLogInfoGet(_appUtils.GetUserId(token), "Sinistro", id);
                        return Ok(adminSin);
                    }
                    return NotFound();
                case Roles.Gestor:
                    int? equipaId = _appUtils.GetEquipaId(userId);
                    if (equipaId == null) return BadRequest();
                    //get sinistrosPessoais
                    var gestorSinPessoals = (from sinistro in _db.Sinistros
                                             join sinistroPessoal in _db.SinistroPessoals on sinistro.Id equals sinistroPessoal.SinistroId
                                             join apolicePessoal in _db.ApolicePessoals on sinistroPessoal.ApolicePessoalId equals apolicePessoal.Id
                                             join apolice in _db.Apolices on apolicePessoal.ApoliceId equals apolice.Id
                                             join agente in _db.Agentes on apolice.AgenteId equals agente.Id
                                             where agente.EquipaId == equipaId && sinistro.Id == id
                                             select new
                                             {
                                                 sinistro.Id,
                                                 sinistro.Descricao,
                                                 sinistro.DataSinistro,
                                                 sinistro.Estado,
                                                 sinistro.Reembolso,
                                                 sinistro.Valido,
                                                 sinistro.Deferido,
                                                 sinistro.Provas,
                                                 sinistro.RelatorioPeritagems,
                                                 sinistro.SinistroPessoals,
                                                 sinistro.SinistroVeiculos
                                             }).ToList();
                    //get sinistrosVeiculos
                    var gestorSinVeiculos = (from sinistro in _db.Sinistros
                                             join sinistroVeiculo in _db.SinistroVeiculos on sinistro.Id equals sinistroVeiculo.SinistroId
                                             join apoliceVeiculo in _db.ApoliceVeiculos on sinistroVeiculo.ApoliceVeiculoId equals apoliceVeiculo.Id
                                             join apolice in _db.Apolices on apoliceVeiculo.ApoliceId equals apolice.Id
                                             join agente in _db.Agentes on apolice.AgenteId equals agente.Id
                                             where agente.EquipaId == equipaId && sinistro.Id == id
                                             select new
                                             {
                                                 sinistro.Id,
                                                 sinistro.Descricao,
                                                 sinistro.DataSinistro,
                                                 sinistro.Estado,
                                                 sinistro.Reembolso,
                                                 sinistro.Valido,
                                                 sinistro.Deferido,
                                                 sinistro.Provas,
                                                 sinistro.RelatorioPeritagems,
                                                 sinistro.SinistroPessoals,
                                                 sinistro.SinistroVeiculos
                                             }).ToList();
                    //join the two lists of sinistros types
                    gestorSinVeiculos.ForEach(item => gestorSinPessoals.Add(item));

                    _logger.SetLogInfoGet(_appUtils.GetUserId(token), "Sinistro", id);
                    return Ok(gestorSinPessoals);
                case Roles.Agente:
                    int? agenteId = _appUtils.GetAgenteId(userId);
                    if (agenteId == null) return BadRequest();
                    //get sinistrosPessoais
                    var agenteSinPessoals = (from sinistro in _db.Sinistros
                                             join sinistroPessoal in _db.SinistroPessoals on sinistro.Id equals sinistroPessoal.SinistroId
                                             join apolicePessoal in _db.ApolicePessoals on sinistroPessoal.ApolicePessoalId equals apolicePessoal.Id
                                             join apolice in _db.Apolices on apolicePessoal.ApoliceId equals apolice.Id
                                             where apolice.AgenteId == agenteId && sinistro.Id == id
                                             select new
                                             {
                                                 sinistro.Id,
                                                 sinistro.Descricao,
                                                 sinistro.DataSinistro,
                                                 sinistro.Estado,
                                                 sinistro.Reembolso,
                                                 sinistro.Valido,
                                                 sinistro.Deferido,
                                                 sinistro.Provas,
                                                 sinistro.RelatorioPeritagems,
                                                 sinistro.SinistroPessoals,
                                                 sinistro.SinistroVeiculos
                                             }).ToList();
                    //get sinistrosVeiculos
                    var agenteSinVeiculos = (from sinistro in _db.Sinistros
                                             join sinistroVeiculo in _db.SinistroVeiculos on sinistro.Id equals sinistroVeiculo.SinistroId
                                             join apoliceVeiculo in _db.ApoliceVeiculos on sinistroVeiculo.ApoliceVeiculoId equals apoliceVeiculo.Id
                                             join apolice in _db.Apolices on apoliceVeiculo.ApoliceId equals apolice.Id
                                             where apolice.AgenteId == agenteId && sinistro.Id == id
                                             select new
                                             {
                                                 sinistro.Id,
                                                 sinistro.Descricao,
                                                 sinistro.DataSinistro,
                                                 sinistro.Estado,
                                                 sinistro.Reembolso,
                                                 sinistro.Valido,
                                                 sinistro.Deferido,
                                                 sinistro.Provas,
                                                 sinistro.RelatorioPeritagems,
                                                 sinistro.SinistroPessoals,
                                                 sinistro.SinistroVeiculos
                                             }).ToList();
                    //join the two lists of sinistros types
                    agenteSinVeiculos.ForEach(item => agenteSinPessoals.Add(item));

                    _logger.SetLogInfoGet(_appUtils.GetUserId(token), "Sinistro", id);
                    return Ok(agenteSinPessoals);
                case Roles.Cliente:
                    int? clienteId = _appUtils.GetClienteId(userId);
                    if (clienteId == null) return BadRequest();
                    //get sinistrosPessoais
                    var clienteSinPessoals = (from sinistro in _db.Sinistros
                                              join sinistroPessoal in _db.SinistroPessoals on sinistro.Id equals sinistroPessoal.SinistroId
                                              join apolicePessoal in _db.ApolicePessoals on sinistroPessoal.ApolicePessoalId equals apolicePessoal.Id
                                              where apolicePessoal.ClienteId == clienteId && sinistro.Id == id
                                              select new
                                              {
                                                  sinistro.Id,
                                                  sinistro.Descricao,
                                                  sinistro.DataSinistro,
                                                  sinistro.Estado,
                                                  sinistro.Reembolso,
                                                  sinistro.Valido,
                                                  sinistro.Deferido,
                                                  sinistro.Provas,
                                                  sinistro.RelatorioPeritagems,
                                                  sinistro.SinistroPessoals,
                                                  sinistro.SinistroVeiculos
                                              }).ToList();
                    //get sinistrosVeiculos
                    var clienteSinVeiculos = (from sinistro in _db.Sinistros
                                              join sinistroVeiculo in _db.SinistroVeiculos on sinistro.Id equals sinistroVeiculo.SinistroId
                                              join apoliceVeiculo in _db.ApoliceVeiculos on sinistroVeiculo.ApoliceVeiculoId equals apoliceVeiculo.Id
                                              join veiculo in _db.Veiculos on apoliceVeiculo.VeiculoId equals veiculo.Id
                                              where veiculo.ClienteId == clienteId && sinistro.Id == id
                                              select new
                                              {
                                                  sinistro.Id,
                                                  sinistro.Descricao,
                                                  sinistro.DataSinistro,
                                                  sinistro.Estado,
                                                  sinistro.Reembolso,
                                                  sinistro.Valido,
                                                  sinistro.Deferido,
                                                  sinistro.Provas,
                                                  sinistro.RelatorioPeritagems,
                                                  sinistro.SinistroPessoals,
                                                  sinistro.SinistroVeiculos
                                              }).ToList();
                    //join the two lists of sinistros types
                    clienteSinVeiculos.ForEach(item => clienteSinPessoals.Add(item));

                    _logger.SetLogInfoGet(_appUtils.GetUserId(token), "Sinistro", id);
                    return Ok(clienteSinPessoals);
            }
            return NotFound();
        }

        /// <summary>
        /// Sinistro put Route
        /// This route can only be accessed by authenticad users, any role can access it.
        /// The returned content depends on the role of the user and his permissions to access.
        /// Admins can update any Sinistro.
        /// Gestores can only update the Sinistro if it is managed by his Agentes
        /// Agentes can only update the Sinistro if it is managed by themselves
        /// Cliente can only update the Sinistro if it is his own.
        /// </summary>
        /// <param name="obj">Sinistro object</param>
        /// <returns>Updated Sinistro if update is successful, if not, return the sent object</returns>
        [HttpPut("{id}"), Auth(Roles.Admin, Roles.Gestor, Roles.Agente, Roles.Cliente)]
        public IActionResult Update(Sinistro obj)
        {
            string bearer = Request.Headers.Authorization[0].Replace("Bearer ", "");
            int? userId = _appUtils.GetUserId(bearer);
            var userRole = _appUtils.GetUserRole(bearer);
            if (userId == null || userRole == null) return BadRequest();

            //Initializes response variables
            var objectUtils = new Sinistro();
            var responseUtils = string.Empty;

            //do actions according to role
            switch (userRole)
            {
                case Roles.Admin:
                    {
                        if (ModelState.IsValid)
                        {
                            //Calls function from utils
                            (objectUtils, responseUtils) = _siUtils.UpdateSinistro(obj);

                            if (objectUtils == null) return BadRequest(error: responseUtils);

                            //Logs
                            var json = JsonConvert.SerializeObject(objectUtils, new JsonSerializerSettings() { MaxDepth = 1, ReferenceLoopHandling = ReferenceLoopHandling.Ignore });
                            _logger.SetLogInfoPut(_appUtils.GetUserId(bearer), "Sinistro", json);

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
                                (objectUtils, responseUtils) = _siUtils.UpdateSinistro(obj);

                                if (objectUtils == null) return BadRequest(error: responseUtils);

                                //Logs
                                var json = JsonConvert.SerializeObject(objectUtils, new JsonSerializerSettings() { MaxDepth = 1, ReferenceLoopHandling = ReferenceLoopHandling.Ignore });
                                _logger.SetLogInfoPut(_appUtils.GetUserId(bearer), "Sinistro", json);

                                return Ok(objectUtils);

                            }
                            return BadRequest(error: "Model state invalid");
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
                                (objectUtils, responseUtils) = _siUtils.UpdateSinistro(obj);

                                if (objectUtils == null) return BadRequest(error: responseUtils);

                                //Logs
                                var json = JsonConvert.SerializeObject(objectUtils, new JsonSerializerSettings() { MaxDepth = 1, ReferenceLoopHandling = ReferenceLoopHandling.Ignore });
                                _logger.SetLogInfoPut(_appUtils.GetUserId(bearer), "Sinistro", json);

                                return Ok(objectUtils);

                            }
                            return BadRequest(error: "Model state invalid");
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
                                (objectUtils, responseUtils) = _siUtils.UpdateSinistro(obj);

                                if (objectUtils == null) return BadRequest(error: responseUtils);

                                //Logs
                                var json = JsonConvert.SerializeObject(objectUtils, new JsonSerializerSettings() { MaxDepth = 1, ReferenceLoopHandling = ReferenceLoopHandling.Ignore });
                                _logger.SetLogInfoPut(_appUtils.GetUserId(bearer), "Sinistro", json);

                                return Ok(objectUtils);

                            }
                            return BadRequest(error: "Model state invalid");
                        }
                        return BadRequest(error: "This Client doesn't have Sinistro");
                    }
            }
            return NotFound();
        }

        /// <summary>
        /// Sinistro delete Route
        /// This route can only be accessed by authenticad users, only admin role can access it.
        /// Admins can delete any DadoClinico.
        /// </summary>
        /// <param name="Id">Sinistro id</param>
        /// <returns>ok status or notfound</returns>
        [HttpDelete("{id}"), Auth(Roles.Admin)]
        public IActionResult Delete(int Id)
        {
            var obj = _db.Sinistros.Find(Id);
            if (obj == null)
            {
                return NotFound();
            }
            _db.Sinistros.Remove(obj);
            _db.SaveChanges(); 
            
            var token = Request.Headers.Authorization[0].Replace("Bearer ", "");
            _logger.SetLogInfoDelete(_appUtils.GetUserId(token), "Sinistro", Id);

            return Ok();
        }
    }
}
