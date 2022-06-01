using Microsoft.AspNetCore.Authorization;
using Auth = ManaLynxAPI.Authentication.Auth;
using Roles = ManaLynxAPI.Models.Roles;
using System.IdentityModel.Tokens.Jwt;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Mvc;
using ManaLynxAPI.Data;
using ManaLynxAPI.Models;
using ManaLynxAPI.Utils;
using Newtonsoft.Json;

namespace ManaLynxAPI.Controllers
{
    [ApiController, Route("[controller]/[action]")]
    public class ApoliceSaudeController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly IAppUtils _app;
        private readonly IApoliceUtils _apolice;
        private readonly IPagamentoUtils _pagamento;
        private readonly ILoggerUtils _logger;

        public ApoliceSaudeController(ApplicationDbContext db, IAppUtils app, IPagamentoUtils pagamento, ILoggerUtils logger, IApoliceUtils apolice)
        {
            _db = db;
            _app = app;
            _pagamento = pagamento;
            _app = app;
            _logger = logger;
            _apolice = apolice;
        }

        #region Apolice
        /// <summary>
        /// ApoliceSaude index Route
        /// This route can only be accessed by authenticad users, any role can access it.
        /// The returned content depends on the role of the user and his permissions to view.
        /// Admins can see all the data in the database.
        /// Gestores can only see ApoliceSaude managed by his Agentes
        /// Agentes can only see ApoliceSaude managed by himselves
        /// Cliente can only see his ApolicesPessoais
        /// </summary>
        /// <returns>ApoliceSaude List, possibly empty</returns>
        [HttpGet, Auth(Roles.Admin, Roles.Gestor, Roles.Agente, Roles.Cliente)]
        public IActionResult Index()
        {
            string bearer = Request.Headers.Authorization[0].Replace("Bearer ", "");
            int? userId = _app.GetUserId(bearer);
            var userRole = _app.GetUserRole(bearer);
            if (userId == null || userRole == null) return BadRequest("Invalid User");

            switch (userRole)
            {
                case Roles.Admin:
                    if (_db.ApoliceSaudes != null)
                    {
                        var objApoliceSaudesList = (from apSaude in _db.ApoliceSaudes
                                                    select new { apSaude.Id, apSaude.Apolice, apSaude.Cliente }).ToList();

                        var token = Request.Headers.Authorization[0].Replace("Bearer ", "");
                        _logger.SetLogInfoGetAll(_app.GetUserId(token), "ApoliceSaude");

                        return Ok(objApoliceSaudesList);
                    }
                    else return NotFound();

                case Roles.Gestor:
                    int? equipaId = _app.GetEquipaId(userId);
                    if (equipaId == null) return BadRequest("Invalid Gestor");

                    var apolicesSaudes = (from apSaude in _db.ApoliceSaudes
                                          join apolice in _db.Apolices on apSaude.ApoliceId equals apolice.Id
                                          join agente in _db.Agentes on apolice.AgenteId equals agente.Id
                                          where agente.EquipaId == equipaId
                                          select new { apSaude.Id, apSaude.Apolice, apSaude.Cliente }).ToList();

                    _logger.SetLogInfoGetAll(userId, "ApoliceSaude");

                    return Ok(apolicesSaudes);

                case Roles.Agente:
                    int? agenteId = _app.GetAgenteId(userId);
                    if (agenteId == null) return BadRequest("Invalid Agente");

                    var apolicesAgenteSaudes = (from apSaude in _db.ApoliceSaudes
                                                join apolice in _db.Apolices on apSaude.ApoliceId equals apolice.Id
                                                where apolice.AgenteId == agenteId
                                                select new { apSaude.Id, apSaude.Apolice, apSaude.Cliente }).ToList();

                    _logger.SetLogInfoGetAll(userId, "ApoliceSaude");

                    return Ok(apolicesAgenteSaudes);

                case Roles.Cliente:
                    int? clienteId = _app.GetClienteId(userId);
                    if (clienteId == null) return BadRequest("Invalid Cliente");

                    var apolicesClienteSaudes = (from apSaude in _db.ApoliceSaudes
                                                 where apSaude.ClienteId == clienteId
                                                 select new { apSaude.Id, apSaude.Apolice, apSaude.Cliente }).ToList();

                    _logger.SetLogInfoGetAll(userId, "ApoliceSaude");

                    return Ok(apolicesClienteSaudes);
                default: return BadRequest();
            }
        }

        /// <summary>
        /// ApoliceSaude Create Route
        /// This route can only be accessed by authenticad users, any role can access it.
        /// The creation depends on the role of the user and his permissions to create.
        /// Admins can create any ApolicePessoal.
        /// Gestores can only create the ApoliceSaude if it Apolice is managed by himselves
        /// Agentes can only create the ApoliceSaude if it Apolice is managed by himselves
        /// Cliente can only create the ApoliceSaude if it is his own.
        /// </summary>
        /// <param name="obj">ApoliceSaude with Apolice incapsulated Object</param>
        /// <returns>If sucessfull return created ApoliceSaudeId</returns>
        [HttpPost, Auth(Roles.Admin, Roles.Gestor, Roles.Agente, Roles.Cliente)]
        public IActionResult Create(ApoliceSaude obj)
        {
            string bearer = Request.Headers.Authorization[0].Replace("Bearer ", "");
            int? userId = _app.GetUserId(bearer);
            var userRole = _app.GetUserRole(bearer);
            if (userId == null || userRole == null) return BadRequest("Invalid user");

            if (!ModelState.IsValid) return BadRequest("Invalid object");
            if (obj.Apolice == null) return BadRequest("Invalid Apolice");

            var createAP = new ApoliceSaude();

            string errorMessage;

            switch (userRole)
            {
                case Roles.Admin:
                    (errorMessage,createAP) =_apolice.CreateApoliceSaude(createAP, obj);
                    if (errorMessage != "") return BadRequest(error: errorMessage);

                    return Ok(createAP);

                case Roles.Gestor:
                case Roles.Agente:
                    int? agenteId = _app.GetAgenteId(userId);
                    if (agenteId == null) return BadRequest("Invalid Agente/Gestor");

                    if (obj.Apolice.AgenteId != agenteId) return BadRequest("Permission Denied");

                    (errorMessage, createAP) = _apolice.CreateApoliceSaude(createAP, obj);
                    if (errorMessage != "") return BadRequest(error: errorMessage);

                    return Ok(createAP);

                case Roles.Cliente:
                    int? clienteId = _app.GetClienteId(userId);
                    if (clienteId == null) return BadRequest("Invalid Cliente");

                    if (obj.ClienteId != clienteId) return BadRequest("Permission Denied");
                    //ensure that apolice has no agente
                    obj.Apolice.AgenteId = null;

                    (errorMessage, createAP) = _apolice.CreateApoliceSaude(createAP, obj);
                    if (errorMessage != "") return BadRequest(error: errorMessage);

                    return Ok(createAP);

                default: return BadRequest();
            }
        }

        /// <summary>
        /// ApoliceSaude Get by Id Route
        /// This route can only be accessed by authenticad users, any role can access it.
        /// The returned content depends on the role of the user and his permissions to view.
        /// Admins can see any ApolicePessoal.
        /// Gestores can only see the ApoliceSaude if it Apolice is managed by his Agentes
        /// Agentes can only see the ApoliceSaude if it Apolice is managed by himselves
        /// Cliente can only see the ApoliceSaude if it is his own.
        /// </summary>
        /// <param name="Id">ApolicePessoalId to get</param>
        /// <returns>ApoliceSaude List, size one or zero</returns>
        [HttpGet("{Id}"), Auth(Roles.Admin, Roles.Gestor, Roles.Agente, Roles.Cliente)]
        public IActionResult ViewById(int Id)
        {
            string bearer = Request.Headers.Authorization[0].Replace("Bearer ", "");
            int? userId = _app.GetUserId(bearer);
            var userRole = _app.GetUserRole(bearer);
            if (userId == null || userRole == null) return BadRequest("Invalid User");

            switch (userRole)
            {
                case Roles.Admin:
                    if (_db.ApolicePessoals != null)
                    {
                        var ApoliceSaude = (from apSaude in _db.ApoliceSaudes
                                            where apSaude.Id == Id
                                            select new { apSaude.Id, apSaude.Apolice, apSaude.Cliente }).ToList().FirstOrDefault();

                        _logger.SetLogInfoGetAll(userId, "ApoliceSaude");

                        return Ok(ApoliceSaude);
                    }
                    else return NotFound();

                case Roles.Gestor:
                    int? equipaId = _app.GetEquipaId(userId);
                    if (equipaId == null) return BadRequest("Invalid Gestor");

                    var apolicegestorSaude = (from apSaude in _db.ApoliceSaudes
                                              join apolice in _db.Apolices on apSaude.ApoliceId equals apolice.Id
                                              join agente in _db.Agentes on apolice.AgenteId equals agente.Id
                                              where apSaude.Id == Id && agente.EquipaId == equipaId
                                              select new { apSaude.Id, apSaude.Apolice, apSaude.Cliente }).ToList().FirstOrDefault();

                    _logger.SetLogInfoGetAll(userId, "ApoliceSaude");

                    return Ok(apolicegestorSaude);

                case Roles.Agente:
                    int? agenteId = _app.GetAgenteId(userId);
                    if (agenteId == null) return BadRequest("Invalid Agente");

                    var apoliceAgenteSaude = (from apSaude in _db.ApoliceSaudes
                                              join apolice in _db.Apolices on apSaude.ApoliceId equals apolice.Id
                                              where apSaude.Id == Id && apolice.AgenteId == agenteId
                                              select new { apSaude.Id, apSaude.Apolice, apSaude.Cliente }).ToList().FirstOrDefault();

                    _logger.SetLogInfoGetAll(userId, "ApoliceSaude");

                    return Ok(apoliceAgenteSaude);

                case Roles.Cliente:
                    int? clienteId = _app.GetClienteId(userId);
                    if (clienteId == null) return BadRequest("Invalid Cliente");

                    var apoliceClienteSaude = (from apSaude in _db.ApoliceSaudes
                                               where apSaude.Id == Id && apSaude.ClienteId == clienteId
                                               select new { apSaude.Id, apSaude.Apolice, apSaude.Cliente }).ToList().FirstOrDefault();

                    _logger.SetLogInfoGetAll(userId, "ApoliceSaude");

                    return Ok(apoliceClienteSaude);
                default: return BadRequest();
            }
        }
        #endregion

        #region Simulacao

        public class ApoliceSaudeActions
        {
            public int ApoliceSaudeId { get; set; }
        }

        [HttpPost, Auth]
        public IActionResult AskSimulacao(ApoliceSaude apoliceSaude, int seguroId)
        {
            if (!ModelState.IsValid) return BadRequest("ModelState Is Not Valid");
            var token = Request.Headers.Authorization[0].Replace("Bearer ", "");
            var reqId = _app.GetUserId(token);
            var role = _app.GetUserRole(token);
            if (reqId is null || role is null) return BadRequest();
            // Request User
            var user = _db.ManaUsers.Find(reqId);
            if (user is null) return BadRequest();
            // Request Apolice
            var apolice = _db.Apolices.Find(apoliceSaude.ApoliceId);
            if (apolice is not null) return BadRequest();
            // Request Seguro
            var seguro = _db.Seguros.Find(seguroId);
            if (seguro is null) return BadRequest();

            apoliceSaude.Transacaos.Clear();

            // Apolice
            if (role == Roles.Admin)
            {
                apolice = apoliceSaude.Apolice;
                if (apolice is null) return BadRequest();
                apolice = _apolice.Clear(apolice);
            }
            else
            {
                apolice = new Apolice
                {
                    Ativa = false,
                    Validade = DateTime.UtcNow.AddDays(-1),
                    SeguroId = seguroId
                };
            }
            apoliceSaude.Apolice = null;

            // Request Cliente
            Cliente? cliente = _db.Clientes.Find(apoliceSaude.ClienteId);
            int? agenteId = _app.GetAgenteId(reqId);

            switch (role)
            {
                case Roles.Admin:
                    apolice = _db.Apolices.Add(apolice).Entity;
                    if (_db.SaveChanges() == 0) return BadRequest();
                    break;

                case Roles.Gestor:
                    if (agenteId is null) return BadRequest("You are not agente");
                    if (_db.Gestors.Any(g => g.AgenteId == agenteId) is false) return BadRequest("You are not gestor");
                    var equipa = _db.Agentes.Where(a => a.Id == agenteId).Select(a => a.Equipa).FirstOrDefault();
                    if (equipa is null) return BadRequest();
                    if (cliente is null) return BadRequest("There is no cliente of the selected vehicle");
                    if (cliente.Agente is not null)
                    {
                        if (equipa.Agentes.Contains(cliente.Agente))
                            goto case Roles.Agente;
                    }
                    else return BadRequest("Cliente has no `Agente`");
                    return BadRequest();

                case Roles.Agente:
                    if (cliente is null || cliente.IsLead == 0) return BadRequest();
                    if (cliente.Agente is not null && cliente.AgenteId != agenteId)
                    {
                        if (cliente.Agente.Equipa is not null && !cliente.Agente.Equipa.Agentes.Any(a => a.Id == agenteId))
                            return BadRequest();
                    }
                    apolice.Simulacao = SimulacaoState.Validada.ToString();
                    apolice.AgenteId = agenteId;
                    apolice = _db.Apolices.Add(apolice).Entity;
                    break;

                case Roles.Cliente:
                    cliente = _db.Clientes.Where(c => c.PessoaId == user.PessoaId).FirstOrDefault();
                    if (cliente is null) return BadRequest();
                    //apolice.Simulacao = SimulacaoState.NaoValidada.ToString();
                    apolice.Simulacao = "Não Validada";
                    apolice = _db.Apolices.Add(apolice).Entity;
                    if (_db.SaveChanges() == 0) return BadRequest();
                    break;
            }

            if (cliente is null) return BadRequest();
            apoliceSaude.ApoliceId = apolice.Id;
            apoliceSaude.ClienteId = cliente.Id;
            _db.ApoliceSaudes.Add(apoliceSaude);
            _db.SaveChanges();
            apolice = _apolice.Clear(apolice);
            apoliceSaude.Cliente = null;
            apoliceSaude.Apolice = null;
            apoliceSaude.Transacaos.Clear();
            return Ok(new object[] { apoliceSaude, apolice });
        }

        [HttpPost, Auth(Roles.Admin, Roles.Gestor, Roles.Agente)]
        public IActionResult ValidateSimulacao([FromBody] ApoliceSaudeActions obj)
        {
            int apoliceSaudeId = obj.ApoliceSaudeId;
            IActionResult result = BadRequest();
            if (!ModelState.IsValid) return BadRequest("ModelState Is Not Valid");
            var token = Request.Headers.Authorization[0].Replace("Bearer ", "");
            var reqId = _app.GetUserId(token);
            var role = _app.GetUserRole(token);
            if (reqId is null || role is null) return BadRequest();
            // Request User
            var user = _db.ManaUsers.Find(reqId);
            if (user is null) return BadRequest();
            // Request Agente
            var agente = _db.Agentes.Where(a => a.PessoaId == user.PessoaId).FirstOrDefault();
            // Request ApoliceVeiculo
            var apoliceSaude = _db.ApoliceSaudes.Find(apoliceSaudeId);
            if (apoliceSaude is null) return BadRequest("Apolice Saude not found!");

            var cliente = _db.Clientes.Where(v => v.Id == apoliceSaude.ClienteId).FirstOrDefault();
            if (cliente is null) return BadRequest("veículo has no cliente associated");

            var apolice = _db.Apolices.Find(apoliceSaude.ApoliceId);
            if (apolice is null) return BadRequest("apolice does not exist");

            if (apolice.Simulacao is not null &&
                !apolice.Simulacao.Equals("Não Validada")) return BadRequest();
            apolice.Simulacao = SimulacaoState.Validada.ToString();

            switch (role)
            {
                case Roles.Admin:
                    _db.Apolices.Update(apolice);
                    _db.ApoliceSaudes.Update(apoliceSaude);
                    result = Ok(); break;

                case Roles.Gestor:
                    if (agente is null) return BadRequest("You are not agente");
                    if (_db.Gestors.Any(g => g.AgenteId == agente.Id) is false) return BadRequest("You are not gestor");
                    var equipa = _db.Agentes.Where(a => a.Id == agente.Id).Select(a => a.Equipa).FirstOrDefault();
                    if (equipa is null) return BadRequest("You don't have a team");
                    if (cliente.Agente is not null)
                    {
                        if (equipa.Agentes.Contains(cliente.Agente))
                            goto case Roles.Agente;
                    }
                    else if (cliente.IsLead == 1)
                        goto case Roles.Agente;
                    else return BadRequest("Cliente has no `Agente`");
                    result = BadRequest("You don't have access to this team."); break;
                case Roles.Agente:
                    if (agente is null) return BadRequest("You are not agente");
                    if (role == Roles.Agente && apolice.AgenteId is not null && apolice.AgenteId != 0)
                        apolice.AgenteId = agente.Id;
                    apolice = _db.Apolices.Update(apolice).Entity;
                    _db.ApoliceSaudes.Update(apoliceSaude);
                    result = Ok(); break;
            }

            _db.SaveChanges();
            return result;
        }

        [HttpPost, Auth]
        public IActionResult AcceptSimulacao([FromBody] ApoliceSaudeActions obj )
        {
            int apoliceSaudeId = obj.ApoliceSaudeId;
            //IActionResult result = BadRequest();
            var token = Request.Headers.Authorization[0].Replace("Bearer ", "");
            var reqId = _app.GetUserId(token);
            var role = _app.GetUserRole(token);
            if (reqId is null || role is null) return BadRequest();
            // Request User
            var user = _db.ManaUsers.Find(reqId);
            if (user is null) return BadRequest();
            // Request Agente
            var agente = _db.Agentes.Where(a => a.PessoaId == user.PessoaId).FirstOrDefault();

            var apoliceSaude = _db.ApoliceSaudes.Find(apoliceSaudeId);
            if (apoliceSaude is null) return BadRequest();

            var apolice = _db.Apolices.Find(apoliceSaude.ApoliceId);
            if (apolice is null) return BadRequest();
            if (apolice.Fracionamento is null || apolice.Premio is null) return BadRequest("Something went wrong generating this apolice");

            Cliente? cliente = _db.Clientes.Find(apoliceSaude.ClienteId); ;

            switch (role)
            {
                case Roles.Gestor:
                    if (agente is null) return BadRequest("You are not agente");
                    if (_db.Gestors.Any(g => g.AgenteId == agente.Id) is false) return BadRequest("You are not gestor");
                    var equipa = _db.Agentes.Where(a => a.Id == agente.Id).Select(a => a.Equipa).FirstOrDefault();
                    if (equipa is null) return BadRequest("You don't have a team");
                    if (cliente is null) return BadRequest("No cliente found");
                    if (cliente.Agente is not null)
                    {
                        if (equipa.Agentes.Contains(cliente.Agente))
                            break;
                    }
                    else return BadRequest("Cliente has no `Agente`");
                    return BadRequest("You don't have access to this team.");
                case Roles.Agente:
                    if (agente is null) return BadRequest("You are not agente");
                    if (cliente is null) return BadRequest();
                    if (cliente.IsLead != 1) return BadRequest("You can't access this cliente");
                    if (apolice.AgenteId != agente.Id)
                    {
                        if (agente.Equipa is null || !agente.Equipa.Agentes.Any(a => a.Id == agente.Id))
                            return BadRequest();
                    }
                    break;
                case Roles.Cliente:
                    cliente = _db.Clientes.Where(c => c.PessoaId == user.PessoaId).FirstOrDefault();
                    if (cliente is null) return BadRequest();
                    break;
            }

            apolice.Ativa = false;
            apolice.Simulacao = "Pagamento Emitido";

            var pagamento = new Pagamento
            {
                ApoliceId = apolice.Id,
                DataEmissao = DateTime.Today,
                DataPagamento = DateTime.Parse("1901-01-01"),
                Metodo = "Cartão"
            };

            var fracionamento = _app.GetFracionamento(apolice.Fracionamento);
            double value;
            switch (fracionamento)
            {
                case Fracionamento.Anual:
                    value = apolice.Premio.Value / 1;
                    pagamento.Montante = Math.Round(value, 2);
                    apolice.Validade = DateTime.Today.AddMonths(12);
                    break;
                case Fracionamento.Semestral:
                    value = apolice.Premio.Value / 2;
                    pagamento.Montante = Math.Round(value, 2);
                    apolice.Validade = DateTime.Today.AddMonths(6);
                    break;
                case Fracionamento.Trimestral:
                    value = apolice.Premio.Value / 4;
                    pagamento.Montante = Math.Round(value, 2);
                    apolice.Validade = DateTime.Today.AddMonths(4);
                    break;
                case Fracionamento.Mensal:
                    value = apolice.Premio.Value / 12;
                    pagamento.Montante = Math.Round(value, 2);
                    apolice.Validade = DateTime.Today.AddMonths(1);
                    break;
                default:
                    return BadRequest("Fracionamento is invalid");
            }

            _db.Apolices.Update(apolice);
            if (_db.SaveChanges() == 0) return BadRequest("Changes were not saved. Abort!");

            _db.ApoliceSaudes.Update(apoliceSaude);
            if (_db.SaveChanges() == 0) return BadRequest("Changes were not saved. Abort!");

            // Add Pagamento
            if (!_pagamento.PagamentoExists(pagamento))
                _pagamento.AddPagamento(pagamento);
            else
                return BadRequest("There is a similar pagamento");
            return Ok("pagamento emitido");
        }

        [HttpPost, Auth]
        public IActionResult CancelarSimulacao([FromBody] ApoliceSaudeActions obj)
        {
            int apoliceSaudeId = obj.ApoliceSaudeId;
            //IActionResult result = BadRequest();
            var token = Request.Headers.Authorization[0].Replace("Bearer ", "");
            var reqId = _app.GetUserId(token);
            var role = _app.GetUserRole(token);
            if (reqId is null || role is null) return BadRequest();
            // Request User
            var user = _db.ManaUsers.Find(reqId);
            if (user is null) return BadRequest();
            // Request Agente
            var agente = _db.Agentes.Where(a => a.PessoaId == user.PessoaId).FirstOrDefault();

            var apoliceSaude = _db.ApoliceSaudes.Find(apoliceSaudeId);
            if (apoliceSaude is null) return BadRequest();

            var apolice = _db.Apolices.Find(apoliceSaude.ApoliceId);
            if (apolice is null) return BadRequest();
            if (apolice.Fracionamento is null || apolice.Premio is null) return BadRequest("Something went wrong generating this apolice");

            Cliente? cliente = _db.Clientes.Find(apoliceSaude.ClienteId); ;

            switch (role)
            {
                case Roles.Gestor:
                    if (agente is null) return BadRequest("You are not agente");
                    if (_db.Gestors.Any(g => g.AgenteId == agente.Id) is false) return BadRequest("You are not gestor");
                    var equipa = _db.Agentes.Where(a => a.Id == agente.Id).Select(a => a.Equipa).FirstOrDefault();
                    if (equipa is null) return BadRequest("You don't have a team");
                    if (cliente is null) return BadRequest("No cliente found");
                    if (cliente.Agente is not null)
                    {
                        if (equipa.Agentes.Contains(cliente.Agente))
                            break;
                    }
                    else return BadRequest("Cliente has no `Agente`");
                    return BadRequest("You don't have access to this team.");
                case Roles.Agente:
                    if (agente is null) return BadRequest("You are not agente");
                    if (cliente is null) return BadRequest();
                    if (cliente.IsLead != 1) return BadRequest("You can't access this cliente");
                    if (apolice.AgenteId != agente.Id)
                    {
                        if (agente.Equipa is null || !agente.Equipa.Agentes.Any(a => a.Id == agente.Id))
                            return BadRequest();
                    }
                    break;
                case Roles.Cliente:
                    cliente = _db.Clientes.Where(c => c.PessoaId == user.PessoaId).FirstOrDefault();
                    if (cliente is null) return BadRequest();
                    break;
            }

            apolice.Ativa = false;
            apolice.Simulacao = SimulacaoState.Cancelada.ToString();

            _db.Apolices.Update(apolice);
            if (_db.SaveChanges() == 0) return BadRequest("Changes were not saved. Abort!");

            _db.ApoliceSaudes.Update(apoliceSaude);
            if (_db.SaveChanges() == 0) return BadRequest("Changes were not saved. Abort!");

            return Ok("Apolice Cancelada");
        }

        /// <summary>
        /// Calculate value for Premio to be paid for an Apolice of a specific Cliente
        /// </summary>
        /// <param name="clienteId">Id for Cliente Information</param>
        /// <returns>Value to be paid for an ApoliceSaude</returns>
        [HttpGet, Auth]
        public IActionResult CalculatePremio(int clienteId)
        {
            var premio = _app.CalculateSaudePremio(clienteId);

            if (premio is null)
                return BadRequest("Something Went Wrong");
            return Ok(premio);
        }
        #endregion
    }
}
