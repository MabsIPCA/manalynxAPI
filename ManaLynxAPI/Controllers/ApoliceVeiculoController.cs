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
    [ApiController, Route("[controller]/[action]"), Auth]
    public class ApoliceVeiculoController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly IPagamentoUtils _pagamento;
        private readonly IAppUtils _app;
        private readonly ILoggerUtils _logger;
        private readonly IApoliceUtils _apolice;

        public ApoliceVeiculoController(ApplicationDbContext db, IAppUtils app, IPagamentoUtils pagamento, ILoggerUtils logger, IApoliceUtils apolice)
        {
            _db = db;
            _app = app;
            _pagamento = pagamento;
            _logger = logger;
            _apolice = apolice;
        }

        #region Apolice
        /// <summary>
        /// ApoliceVeiculo index Route
        /// This route can only be accessed by authenticad users, any role can access it.
        /// The returned content depends on the role of the user and his permissions to view.
        /// Admins can see all the data in the database.
        /// Gestores can only see ApoliceVeiculo managed by his Agentes
        /// Agentes can only see ApoliceVeiculo managed by himselves
        /// Cliente can only see his ApoliceVeiculo
        /// </summary>
        /// <returns>ApoliceVeiculo List, possibly empty</returns>
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
                        var objApoliceVeiculosList = (from apVeiculo in _db.ApoliceVeiculos
                                                      select new
                                                      {
                                                          apVeiculo.Id,
                                                          apVeiculo.DataCartaConducao,
                                                          apVeiculo.AcidentesRecentes,
                                                          apVeiculo.Veiculo,
                                                          apVeiculo.Apolice
                                                      }).ToList();

                        _logger.SetLogInfoGetAll(userId, "ApoliceVeiculo");

                        return Ok(objApoliceVeiculosList);
                    }
                    else return NotFound();

                case Roles.Gestor:
                    int? equipaId = _app.GetEquipaId(userId);
                    if (equipaId == null) return BadRequest("Invalid Gestor");

                    var apolicesGestorVeiculos = (from apVeiculo in _db.ApoliceVeiculos
                                                  join apolice in _db.Apolices on apVeiculo.ApoliceId equals apolice.Id
                                                  join agente in _db.Agentes on apolice.AgenteId equals agente.Id
                                                  where agente.EquipaId == equipaId
                                                  select new
                                                  {
                                                      apVeiculo.Id,
                                                      apVeiculo.DataCartaConducao,
                                                      apVeiculo.AcidentesRecentes,
                                                      apVeiculo.Veiculo,
                                                      apVeiculo.Apolice
                                                  }).ToList();

                    _logger.SetLogInfoGetAll(userId, "ApoliceVeiculo");

                    return Ok(apolicesGestorVeiculos);

                case Roles.Agente:
                    int? agenteId = _app.GetAgenteId(userId);
                    if (agenteId == null) return BadRequest("Invalid Agente");

                    var apolicesAgenteVeiculos = (from apVeiculo in _db.ApoliceVeiculos
                                                  join apolice in _db.Apolices on apVeiculo.ApoliceId equals apolice.Id
                                                  where apolice.AgenteId == agenteId
                                                  select new
                                                  {
                                                      apVeiculo.Id,
                                                      apVeiculo.DataCartaConducao,
                                                      apVeiculo.AcidentesRecentes,
                                                      apVeiculo.Veiculo,
                                                      apVeiculo.Apolice
                                                  }).ToList();

                    _logger.SetLogInfoGetAll(userId, "ApoliceVeiculo");

                    return Ok(apolicesAgenteVeiculos);

                case Roles.Cliente:
                    int? clienteId = _app.GetClienteId(userId);
                    if (clienteId == null) return BadRequest("Invalid Cliente");

                    var apolicesClienteVeiculos = (from apVeiculo in _db.ApoliceVeiculos
                                                   join veiculo in _db.Veiculos on apVeiculo.VeiculoId equals veiculo.Id
                                                   where veiculo.ClienteId == clienteId
                                                   select new
                                                   {
                                                       apVeiculo.Id,
                                                       apVeiculo.DataCartaConducao,
                                                       apVeiculo.AcidentesRecentes,
                                                       apVeiculo.Veiculo,
                                                       apVeiculo.Apolice
                                                   }).ToList();

                    _logger.SetLogInfoGetAll(userId, "ApoliceVeiculo");

                    return Ok(apolicesClienteVeiculos);
                default: return BadRequest();
            }
        }

        /// <summary>
        /// ApoliceVeiculo Create Route
        /// This route can only be accessed by authenticad users, any role can access it.
        /// The creation depends on the role of the user and his permissions to create.
        /// Admins can create any ApoliceVeiculo.
        /// Gestores can only create the ApoliceVeiculo if it Apolice is managed by himselves
        /// Agentes can only create the ApoliceVeiculo if it Apolice is managed by himselves
        /// Cliente can only create the ApoliceVeiculo if it is his own.
        /// </summary>
        /// <param name="obj">ApoliceVeiculo with Apolice incapsulated Object</param>
        /// <returns>If sucessfull return created ApoliceVeiculoId</returns>
        [HttpPost, Auth(Roles.Admin, Roles.Gestor, Roles.Agente, Roles.Cliente)]
        public IActionResult Create(ApoliceVeiculo obj)
        {
            string bearer = Request.Headers.Authorization[0].Replace("Bearer ", "");
            int? userId = _app.GetUserId(bearer);
            var userRole = _app.GetUserRole(bearer);
            if (userId == null || userRole == null) return BadRequest("Invalid user");

            if (!ModelState.IsValid) return BadRequest("Invalid object");

            var createAP = new ApoliceVeiculo();

            string errorMessage;

            switch (userRole)
            {
                case Roles.Admin:
                    (errorMessage, createAP) = _apolice.CreateApoliceVeiculo(createAP, obj);
                    if (errorMessage != "") return BadRequest(error: errorMessage);

                    return Ok(createAP);

                case Roles.Gestor:
                case Roles.Agente:
                    int? agenteId = _app.GetAgenteId(userId);
                    if (agenteId == null) return BadRequest("Invalid Agente/Gestor");

                    if (obj.Apolice.AgenteId != agenteId) return BadRequest("Permission Denied");

                    (errorMessage, createAP) = _apolice.CreateApoliceVeiculo(createAP, obj);
                    if (errorMessage != "") return BadRequest(error: errorMessage);

                    return Ok(createAP);

                case Roles.Cliente:
                    int? clienteId = _app.GetClienteId(userId);
                    if (clienteId == null) return BadRequest("Invalid Cliente");

                    var veiculo = _db.Veiculos.Find(obj.VeiculoId);
                    if (veiculo.ClienteId != clienteId) return BadRequest("Permission Denied");
                    //ensure that apolice has no agente
                    obj.Apolice.AgenteId = null;
                    obj.Apolice.Simulacao = "Não Validada";

                    (errorMessage, createAP) = _apolice.CreateApoliceVeiculo(createAP, obj);
                    if (errorMessage != "") return BadRequest(error: errorMessage);

                    return Ok(createAP);

                default: return BadRequest();
            }
        }

        [HttpGet("ApoliceVeiculo/{id}"), Auth(Roles.Admin)]
        public IActionResult GetAdmin(int id)
        {
            var apoliceVeiculo = (from av in _db.ApoliceVeiculos
                                  where av.Id == id
                                  select new { av.Id, av.Veiculo, av.Apolice }).ToList().FirstOrDefault();

            var token = Request.Headers.Authorization[0].Replace("Bearer ", "");
            _logger.SetLogInfoGet(_app.GetUserId(token), "ApoliceVeiculo", id);

            return Ok(apoliceVeiculo);
        }

        /// <summary>
        /// ApoliceVeiculo Get by Id Route
        /// This route can only be accessed by authenticad users, any role can access it.
        /// The returned content depends on the role of the user and his permissions to view.
        /// Admins can see any ApoliceVeiculo.
        /// Gestores can only see the ApoliceVeiculo if it Apolice is managed by his Agentes
        /// Agentes can only see the ApoliceVeiculo if it Apolice is managed by himselves
        /// Cliente can only see the ApoliceVeiculo if it is his own.
        /// </summary>
        /// <param name="Id">ApoliceVeiculoId to get</param>
        /// <returns>ApoliceVeiculo List, size one or zero</returns>
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
                        var objApoliceVeiculosList = (from apVeiculo in _db.ApoliceVeiculos
                                                      where apVeiculo.Id == Id
                                                      select new
                                                      {
                                                          apVeiculo.Id,
                                                          apVeiculo.DataCartaConducao,
                                                          apVeiculo.AcidentesRecentes,
                                                          apVeiculo.Veiculo,
                                                          apVeiculo.Apolice
                                                      }).ToList();

                        _logger.SetLogInfoGetAll(userId, "ApoliceVeiculo");

                        return Ok(objApoliceVeiculosList);
                    }
                    else return NotFound();

                case Roles.Gestor:
                    int? equipaId = _app.GetEquipaId(userId);
                    if (equipaId == null) return BadRequest("Invalid Gestor");

                    var apolicesGestorVeiculos = (from apVeiculo in _db.ApoliceVeiculos
                                                  join apolice in _db.Apolices on apVeiculo.ApoliceId equals apolice.Id
                                                  join agente in _db.Agentes on apolice.AgenteId equals agente.Id
                                                  where agente.EquipaId == equipaId && apVeiculo.Id == Id
                                                  select new
                                                  {
                                                      apVeiculo.Id,
                                                      apVeiculo.DataCartaConducao,
                                                      apVeiculo.AcidentesRecentes,
                                                      apVeiculo.Veiculo,
                                                      apVeiculo.Apolice
                                                  }).ToList();

                    _logger.SetLogInfoGetAll(userId, "ApoliceVeiculo");

                    return Ok(apolicesGestorVeiculos);

                case Roles.Agente:
                    int? agenteId = _app.GetAgenteId(userId);
                    if (agenteId == null) return BadRequest("Invalid Agente");

                    var apolicesAgenteVeiculos = (from apVeiculo in _db.ApoliceVeiculos
                                                  join apolice in _db.Apolices on apVeiculo.ApoliceId equals apolice.Id
                                                  where apolice.AgenteId == agenteId && apVeiculo.Id == Id
                                                  select new
                                                  {
                                                      apVeiculo.Id,
                                                      apVeiculo.DataCartaConducao,
                                                      apVeiculo.AcidentesRecentes,
                                                      apVeiculo.Veiculo,
                                                      apVeiculo.Apolice
                                                  }).ToList();

                    _logger.SetLogInfoGetAll(userId, "ApoliceVeiculo");

                    return Ok(apolicesAgenteVeiculos);

                case Roles.Cliente:
                    int? clienteId = _app.GetClienteId(userId);
                    if (clienteId == null) return BadRequest("Invalid Cliente");

                    var apolicesClienteVeiculos = (from apVeiculo in _db.ApoliceVeiculos
                                                   join veiculo in _db.Veiculos on apVeiculo.VeiculoId equals veiculo.Id
                                                   where veiculo.ClienteId == clienteId && apVeiculo.Id == Id
                                                   select new
                                                   {
                                                       apVeiculo.Id,
                                                       apVeiculo.DataCartaConducao,
                                                       apVeiculo.AcidentesRecentes,
                                                       apVeiculo.Veiculo,
                                                       apVeiculo.Apolice
                                                   }).ToList();

                    _logger.SetLogInfoGetAll(userId, "ApoliceVeiculo");

                    return Ok(apolicesClienteVeiculos);
                default: return BadRequest();
            }
        }


        /// <summary>
        /// ApoliceVeiculo Update Route
        /// This route can only be accessed by authenticad users, any role can access it.
        /// The update depends on the role of the user and his permissions to update.
        /// Admins can update any ApoliceVeiculo.
        /// Gestores can only update the ApoliceVeiculo if it Apolice is managed by his Agentes
        /// Agentes can only update the ApoliceVeiculo if it Apolice is managed by himselves
        /// Cliente can only update the ApoliceVeiculo if it is his own.
        /// </summary>
        /// <param name="Id">ApoliceVeiculoId to update</param>
        /// <param name="obj">ApoliceVeiculo</param>
        /// <returns>If sucessfull the updated object</returns>
        [HttpPut("{Id}"), Auth(Roles.Admin, Roles.Gestor, Roles.Agente, Roles.Cliente)]
        public IActionResult UpdateApoliceVeiculo(int Id, ApoliceVeiculo obj)
        {
            string bearer = Request.Headers.Authorization[0].Replace("Bearer ", "");
            int? userId = _app.GetUserId(bearer);
            var userRole = _app.GetUserRole(bearer);
            if (userId == null || userRole == null) return BadRequest("Invalid User");

            ApoliceVeiculo updateObj = _db.ApoliceVeiculos.Find(Id);
            if (updateObj == null) return BadRequest(error: "Invalid Id");

            string errorMessage;

            switch (userRole)
            {
                case Roles.Admin:
                    (errorMessage, updateObj) = _apolice.UpdateApoliceVeiculo(updateObj, obj);
                    if (errorMessage != "") return BadRequest(error: errorMessage);

                    return Ok(updateObj);


                case Roles.Gestor:
                    int? equipaId = _app.GetEquipaId(userId);
                    if (equipaId == null) return BadRequest("Invalid Gestor");


                    var ap = _db.Apolices.Find(updateObj.ApoliceId);
                    var agente = _db.Agentes.Find(ap.AgenteId);
                    if (agente.EquipaId != equipaId || !_apolice.CanUpdate(ap.Id)) return BadRequest("Permission Denied");

                    (errorMessage, updateObj) = _apolice.UpdateApoliceVeiculo(updateObj, obj);
                    if (errorMessage != "") return BadRequest(error: errorMessage);

                    return Ok(updateObj);

                case Roles.Agente:
                    int? agenteId = _app.GetAgenteId(userId);
                    if (agenteId == null) return BadRequest("Invalid Agente");

                    var apA = _db.Apolices.Find(updateObj.ApoliceId);
                    if (apA.AgenteId != agenteId) return BadRequest("Permission Denied");

                    (errorMessage, updateObj) = _apolice.UpdateApoliceVeiculo(updateObj, obj);
                    if (errorMessage != "") return BadRequest(error: errorMessage);

                    return Ok(updateObj);


                case Roles.Cliente:
                    int? clienteId = _app.GetClienteId(userId);
                    if (clienteId == null) return BadRequest("Invalid Cliente");

                    var veiculo = _db.Veiculos.Find(updateObj.VeiculoId);
                    if (veiculo.ClienteId != clienteId ) return BadRequest("Permission Denied");

                    (errorMessage, updateObj) = _apolice.UpdateApoliceVeiculo(updateObj, obj);
                    if (errorMessage != "") return BadRequest(error: errorMessage);

                    return Ok(updateObj);

                default: return BadRequest();
            }
        }
        #endregion

        #region Simulacao

        public class ApoliceVeiculoActions
        {
            public int ApoliceVeiculoId { get; set; }
        }

        [HttpPost, Auth]
        public IActionResult AskSimulacao(ApoliceVeiculo apoliceVeiculo, int seguroId)
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
            var apolice = _db.Apolices.Find(apoliceVeiculo.ApoliceId);
            if (apolice is not null) return BadRequest();
            // Request Seguro
            var seguro = _db.Seguros.Find(seguroId);
            if (seguro is null) return BadRequest();

            // Apolice Veiculo
            apoliceVeiculo.SinistroVeiculos.Clear();

            // Apolice
            if (role == Roles.Admin)
            {
                apolice = apoliceVeiculo.Apolice;
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
                    SeguroId = seguroId
                };
            }
            apoliceVeiculo.Apolice = null;

            // Request Veiculo
            Veiculo? veiculo = _db.Veiculos.Find(apoliceVeiculo.VeiculoId);
            if (veiculo is null) return BadRequest();
            // Request Cliente
            Cliente? cliente = _db.Clientes.Find(veiculo.ClienteId);
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
                    veiculo = _db.Veiculos.Where(v => v.ClienteId == cliente.Id && v.Id == apoliceVeiculo.VeiculoId).FirstOrDefault();
                    if (veiculo is null) return BadRequest();
                    //apolice.Simulacao = SimulacaoState.Validada.ToString();
                    apolice.Simulacao = "Não Validada";
                    apolice = _db.Apolices.Add(apolice).Entity;
                    if (_db.SaveChanges() == 0) return BadRequest();
                    break;

                case Roles.Cliente:
                    cliente = _db.Clientes.Where(c => c.PessoaId == user.PessoaId).FirstOrDefault();
                    if (cliente is null) return BadRequest();
                    veiculo = _db.Veiculos.Where(v => v.ClienteId == cliente.Id && v.Id == apoliceVeiculo.VeiculoId).FirstOrDefault();
                    if (veiculo is null) return BadRequest();
                    //apolice.Simulacao = SimulacaoState.NaoValidada.ToString();
                    apolice.Simulacao = "Não Validada";
                    apolice = _db.Apolices.Add(apolice).Entity;
                    if (_db.SaveChanges() == 0) return BadRequest();
                    break;
            }

            if (veiculo is null) return BadRequest();
            apoliceVeiculo.ApoliceId = apolice.Id;
            apoliceVeiculo.VeiculoId = veiculo.Id;
            _db.ApoliceVeiculos.Add(apoliceVeiculo);
            _db.SaveChanges();
            apolice = _apolice.Clear(apolice);
            apoliceVeiculo.Apolice = null;
            apoliceVeiculo.SinistroVeiculos.Clear();
            apoliceVeiculo.Veiculo = null;
            return Ok(new object[] { apolice, apoliceVeiculo });
        }

        [HttpPost, Auth(Roles.Admin, Roles.Gestor, Roles.Agente)]
        public IActionResult ValidateSimulacao([FromBody] ApoliceVeiculoActions obj)
        {
            int apoliceVeiculoId = obj.ApoliceVeiculoId;
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
            var apoliceVeiculo = _db.ApoliceVeiculos.Find(apoliceVeiculoId);
            if (apoliceVeiculo is null) return BadRequest("Apolice Veiculo not found!");

            var cliente = _db.Veiculos.Where(v => v.Id == apoliceVeiculo.VeiculoId).Select(v => v.Cliente).FirstOrDefault();
            if (cliente is null) return BadRequest("veículo has no cliente associated");

            var apolice = _db.Apolices.Find(apoliceVeiculo.ApoliceId);
            if (apolice is null) return BadRequest("apolice does not exist");

            if (apolice.Simulacao is not null &&
                !apolice.Simulacao.Equals("Não Validada")) return BadRequest();
            apolice.Simulacao = SimulacaoState.Validada.ToString();

            switch (role)
            {
                case Roles.Admin:
                    _db.Apolices.Update(apolice);
                    _db.ApoliceVeiculos.Update(apoliceVeiculo);
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
                    _db.ApoliceVeiculos.Update(apoliceVeiculo);
                    result = Ok(); break;
            }

            _db.SaveChanges();
            return result;
        }

        [HttpPost, Auth]
        public IActionResult AcceptSimulacao([FromBody] ApoliceVeiculoActions obj)
        {
            int apoliceVeiculoId = obj.ApoliceVeiculoId;
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

            var apoliceVeiculo = _db.ApoliceVeiculos.Find(apoliceVeiculoId);
            if (apoliceVeiculo is null) return BadRequest();

            var apolice = _db.Apolices.Find(apoliceVeiculo.ApoliceId);
            if (apolice is null) return BadRequest();
            if (apolice.Fracionamento is null || apolice.Premio is null) return BadRequest("Something went wrong generating this apolice");

            var cliente = _db.Veiculos.Where(v => v.Id == apoliceVeiculo.VeiculoId).Select(v => v.Cliente).FirstOrDefault();
            if (cliente is null) return BadRequest("veículo has no cliente associated");

            switch (role)
            {
                case Roles.Gestor:
                    if (agente is null) return BadRequest("You are not agente");
                    if (_db.Gestors.Any(g => g.AgenteId == agente.Id) is false) return BadRequest("You are not gestor");
                    var equipa = _db.Agentes.Where(a => a.Id == agente.Id).Select(a => a.Equipa).FirstOrDefault();
                    if (equipa is null) return BadRequest("You don't have a team");
                    if (cliente.Agente is not null)
                    {
                        if (equipa.Agentes.Contains(cliente.Agente))
                            break;
                    }
                    else return BadRequest("Cliente has no `Agente`");
                    return BadRequest("You don't have access to this team.");
                case Roles.Agente:
                    if (agente is null) return BadRequest("You are not agente");
                    if (cliente.IsLead != 1) return BadRequest("You can't access this cliente");
                    if (apolice.AgenteId != agente.Id) return BadRequest();
                    break;
                case Roles.Cliente:
                    if (user.PessoaId != cliente.PessoaId) return BadRequest("You do not have permission");
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

            _db.ApoliceVeiculos.Update(apoliceVeiculo);
            if (_db.SaveChanges() == 0) return BadRequest("Changes were not saved. Abort!");

            // Add Pagamento
            if (!_pagamento.PagamentoExists(pagamento))
                _pagamento.AddPagamento(pagamento);
            else
                return BadRequest(error: "There is a similar pagamento");
            return Ok("pagamento emitido");
        }

        [HttpPost, Auth]
        public IActionResult CancelarSimulacao([FromBody] ApoliceVeiculoActions obj)
        {
            int apoliceVeiculoId = obj.ApoliceVeiculoId;
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

            var apoliceVeiculo = _db.ApoliceVeiculos.Find(apoliceVeiculoId);
            if (apoliceVeiculo is null) return BadRequest();

            var apolice = _db.Apolices.Find(apoliceVeiculo.ApoliceId);
            if (apolice is null) return BadRequest();
            if (apolice.Fracionamento is null || apolice.Premio is null) return BadRequest("Something went wrong generating this apolice");

            var cliente = _db.Veiculos.Where(v => v.Id == apoliceVeiculo.VeiculoId).Select(v => v.Cliente).FirstOrDefault();
            if (cliente is null) return BadRequest("veículo has no cliente associated");

            switch (role)
            {
                case Roles.Gestor:
                    if (agente is null) return BadRequest("You are not agente");
                    if (_db.Gestors.Any(g => g.AgenteId == agente.Id) is false) return BadRequest("You are not gestor");
                    var equipa = _db.Agentes.Where(a => a.Id == agente.Id).Select(a => a.Equipa).FirstOrDefault();
                    if (equipa is null) return BadRequest("You don't have a team");
                    if (cliente.Agente is not null)
                    {
                        if (equipa.Agentes.Contains(cliente.Agente))
                            break;
                    }
                    else return BadRequest("Cliente has no `Agente`");
                    return BadRequest("You don't have access to this team.");
                case Roles.Agente:
                    if (agente is null) return BadRequest("You are not agente");
                    if (cliente.IsLead != 1) return BadRequest("You can't access this cliente");
                    if (apolice.AgenteId != agente.Id) return BadRequest();
                    break;
                case Roles.Cliente:
                    if (user.PessoaId != cliente.PessoaId) return BadRequest("You do not have permission");
                    break;
            }

            apolice.Ativa = false;
            apolice.Simulacao = SimulacaoState.Cancelada.ToString();

            _db.Apolices.Update(apolice);
            if (_db.SaveChanges() == 0) return BadRequest("Changes were not saved. Abort!");

            _db.ApoliceVeiculos.Update(apoliceVeiculo);
            if (_db.SaveChanges() == 0) return BadRequest("Changes were not saved. Abort!");

            return Ok("Apolice Cancelada");
        }

        /// <summary>
        /// Represents the Information needed to Calculate Premio Veiculo
        /// </summary>
        public record CalculatePremioVeiculo
        {
            /// <summary>
            /// Id to get the Veiculo Information
            /// </summary>
            public int VeiculoId { get; set; }
            /// <summary>
            /// CartaConducao
            /// </summary>
            public DateTime CartaConducao { get; set; }
            /// <summary>
            /// Number of Sinistros
            /// </summary>
            public int Sinistros { get; set; }
        }

        /// <summary>
        /// Calculate value to be paid for an Apolice for a specific veiculo.
        /// </summary>
        /// <param name="obj">Information required for the calculus.</param>
        /// <returns>Value to be paid for an ApoliceVeiculo</returns>
        [HttpPost, Auth]
        public IActionResult CalculatePremio(CalculatePremioVeiculo obj)
        {
            double premio = 0;
            var veiculo = _db.Veiculos.Find(obj.VeiculoId);
            if (veiculo is null) return BadRequest(error: "Veiculo not found.");

            var categoria = _db.CategoriaVeiculos.Find(veiculo.CategoriaVeiculoId);
            if (categoria is null) return BadRequest();

            // Sinistros Recentes
            if (obj.Sinistros > 1) premio += obj.Sinistros * 40;
            else if (obj.Sinistros == 1) premio += 10;
            else if (obj.Sinistros == 0) premio += 0;

            // Idade de Carta
            int idade = DateTime.UtcNow.Year - veiculo.Ano;
            int idadeCarta = DateTime.UtcNow.Year - obj.CartaConducao.Year;
            if (idadeCarta < 3) premio += 40;

            // Idade de Veiculo
            if (idade > 20) premio += 20;
            else if (idade <= 20 && idade > 5) premio += 20;
            else if (idade < 5 && idade > 0) premio += 40;
            else if (idade == 0) premio += 80;
            else if (idade < 0) return BadRequest();

            // Cilindrada
            if (veiculo.Cilindrada is null) return BadRequest();

            switch (categoria.Id)
            {
                case 1://Merc
                    if (veiculo.Peso <= 2500) premio += 40;
                    else if (veiculo.Peso > 2500 && veiculo.Peso <= 3500) premio += 60;

                    if (veiculo.Cilindrada <= 2000) premio += 20;
                    else if (veiculo.Cilindrada > 2000) premio += 40;

                    if (veiculo.Potencia > 60 && veiculo.Potencia <= 100) premio += 20;
                    else if (veiculo.Potencia > 100 && veiculo.Potencia <= 200) premio += 40;
                    else if (veiculo.Potencia > 200) premio += 80;
                    break;
                case 2://Pass
                    if (veiculo.Lugares > 0 && veiculo.Lugares <= 5) premio += 20;
                    else if (veiculo.Lugares > 5 && veiculo.Lugares <= 9) premio += 30;
                    goto case 1;

                case 3://Motociclo
                    if (veiculo.Cilindrada > 50 && veiculo.Cilindrada <= 125) premio += 20;
                    else if (veiculo.Cilindrada > 125 && veiculo.Cilindrada <= 250) premio += 40;
                    else if (veiculo.Cilindrada > 250 && veiculo.Cilindrada <= 750) premio += 60;
                    else if (veiculo.Cilindrada > 750 && veiculo.Cilindrada <= 1000) premio += 80;
                    else if (veiculo.Cilindrada > 1000) premio += 100;
                    break;

                case 4://Ciclomotor
                    if (veiculo.Cilindrada < 50) premio += 20;
                    break;

                case 5://Pass
                    if (veiculo.Lugares > 9 && veiculo.Lugares <= 20) premio += 50;
                    else if (veiculo.Lugares > 20 && veiculo.Lugares <= 70) premio += 70;
                    goto case 6;
                case 6://Merc
                    if (veiculo.Peso >= 3500 && veiculo.Peso <= 7500) premio += 40;
                    else if (veiculo.Peso > 7500 && veiculo.Peso <= 12000) premio += 60;

                    if (veiculo.Cilindrada >= 7000 && veiculo.Cilindrada <= 9000) premio += 20;
                    else if (veiculo.Cilindrada > 2000) premio += 40;

                    if (veiculo.Potencia > 60 && veiculo.Potencia <= 100) premio += 20;
                    else if (veiculo.Potencia > 100 && veiculo.Potencia <= 200) premio += 40;
                    else if (veiculo.Potencia > 200) premio += 80;
                    break;
            }

            return Ok(premio);
        }
        #endregion
    }
}
