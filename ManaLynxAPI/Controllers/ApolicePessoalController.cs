using Microsoft.AspNetCore.Mvc;
using ManaLynxAPI.Data;
using ManaLynxAPI.Models;
using ManaLynxAPI.Utils;
using Microsoft.AspNetCore.Authorization;
using Auth = ManaLynxAPI.Authentication.Auth;
using Roles = ManaLynxAPI.Models.Roles;
using System.IdentityModel.Tokens.Jwt;
using Newtonsoft.Json;

namespace ManaLynxAPI.Controllers
{
    [Authorize]
    [ApiController, Route("[controller]/[action]")]
    public class ApolicePessoalController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly ILoggerUtils _logger;
        private readonly IAppUtils _app;
        private readonly IPagamentoUtils _pagamento;
        private readonly IApoliceUtils _apolice;

        public ApolicePessoalController(ApplicationDbContext db, ILoggerUtils logger, IAppUtils app, IPagamentoUtils pagamento, IApoliceUtils apolice)
        {
            _db = db;
            _logger = logger;
            _app = app;
            _pagamento = pagamento;
            _apolice = apolice;
        }

        #region Apolice
        /// <summary>
        /// ApolicePessoal index Route
        /// This route can only be accessed by authenticad users, any role can access it.
        /// The returned content depends on the role of the user and his permissions to view.
        /// Admins can see all the data in the database.
        /// Gestores can only see ApolicesPessoais managed by his Agentes
        /// Agentes can only see ApolicesPessoais managed by himselves
        /// Cliente can only see his ApolicesPessoais
        /// </summary>
        /// <returns>ApolicesPessoais List, possibly empty</returns>
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
                    if (_db.ApolicePessoals != null)
                    {
                        var objApolicePessoalsList = (from apPessoal in _db.ApolicePessoals
                                                      select new { apPessoal.Id, apPessoal.Valor, apPessoal.Apolice, apPessoal.Cliente }).ToList();

                        _logger.SetLogInfoGetAll(userId, "ApolicePessoal");

                        return Ok(objApolicePessoalsList);
                    }
                    else return NotFound();

                case Roles.Gestor:
                    int? equipaId = _app.GetEquipaId(userId);
                    if (equipaId == null) return BadRequest("Invalid Gestor");

                    var apolicesPessoais = (from apPessoal in _db.ApolicePessoals
                                            join apolice in _db.Apolices on apPessoal.ApoliceId equals apolice.Id
                                            join agente in _db.Agentes on apolice.AgenteId equals agente.Id
                                            where agente.EquipaId == equipaId
                                            select new { apPessoal.Id, apPessoal.Valor, apPessoal.Apolice, apPessoal.Cliente }).ToList();

                    _logger.SetLogInfoGetAll(userId, "ApolicePessoal");

                    return Ok(apolicesPessoais);

                case Roles.Agente:
                    int? agenteId = _app.GetAgenteId(userId);
                    if (agenteId == null) return BadRequest("Invalid Agente");

                    var apolicesAgentePessoais = (from apPessoal in _db.ApolicePessoals
                                                  join apolice in _db.Apolices on apPessoal.ApoliceId equals apolice.Id
                                                  where apolice.AgenteId == agenteId
                                                  select new { apPessoal.Id, apPessoal.Valor, apPessoal.Apolice, apPessoal.Cliente }).ToList();

                    _logger.SetLogInfoGetAll(userId, "ApolicePessoal");

                    return Ok(apolicesAgentePessoais);

                case Roles.Cliente:
                    int? clienteId = _app.GetClienteId(userId);
                    if (clienteId == null) return BadRequest("Invalid Cliente");

                    var apolicesClientePessoais = (from apPessoal in _db.ApolicePessoals
                                                   where apPessoal.ClienteId == clienteId
                                                   select new { apPessoal.Id, apPessoal.Valor, apPessoal.Apolice, apPessoal.Cliente }).ToList();

                    _logger.SetLogInfoGetAll(userId, "ApolicePessoal");

                    return Ok(apolicesClientePessoais);
                default: return BadRequest();
            }
        }

        /// <summary>
        /// ApolicePessoal Create Route
        /// This route can only be accessed by authenticad users, any role can access it.
        /// The creation depends on the role of the user and his permissions to create.
        /// Admins can create any ApolicePessoal.
        /// Gestores can only create the ApolicePessoal if it Apolice is managed by himselves
        /// Agentes can only create the ApolicePessoal if it Apolice is managed by himselves
        /// Cliente can only create the ApolicePessoal if it is his own.
        /// </summary>
        /// <param name="obj">ApolicePessoal with Apolice incapsulated Object</param>
        /// <returns>If sucessfull return created ApolicePessoalId</returns>
        [HttpPost, Auth(Roles.Admin, Roles.Gestor, Roles.Agente, Roles.Cliente)]
        public IActionResult Create(ApolicePessoal obj)
        {
            string bearer = Request.Headers.Authorization[0].Replace("Bearer ", "");
            int? userId = _app.GetUserId(bearer);
            var userRole = _app.GetUserRole(bearer);
            if (userId == null || userRole == null) return BadRequest("Invalid user");

            if (!ModelState.IsValid) return BadRequest("Invalid object");

            ApolicePessoal createAP = new ApolicePessoal();
            string errorMessage;
            switch (userRole)
            {
                case Roles.Admin:

                    (errorMessage, createAP) = _apolice.CreateApolicePessoal(createAP, obj);
                    if (createAP == null) return BadRequest(error: errorMessage);

                    return Ok(createAP.Id);

                case Roles.Gestor:
                case Roles.Agente:
                    int? agenteId = _app.GetAgenteId(userId);
                    if (agenteId == null) return BadRequest("Invalid Agente/Gestor");
                    if (obj.Apolice.AgenteId != agenteId) return BadRequest("Permission Denied");


                    (errorMessage, createAP) = _apolice.CreateApolicePessoal(createAP, obj);
                    if (createAP == null) return BadRequest(error: errorMessage);

                    return Ok(createAP.Id);

                case Roles.Cliente:
                    int? clienteId = _app.GetClienteId(userId);
                    if (clienteId == null) return BadRequest("Invalid Cliente");
                    if (obj.ClienteId != clienteId) return BadRequest("Permission Denied");
                    //ensure that apolice has no agente
                    obj.Apolice.AgenteId = null;
                    obj.Apolice.Simulacao = "Não Validada";

                    (errorMessage, createAP) = _apolice.CreateApolicePessoal(createAP, obj);
                    if (createAP == null) return BadRequest(error: errorMessage);

                    return Ok(createAP.Id);

                default: return BadRequest();
            }
        }

        /// <summary>
        /// ApolicePessoal Get by Id Route
        /// This route can only be accessed by authenticad users, any role can access it.
        /// The returned content depends on the role of the user and his permissions to view.
        /// Admins can see any ApolicePessoal.
        /// Gestores can only see the ApolicePessoal if it Apolice is managed by his Agentes
        /// Agentes can only see the ApolicePessoal if it Apolice is managed by himselves
        /// Cliente can only see the ApolicePessoal if it is his own.
        /// </summary>
        /// <param name="Id">ApolicePessoalId to get</param>
        /// <returns>ApolicePessoal List, size one or zero</returns>
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
                        var objApolicePessoalsList = (from apPessoal in _db.ApolicePessoals
                                                      where apPessoal.Id == Id
                                                      select new { apPessoal.Id, apPessoal.Valor, apPessoal.Apolice, apPessoal.Cliente }).ToList();

                        _logger.SetLogInfoGetAll(userId, "ApolicePessoal");

                        return Ok(objApolicePessoalsList);
                    }
                    else return NotFound();

                case Roles.Gestor:
                    int? equipaId = _app.GetEquipaId(userId);
                    if (equipaId == null) return BadRequest("Invalid Gestor");

                    var apolicesPessoais = (from apPessoal in _db.ApolicePessoals
                                            join apolice in _db.Apolices on apPessoal.ApoliceId equals apolice.Id
                                            join agente in _db.Agentes on apolice.AgenteId equals agente.Id
                                            where agente.EquipaId == equipaId && apPessoal.Id == Id
                                            select new { apPessoal.Id, apPessoal.Valor, apPessoal.Apolice, apPessoal.Cliente }).ToList();

                    _logger.SetLogInfoGetAll(userId, "ApolicePessoal");

                    return Ok(apolicesPessoais);

                case Roles.Agente:
                    int? agenteId = _app.GetAgenteId(userId);
                    if (agenteId == null) return BadRequest("Invalid Agente");

                    var apolicesAgentePessoais = (from apPessoal in _db.ApolicePessoals
                                                  join apolice in _db.Apolices on apPessoal.ApoliceId equals apolice.Id
                                                  where apolice.AgenteId == agenteId && apPessoal.Id == Id
                                                  select new { apPessoal.Id, apPessoal.Valor, apPessoal.Apolice, apPessoal.Cliente }).ToList();

                    _logger.SetLogInfoGetAll(userId, "ApolicePessoal");

                    return Ok(apolicesAgentePessoais);

                case Roles.Cliente:
                    int? clienteId = _app.GetClienteId(userId);
                    if (clienteId == null) return BadRequest("Invalid Cliente");

                    var apolicesClientePessoais = (from apPessoal in _db.ApolicePessoals
                                                   where apPessoal.ClienteId == clienteId && apPessoal.Id == Id
                                                   select new { apPessoal.Id, apPessoal.Valor, apPessoal.Apolice, apPessoal.Cliente }).ToList();

                    _logger.SetLogInfoGetAll(userId, "ApolicePessoal");

                    return Ok(apolicesClientePessoais);
                default: return BadRequest();
            }
        }

        /// <summary>
        /// ApolicePessoal Update Route
        /// This route can only be accessed by authenticad users, any role can access it.
        /// The update depends on the role of the user and his permissions to update.
        /// Admins can update any ApolicePessoal.
        /// Gestores can only update the ApolicePessoal if it Apolice is managed by his Agentes
        /// Agentes can only update the ApolicePessoal if it Apolice is managed by himselves
        /// Cliente can only update the ApolicePessoal if it is his own.
        /// </summary>
        /// <param name="Id">ApolicePessoalId to update</param>
        /// <param name="obj">ApolicePessoal</param>
        /// <returns>If sucessfull the updated object</returns>
        [HttpPut("{Id}"), Auth(Roles.Admin, Roles.Gestor, Roles.Agente, Roles.Cliente)]
        public IActionResult UpdateApolicePessoal(int Id, ApolicePessoal obj)
        {
            string bearer = Request.Headers.Authorization[0].Replace("Bearer ", "");
            int? userId = _app.GetUserId(bearer);
            var userRole = _app.GetUserRole(bearer);
            if (userId == null || userRole == null) return BadRequest("Invalid User");

            if (!ModelState.IsValid) return BadRequest(error: "Invalid Object");

            ApolicePessoal? updateObj = _db.ApolicePessoals.Find(Id);
            if (updateObj == null) return BadRequest(error: "Invalid Id");

            string errorMessage;

            switch (userRole)
            {
                case Roles.Admin:
                    (errorMessage, updateObj) = _apolice.UpdateApolicePessoal(updateObj, obj);
                    if (errorMessage != "") return BadRequest(error: errorMessage);
                    return Ok(updateObj);

                case Roles.Gestor:
                    int? equipaId = _app.GetEquipaId(userId);

                    if (equipaId == null) return BadRequest("Invalid Gestor");

                    (errorMessage, updateObj) = _apolice.UpdateApolicePessoal(updateObj, obj);
                    if (errorMessage != "") return BadRequest(error: errorMessage);
                    return Ok(updateObj);

                case Roles.Agente:
                    int? agenteId = _app.GetAgenteId(userId);
                    if (agenteId == null) return BadRequest("Invalid Agente");

                    var ap = _db.Apolices.Find(updateObj.ApoliceId);
                    if (ap.AgenteId != agenteId || !_apolice.CanUpdate(ap.Id)) return BadRequest("Permission Denied");

                    (errorMessage, updateObj) = _apolice.UpdateApolicePessoal(updateObj, obj);
                    if (errorMessage != "") return BadRequest(error: errorMessage);
                    return Ok(updateObj);

                case Roles.Cliente:
                    int? clienteId = _app.GetClienteId(userId);
                    if (clienteId == null) return BadRequest("Invalid Cliente");

                    if (updateObj.ClienteId != clienteId) return BadRequest("Permission Denied");

                    (errorMessage, updateObj) = _apolice.UpdateApolicePessoal(updateObj, obj);
                    if (errorMessage != "") return BadRequest(error: errorMessage);
                    return Ok(updateObj);


                default: return BadRequest();
            }
        }
        #endregion

        #region Simulacao

        public class ApolicePessoalActions
        {
            public int ApolicePessoalId { get; set; }
        }

        /// <summary>
        /// Subscription to Simulacao
        /// Expects an object of Type <paramref name="apolicePessoal"/>
        /// </summary>
        /// <param name="apolicePessoal">Describes an instance of a Seguro <b>Pessoal</b>. Must be populated with the correct information.</param>
        /// <returns></returns>
        [HttpPost, Auth]
        public IActionResult AskSimulacao(ApolicePessoal apolicePessoal)
        {
            if (!ModelState.IsValid) return BadRequest("ModelState Is Not Valid");
            var token = Request.Headers.Authorization[0].Replace("Bearer ", "");
            var reqId = _app.GetUserId(token);
            var role = _app.GetUserRole(token);
            if (reqId is null || role is null) return BadRequest("Error retrieving Header Information.");
            // Request User
            var user = _db.ManaUsers.Find(reqId);
            if (user is null) return BadRequest("Error finding request user.");
            // Request Apolice
            var apolice = apolicePessoal.Apolice;
            if (apolice is null) return BadRequest("No Apolice was described on request body.");
            // Request Seguro
            var seguro = _db.Seguros.Find(apolice.SeguroId);
            if (seguro is null) return BadRequest("");

            // Apolice Veiculo
            apolicePessoal.Cliente = null;
            apolicePessoal.SinistroPessoals.Clear();

            // Apolice
            if (role == Roles.Admin)
            {
                apolice = apolicePessoal.Apolice;
                if (apolice is null) return BadRequest();
                apolice.ApolicePessoals.Clear();
                apolice.ApoliceSaudes.Clear();
                apolice.ApoliceVeiculos.Clear();
                apolice.CoberturaHasApolices.Clear();
                apolice.Seguro = null;
            }
            else
            {
                apolice = new Apolice
                {
                    Ativa = false,
                    Validade = DateTime.UtcNow.AddDays(-1),
                    //SeguroId = seguroId
                };
            }
            apolicePessoal.Apolice = null;

            // Request Cliente
            Cliente? cliente = _db.Clientes.Find(apolicePessoal.ClienteId);
            int? agenteId = _app.GetAgenteId(reqId);

            switch (role)
            {
                case Roles.Admin:
                    apolice.Simulacao = "Não Validada";
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
                    //apolice.Simulacao = SimulacaoState.Validada.ToString();
                    apolice.Simulacao = "Não Validada";
                    apolice = _db.Apolices.Add(apolice).Entity;
                    if (_db.SaveChanges() == 0) return BadRequest();
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
            apolicePessoal.ApoliceId = apolice.Id;
            apolicePessoal.ClienteId = cliente.Id;
            _db.ApolicePessoals.Add(apolicePessoal);
            _db.SaveChanges();
            apolice = _apolice.Clear(apolice);
            apolicePessoal.Cliente = null;
            apolicePessoal.Apolice = null;
            apolicePessoal.SinistroPessoals.Clear();
            return Ok(new object[] { apolicePessoal, apolice });
        }

        [HttpPost, Auth(Roles.Admin, Roles.Gestor, Roles.Agente)]
        public IActionResult ValidateSimulacao([FromBody] ApolicePessoalActions obj)
        {
            int apolicePessoalId = obj.ApolicePessoalId;
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
            var apolicePessoal = _db.ApolicePessoals.Find(apolicePessoalId);
            if (apolicePessoal is null) return BadRequest("Apolice Veiculo not found!");

            var cliente = _db.Clientes.Where(v => v.Id == apolicePessoal.ClienteId).FirstOrDefault();
            if (cliente is null) return BadRequest("veículo has no cliente associated");

            var apolice = _db.Apolices.Find(apolicePessoal.ApoliceId);
            if (apolice is null) return BadRequest("apolice does not exist");

            if (apolice.Simulacao is not null &&
                !apolice.Simulacao.Equals("Não Validada")) return BadRequest();
            apolice.Simulacao = SimulacaoState.Validada.ToString();

            switch (role)
            {
                case Roles.Admin:
                    _db.Apolices.Update(apolice);
                    _db.ApolicePessoals.Update(apolicePessoal);
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
                    _db.ApolicePessoals.Update(apolicePessoal);
                    result = Ok(); break;
            }

            _db.SaveChanges();
            return result;
        }

        /// <summary>
        /// Accept Simulação Pessoal
        /// </summary>
        /// <param name="apolicePessoalId"></param>
        /// <returns></returns>
        /// <returns></returns>
        [HttpPost, Auth]
        public IActionResult AcceptSimulacao([FromBody] ApolicePessoalActions obj)
        {
            int apolicePessoalId = obj.ApolicePessoalId;
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

            var apolicePessoal = _db.ApolicePessoals.Find(apolicePessoalId);
            if (apolicePessoal is null) return BadRequest();

            var apolice = _db.Apolices.Find(apolicePessoal.ApoliceId);
            if (apolice is null) return BadRequest();
            if (apolice.Fracionamento is null || apolice.Premio is null) return BadRequest("Something went wrong generating this apolice");

            Cliente? cliente = _db.Clientes.Find(apolicePessoal.ClienteId); ;

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
                    else return BadRequest(error: "Cliente has no `Agente`");
                    return BadRequest(error: "You don't have access to this team.");
                case Roles.Agente:
                    if (agente is null) return BadRequest(error: "You are not agente");
                    if (cliente is null) return BadRequest(error: "Cliente is null");
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
                DataPagamento = DateTime.Parse("1900-01-01"),
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

            _db.ApolicePessoals.Update(apolicePessoal);
            if (_db.SaveChanges() == 0) return BadRequest("Changes were not saved. Abort!");

            // Add Pagamento
            if (!_pagamento.PagamentoExists(pagamento))
                _pagamento.AddPagamento(pagamento);
            else
                return BadRequest(error: "There is a similar pagamento");
            return Ok("pagamento emitido");
        }

        [HttpPost, Auth]
        public IActionResult CancelarSimulacao([FromBody] ApolicePessoalActions obj)
        {
            int apolicePessoalId = obj.ApolicePessoalId;
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

            var apolicePessoal = _db.ApolicePessoals.Find(apolicePessoalId);
            if (apolicePessoal is null) return BadRequest();

            var apolice = _db.Apolices.Find(apolicePessoal.ApoliceId);
            if (apolice is null) return BadRequest();
            if (apolice.Fracionamento is null || apolice.Premio is null) return BadRequest("Something went wrong generating this apolice");

            Cliente? cliente = _db.Clientes.Find(apolicePessoal.ClienteId); ;

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
                    else return BadRequest(error: "Cliente has no `Agente`");
                    return BadRequest(error: "You don't have access to this team.");
                case Roles.Agente:
                    if (agente is null) return BadRequest(error: "You are not agente");
                    if (cliente is null) return BadRequest(error: "Cliente is null");
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

            _db.ApolicePessoals.Update(apolicePessoal);
            if (_db.SaveChanges() == 0) return BadRequest("Changes were not saved. Abort!");
            return Ok("Apolice Cancelada");
        }

        /// <summary>
        /// Represents the Information needed to Calculate Premio Pessoal
        /// </summary>
        public record CalculatePremioPessoal
        {
            /// <summary>
            /// Id for Cliente Information
            /// </summary>
            public int ClienteId { get; set; }
            /// <summary>
            /// Value that Cliente retrieves in case of Apolice Action
            /// </summary>
            public double Valor { get; set; }
        }

        /// <summary>
        /// Calculate value to be paid for an Apolice for a specific Cliente.
        /// </summary>
        /// <param name="obj">Information required for the calculus.</param>
        /// <returns>Value to be paid for an ApolicePessoal</returns>
        [HttpPost, Auth]
        public IActionResult CalculatePremio(CalculatePremioPessoal obj)
        {
            var premio = _app.CalculateSaudePremio(obj.ClienteId);

            if (premio is null)
                return BadRequest("Something Went Wrong");

            var cliente = _db.Clientes.Find(obj.ClienteId);
            if (cliente is null) return BadRequest();
            if (cliente.ProfissaoRisco is not null && cliente.ProfissaoRisco.Value)
            {
                premio += 100;
            }

            premio += obj.Valor / 1000.0;

            return Ok(premio);
        }
        #endregion
    }
}
