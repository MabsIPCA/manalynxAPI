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
    public class ProvaController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly IAppUtils _appUtils;
        private readonly ISinistroUtils _sinistroUtils;
        private readonly ILoggerUtils _logger;
        private readonly IProvaUtils  _prUtils;

        public ProvaController(ApplicationDbContext db, IAppUtils app, ILoggerUtils logger, IProvaUtils prova)
        {
            _db = db;
            _appUtils = app;
            _sinistroUtils = new SinistroUtils(db);
            _logger = logger;
            _prUtils = prova;
        }

        /// <summary>
        /// Prova index Route
        /// This route can only be accessed by authenticad users, any role can access it.
        /// The returned content depends on the role of the user and his permissions to view.
        /// Admins can see any Prova.
        /// Gestores can only see the Prova if it is managed by his Agentes
        /// Agentes can only see the Prova if it is managed by themselves
        /// Cliente can only see the Prova if it is his own.
        /// </summary>
        /// <returns>DadoClinico List, possibly empty</returns>
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
                    if (_db.Provas != null)
                    {
                        var objList = (from prova in _db.Provas
                                       select new { prova.Id, prova.Conteudo, prova.DataSubmissao, prova.Sinistro }).ToList();

                        
                        _logger.SetLogInfoGetAll(_appUtils.GetUserId(token), "Prova");

                        return Ok(objList);
                    }
                    else return NotFound();
                case Roles.Gestor:
                    var equipaId = _appUtils.GetEquipaId(userId);
                    if (equipaId == null) return BadRequest();
                    var provaSinV = (from prova in _db.Provas
                                     join sinistro in _db.Sinistros on prova.SinistroId equals sinistro.Id
                                     join sinistroVeiculo in _db.SinistroVeiculos on sinistro.Id equals sinistroVeiculo.SinistroId
                                     join apoliceVeiculo in _db.ApoliceVeiculos on sinistroVeiculo.ApoliceVeiculoId equals apoliceVeiculo.Id
                                     join apolice in _db.Apolices on apoliceVeiculo.ApoliceId equals apolice.Id
                                     join agente in _db.Agentes on apolice.AgenteId equals agente.Id
                                     where agente.EquipaId == equipaId
                                     select new { prova.Id, prova.Conteudo, prova.DataSubmissao, prova.Sinistro }).ToList();
                    var provaSinP = (from prova in _db.Provas
                                     join sinistro in _db.Sinistros on prova.SinistroId equals sinistro.Id
                                     join sinistroPessoal in _db.SinistroPessoals on sinistro.Id equals sinistroPessoal.SinistroId
                                     join apolicePessoal in _db.ApolicePessoals on sinistroPessoal.ApolicePessoalId equals apolicePessoal.Id
                                     join apolice in _db.Apolices on apolicePessoal.ApoliceId equals apolice.Id
                                     join agente in _db.Agentes on apolice.AgenteId equals agente.Id
                                     where agente.EquipaId == equipaId
                                     select new { prova.Id, prova.Conteudo, prova.DataSubmissao, prova.Sinistro }).ToList();
                    provaSinP.ForEach(item => provaSinV.Add(item));

                    _logger.SetLogInfoGetAll(_appUtils.GetUserId(token), "Prova");
                    return Ok(provaSinV);

                case Roles.Agente:
                    int? agenteId = _appUtils.GetAgenteId(userId);
                    if (agenteId == null) return BadRequest();
                    var provaAgenteSinV = (from prova in _db.Provas
                                           join sinistro in _db.Sinistros on prova.SinistroId equals sinistro.Id
                                           join sinistroVeiculo in _db.SinistroVeiculos on sinistro.Id equals sinistroVeiculo.SinistroId
                                           join apoliceVeiculo in _db.ApoliceVeiculos on sinistroVeiculo.ApoliceVeiculoId equals apoliceVeiculo.Id
                                           join apolice in _db.Apolices on apoliceVeiculo.ApoliceId equals apolice.Id
                                           where apolice.AgenteId == agenteId
                                           select new { prova.Id, prova.Conteudo, prova.DataSubmissao, prova.Sinistro }).ToList();
                    var provaAgenteSinP = (from prova in _db.Provas
                                           join sinistro in _db.Sinistros on prova.SinistroId equals sinistro.Id
                                           join sinistroPessoal in _db.SinistroPessoals on sinistro.Id equals sinistroPessoal.SinistroId
                                           join apolicePessoal in _db.ApolicePessoals on sinistroPessoal.ApolicePessoalId equals apolicePessoal.Id
                                           join apolice in _db.Apolices on apolicePessoal.ApoliceId equals apolice.Id
                                           where apolice.AgenteId == agenteId
                                           select new { prova.Id, prova.Conteudo, prova.DataSubmissao, prova.Sinistro }).ToList();
                    provaAgenteSinP.ForEach(item => provaAgenteSinV.Add(item));

                    _logger.SetLogInfoGetAll(_appUtils.GetUserId(token), "Prova");
                    return Ok(provaAgenteSinV);

                case Roles.Cliente:
                    int? clienteId = _appUtils.GetClienteId(userId);
                    if (clienteId == null) return BadRequest();
                    var provaClienteSinV = (from prova in _db.Provas
                                           join sinistro in _db.Sinistros on prova.SinistroId equals sinistro.Id
                                           join sinistroVeiculo in _db.SinistroVeiculos on sinistro.Id equals sinistroVeiculo.SinistroId
                                           join apoliceVeiculo in _db.ApoliceVeiculos on sinistroVeiculo.ApoliceVeiculoId equals apoliceVeiculo.Id
                                           join veiculo in _db.Veiculos on apoliceVeiculo.VeiculoId equals veiculo.Id
                                           where veiculo.ClienteId == clienteId
                                           select new { prova.Id, prova.Conteudo, prova.DataSubmissao, prova.Sinistro }).ToList();
                    var provaClienteSinP = (from prova in _db.Provas
                                           join sinistro in _db.Sinistros on prova.SinistroId equals sinistro.Id
                                           join sinistroPessoal in _db.SinistroPessoals on sinistro.Id equals sinistroPessoal.SinistroId
                                           join apolicePessoal in _db.ApolicePessoals on sinistroPessoal.ApolicePessoalId equals apolicePessoal.Id                                           
                                           where apolicePessoal.ClienteId == clienteId
                                           select new { prova.Id, prova.Conteudo, prova.DataSubmissao, prova.Sinistro }).ToList();
                    provaClienteSinP.ForEach(item => provaClienteSinV.Add(item));

                    _logger.SetLogInfoGetAll(_appUtils.GetUserId(token), "Prova");
                    return Ok(provaClienteSinV);
            }
            return BadRequest();
        }

        /// <summary>
        /// Prova IndexById Route
        /// This route can only be accessed by authenticad users, any role can access it.
        /// The returned content depends on the role of the user and his permissions to view.
        /// Admins can see any Prova.
        /// Gestores can only see the Prova if it is managed by his Agentes
        /// Agentes can only see the Prova if it is managed by themselves
        /// Cliente can only see the Prova if it is his own.
        /// </summary>
        /// <param name="Id">ProvaId to get</param>
        /// <returns>Prova List, size one or zero</returns>
        [HttpGet("{Id}"), Auth]
        public IActionResult ViewById(int? Id)
        {
            var token = Request.Headers.Authorization[0].Replace("Bearer ", "");
            string bearer = Request.Headers.Authorization[0].Replace("Bearer ", "");
            int? userId = _appUtils.GetUserId(bearer);
            var userRole = _appUtils.GetUserRole(bearer);
            if (userId == null || userRole == null) return BadRequest();
            if (Id == null || Id == 0)
            {
                return NotFound();
            }
            switch (userRole)
            {
                case Roles.Admin:
                    if (_db.Provas != null)
                    {
                        var objList = (from prova in _db.Provas
                                       select new { prova.Id, prova.Conteudo, prova.DataSubmissao, prova.Sinistro }).ToList();

                        _logger.SetLogInfoGet(_appUtils.GetUserId(token), "Prova", Id);
                        return Ok(objList);
                    }
                    else return NotFound();
                case Roles.Gestor:
                    var equipaId = _appUtils.GetEquipaId(userId);
                    if (equipaId == null) return BadRequest();
                    var provaSinV = (from prova in _db.Provas
                                     join sinistro in _db.Sinistros on prova.SinistroId equals sinistro.Id
                                     join sinistroVeiculo in _db.SinistroVeiculos on sinistro.Id equals sinistroVeiculo.SinistroId
                                     join apoliceVeiculo in _db.ApoliceVeiculos on sinistroVeiculo.ApoliceVeiculoId equals apoliceVeiculo.Id
                                     join apolice in _db.Apolices on apoliceVeiculo.ApoliceId equals apolice.Id
                                     join agente in _db.Agentes on apolice.AgenteId equals agente.Id
                                     where agente.EquipaId == equipaId && prova.Id == Id
                                     select new { prova.Id, prova.Conteudo, prova.DataSubmissao, prova.Sinistro }).ToList();
                    var provaSinP = (from prova in _db.Provas
                                     join sinistro in _db.Sinistros on prova.SinistroId equals sinistro.Id
                                     join sinistroPessoal in _db.SinistroPessoals on sinistro.Id equals sinistroPessoal.SinistroId
                                     join apolicePessoal in _db.ApolicePessoals on sinistroPessoal.ApolicePessoalId equals apolicePessoal.Id
                                     join apolice in _db.Apolices on apolicePessoal.ApoliceId equals apolice.Id
                                     join agente in _db.Agentes on apolice.AgenteId equals agente.Id
                                     where agente.EquipaId == equipaId && prova.Id == Id
                                     select new { prova.Id, prova.Conteudo, prova.DataSubmissao, prova.Sinistro }).ToList();
                    provaSinP.ForEach(item => provaSinV.Add(item));

                    _logger.SetLogInfoGet(_appUtils.GetUserId(token), "Prova", Id);
                    return Ok(provaSinV);

                case Roles.Agente:
                    int? agenteId = _appUtils.GetAgenteId(userId);
                    if (agenteId == null) return BadRequest();
                    var provaAgenteSinV = (from prova in _db.Provas
                                           join sinistro in _db.Sinistros on prova.SinistroId equals sinistro.Id
                                           join sinistroVeiculo in _db.SinistroVeiculos on sinistro.Id equals sinistroVeiculo.SinistroId
                                           join apoliceVeiculo in _db.ApoliceVeiculos on sinistroVeiculo.ApoliceVeiculoId equals apoliceVeiculo.Id
                                           join apolice in _db.Apolices on apoliceVeiculo.ApoliceId equals apolice.Id
                                           where apolice.AgenteId == agenteId && prova.Id == Id
                                           select new { prova.Id, prova.Conteudo, prova.DataSubmissao, prova.Sinistro }).ToList();
                    var provaAgenteSinP = (from prova in _db.Provas
                                           join sinistro in _db.Sinistros on prova.SinistroId equals sinistro.Id
                                           join sinistroPessoal in _db.SinistroPessoals on sinistro.Id equals sinistroPessoal.SinistroId
                                           join apolicePessoal in _db.ApolicePessoals on sinistroPessoal.ApolicePessoalId equals apolicePessoal.Id
                                           join apolice in _db.Apolices on apolicePessoal.ApoliceId equals apolice.Id
                                           where apolice.AgenteId == agenteId && prova.Id == Id
                                           select new { prova.Id, prova.Conteudo, prova.DataSubmissao, prova.Sinistro }).ToList();
                    provaAgenteSinP.ForEach(item => provaAgenteSinV.Add(item));

                    _logger.SetLogInfoGet(_appUtils.GetUserId(token), "Prova", Id);
                    return Ok(provaAgenteSinV);

                case Roles.Cliente:
                    int? clienteId = _appUtils.GetClienteId(userId);
                    if (clienteId == null) return BadRequest();
                    var provaClienteSinV = (from prova in _db.Provas
                                            join sinistro in _db.Sinistros on prova.SinistroId equals sinistro.Id
                                            join sinistroVeiculo in _db.SinistroVeiculos on sinistro.Id equals sinistroVeiculo.SinistroId
                                            join apoliceVeiculo in _db.ApoliceVeiculos on sinistroVeiculo.ApoliceVeiculoId equals apoliceVeiculo.Id
                                            join veiculo in _db.Veiculos on apoliceVeiculo.VeiculoId equals veiculo.Id
                                            where veiculo.ClienteId == clienteId && prova.Id == Id
                                            select new { prova.Id, prova.Conteudo, prova.DataSubmissao, prova.Sinistro }).ToList();
                    var provaClienteSinP = (from prova in _db.Provas
                                            join sinistro in _db.Sinistros on prova.SinistroId equals sinistro.Id
                                            join sinistroPessoal in _db.SinistroPessoals on sinistro.Id equals sinistroPessoal.SinistroId
                                            join apolicePessoal in _db.ApolicePessoals on sinistroPessoal.ApolicePessoalId equals apolicePessoal.Id
                                            where apolicePessoal.ClienteId == clienteId && prova.Id == Id
                                            select new { prova.Id, prova.Conteudo, prova.DataSubmissao, prova.Sinistro }).ToList();
                    provaClienteSinP.ForEach(item => provaClienteSinV.Add(item));

                    _logger.SetLogInfoGet(_appUtils.GetUserId(token), "Prova", Id);
                    return Ok(provaClienteSinV);
            }
            return BadRequest();
        }

        /// <summary>
        /// Prova Post Route
        /// This route can only be accessed by authenticad users, any role can access it.
        /// The returned content depends on the role of the user and his permissions to access.
        /// Admins can create any Prova.
        /// Gestores can only create the Prova if it is managed by his Agentes
        /// Agentes can only create the Prova if it is managed by themselves
        /// Cliente can only create the Prova if it is his own.
        /// </summary>
        /// <param name="obj">Prova object</param>
        /// <returns>Created Prova if create is successful, if not, return NotFound</returns>
        [HttpPost, Auth]
        public IActionResult Create(Prova obj)
        {
            var token = Request.Headers.Authorization[0].Replace("Bearer ", "");
            string bearer = Request.Headers.Authorization[0].Replace("Bearer ", "");
            int? userId = _appUtils.GetUserId(bearer);
            var userRole = _appUtils.GetUserRole(bearer);
            if (userId == null || userRole == null) return BadRequest();
            if (ModelState.IsValid)
            {


                //Initializes response variables
                var objectUtils = new Prova();
                var responseUtils = string.Empty;

                switch (userRole)
                {
                    case Roles.Admin:
                        if (_db.Provas != null)
                        {
                            //Calls function from utils
                            (objectUtils, responseUtils) = _prUtils.CreateProva(obj);

                            if (objectUtils == null) return BadRequest(error: responseUtils);

                            var json = JsonConvert.SerializeObject(objectUtils, new JsonSerializerSettings() { MaxDepth = 1, ReferenceLoopHandling = ReferenceLoopHandling.Ignore });
                            _logger.SetLogInfoPost(_appUtils.GetUserId(token), "Prova", json);

                            return Ok(objectUtils);
                        }
                        else return NotFound();
                    case Roles.Gestor:
                        var equipaId = _appUtils.GetEquipaId(userId);
                        if (equipaId == null) return BadRequest();

                        var auxGestorObj = _db.Sinistros.Find(obj.SinistroId);

                        var sinPessoal = auxGestorObj.SinistroPessoals.ToList().FirstOrDefault();
                        var sinPessoalEquipaId = sinPessoal.ApolicePessoal.Apolice.Agente.EquipaId;
                        var sinVeiculo = auxGestorObj.SinistroVeiculos.ToList().FirstOrDefault();
                        var sinVeiculoEquipaId = sinVeiculo.ApoliceVeiculo.Apolice.Agente.EquipaId;

                        if (sinPessoalEquipaId == equipaId || sinVeiculoEquipaId == equipaId)
                        {
                            //Calls function from utils
                            (objectUtils, responseUtils) = _prUtils.CreateProva(obj);

                            if (objectUtils == null) return BadRequest(error: responseUtils);

                            var json = JsonConvert.SerializeObject(objectUtils, new JsonSerializerSettings() { MaxDepth = 1, ReferenceLoopHandling = ReferenceLoopHandling.Ignore });
                            _logger.SetLogInfoPost(_appUtils.GetUserId(token), "Prova", json);
                            return Ok(objectUtils);
                        }
                        return BadRequest();
                    case Roles.Agente:
                        int? agenteId = _appUtils.GetAgenteId(userId);
                        if (agenteId == null) return BadRequest();

                        var auxAgenteObj = _db.Sinistros.Find(obj.SinistroId);

                        var sinPessoalAgente = auxAgenteObj.SinistroPessoals.ToList().FirstOrDefault();
                        var sinPessoalAgenteId = sinPessoalAgente.ApolicePessoal.Apolice.Agente.EquipaId;
                        var sinVeiculoAgente = auxAgenteObj.SinistroVeiculos.ToList().FirstOrDefault();
                        var sinVeiculoAgenteId = sinVeiculoAgente.ApoliceVeiculo.Apolice.Agente.EquipaId;

                        if (sinPessoalAgenteId == agenteId || sinVeiculoAgenteId == agenteId)
                        {
                            //Calls function from utils
                            (objectUtils, responseUtils) = _prUtils.CreateProva(obj);

                            if (objectUtils == null) return BadRequest(error: responseUtils);

                            var json = JsonConvert.SerializeObject(objectUtils, new JsonSerializerSettings() { MaxDepth = 1, ReferenceLoopHandling = ReferenceLoopHandling.Ignore });
                            _logger.SetLogInfoPost(_appUtils.GetUserId(token), "Prova", json);
                            return Ok(objectUtils);
                        }
                        return BadRequest();
                    case Roles.Cliente:
                        int? clienteId = _appUtils.GetClienteId(userId);
                        if (clienteId == null) return BadRequest();

                        var auxClienteObj = _db.Sinistros.Find(obj.SinistroId);

                        var sinPessoalCliente = auxClienteObj.SinistroPessoals.ToList().FirstOrDefault();
                        var sinPessoalClienteId = sinPessoalCliente.ApolicePessoal.ClienteId;
                        var sinVeiculoCliente = auxClienteObj.SinistroVeiculos.ToList().FirstOrDefault();
                        var sinVeiculoClienteId = sinVeiculoCliente.ApoliceVeiculo.Veiculo.ClienteId;
                        if (sinPessoalClienteId == clienteId || sinVeiculoClienteId == clienteId)
                        {
                            //Calls function from utils
                            (objectUtils, responseUtils) = _prUtils.CreateProva(obj);

                            if (objectUtils == null) return BadRequest(error: responseUtils);

                            var json = JsonConvert.SerializeObject(objectUtils, new JsonSerializerSettings() { MaxDepth = 1, ReferenceLoopHandling = ReferenceLoopHandling.Ignore });
                            _logger.SetLogInfoPost(_appUtils.GetUserId(token), "Prova", json);
                            return Ok(objectUtils);
                        }
                        return BadRequest();
                }                
            }
            return View(obj);
        }

        /// <summary>
        /// Prova Delete Route
        /// This route can only be accessed by authenticad users, only admin role can access it.
        /// Admins can delete any Prova.
        /// </summary>
        /// <param name="Id">ProvaId te remove</param>
        /// <returns>Returns object</returns>
        [HttpDelete, Auth(Roles.Admin)]
        public IActionResult Delete(int Id)
        {
            var obj = _db.Provas.Find(Id);
            if (obj == null)
            {
                return NotFound();
            }
            _db.Provas.Remove(obj);
            _db.SaveChanges();

            var token = Request.Headers.Authorization[0].Replace("Bearer ", "");
            _logger.SetLogInfoDelete(_appUtils.GetUserId(token), "Prova", Id);

            return Ok(obj);
        }
    }
}
