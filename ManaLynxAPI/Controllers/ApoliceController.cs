using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Auth = ManaLynxAPI.Authentication.Auth;
using ManaLynxAPI.Data;
using ManaLynxAPI.Models;
using Roles = ManaLynxAPI.Models.Roles;
using Microsoft.AspNetCore.Authorization;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using ManaLynxAPI.Utils;

namespace ManaLynxAPI.Controllers
{
    [Authorize]
    [ApiController]
    public class ApoliceController : Controller
    {
        private readonly IJWTAuthManager _auth;
        private readonly ApplicationDbContext _db;
        private readonly ILoggerUtils _logger;
        private readonly IAppUtils _app;
        private readonly IApoliceUtils _apolice;

        public ApoliceController(ApplicationDbContext db, IJWTAuthManager auth, ILoggerUtils logger, IAppUtils app, IApoliceUtils apolice)
        {
            _db = db;
            _auth = auth;
            _logger = logger;
            _app = app;
            _apolice = apolice;
        }

        /// <summary>
        /// Apolice index Route
        /// This route can only be accessed by authenticad users, any role can access it.
        /// The returned content depends on the role of the user and his permissions to view.
        /// Admins can see all the data in the database.
        /// Gestores can only see Apolice managed by his Agentes
        /// Agentes can only see Apolice managed by himselves
        /// Cliente can only see his Apolice
        /// </summary>
        /// <returns>Apolice List, possibly empty</returns>
        [HttpGet("Apolice"), Auth(Roles.Admin, Roles.Gestor, Roles.Agente, Roles.Cliente)]
        public IActionResult Index()
        {
            string bearer = Request.Headers.Authorization[0].Replace("Bearer ", "");
            int? userId = _app.GetUserId(bearer);
            var userRole = _app.GetUserRole(bearer);
            if (userId == null || userRole == null) return BadRequest("Invalid User");

            switch (userRole)
            {
                case Roles.Admin:
                    if (_db.Apolices != null)
                    {
                        var objApoliceList = (from ap in _db.Apolices
                                              select new
                                              {
                                                  ap.Id,
                                                  ap.Ativa,
                                                  ap.Premio,
                                                  ap.Validade,
                                                  ap.Fracionamento,
                                                  ap.Simulacao,
                                                  ap.Seguro,
                                                  ap.Agente,
                                                  ap.ApolicePessoals,
                                                  ap.ApoliceSaudes,
                                                  ap.ApoliceVeiculos,
                                                  ap.CoberturaHasApolices
                                              }).ToList();

                        _logger.SetLogInfoGetAll(userId, "Apolice");

                        return Ok(objApoliceList);
                    }
                    else return NotFound();

                case Roles.Gestor:
                    int? equipaId = _app.GetEquipaId(userId);
                    if (equipaId == null) return BadRequest("Invalid Gestor");

                    var apolicesGestor = (from ap in _db.Apolices
                                          join agente in _db.Agentes on ap.AgenteId equals agente.Id
                                          where agente.EquipaId == equipaId
                                          select new
                                          {
                                              ap.Id,
                                              ap.Ativa,
                                              ap.Premio,
                                              ap.Validade,
                                              ap.Fracionamento,
                                              ap.Simulacao,
                                              ap.Seguro,
                                              ap.Agente,
                                              ap.ApolicePessoals,
                                              ap.ApoliceSaudes,
                                              ap.ApoliceVeiculos,
                                              ap.CoberturaHasApolices
                                          }).ToList();
                    var apolicesGestorNull = (from ap in _db.Apolices
                                              where ap.AgenteId == null
                                              select new
                                              {
                                                  ap.Id,
                                                  ap.Ativa,
                                                  ap.Premio,
                                                  ap.Validade,
                                                  ap.Fracionamento,
                                                  ap.Simulacao,
                                                  ap.Seguro,
                                                  ap.Agente,
                                                  ap.ApolicePessoals,
                                                  ap.ApoliceSaudes,
                                                  ap.ApoliceVeiculos,
                                                  ap.CoberturaHasApolices
                                              }).ToList();
                    foreach (var ap in apolicesGestorNull)
                    {
                        apolicesGestor.Add(ap);
                    }

                    _logger.SetLogInfoGetAll(userId, "Apolice");

                    return Ok(apolicesGestor);

                case Roles.Agente:
                    int? agenteId = _app.GetAgenteId(userId);
                    if (agenteId == null) return BadRequest("Invalid Agente");

                    var apolicesAgente = (from ap in _db.Apolices
                                          where ap.AgenteId == agenteId
                                          select new
                                          {
                                              ap.Id,
                                              ap.Ativa,
                                              ap.Premio,
                                              ap.Validade,
                                              ap.Fracionamento,
                                              ap.Simulacao,
                                              ap.Seguro,
                                              ap.Agente,
                                              ap.ApolicePessoals,
                                              ap.ApoliceSaudes,
                                              ap.ApoliceVeiculos,
                                              ap.CoberturaHasApolices
                                          }).ToList();

                    _logger.SetLogInfoGetAll(userId, "Apolice");

                    return Ok(apolicesAgente);

                case Roles.Cliente:
                    int? clienteId = _app.GetClienteId(userId);
                    if (clienteId == null) return BadRequest("Invalid Cliente");

                    var apolicesS = (from ap in _db.Apolices
                                     join apsaude in _db.ApoliceSaudes on ap.Id equals apsaude.ApoliceId
                                     where apsaude.ClienteId == clienteId
                                     select new
                                     {
                                         ap.Id,
                                         ap.Ativa,
                                         ap.Premio,
                                         ap.Validade,
                                         ap.Fracionamento,
                                         ap.Simulacao,
                                         ap.Seguro,
                                         ap.Agente,
                                         ap.ApolicePessoals,
                                         ap.ApoliceSaudes,
                                         ap.ApoliceVeiculos,
                                         ap.CoberturaHasApolices
                                     }).ToList();
                    var apolicesP = (from ap in _db.Apolices
                                     join appessoal in _db.ApolicePessoals on ap.Id equals appessoal.ApoliceId
                                     where appessoal.ClienteId == clienteId
                                     select new
                                     {
                                         ap.Id,
                                         ap.Ativa,
                                         ap.Premio,
                                         ap.Validade,
                                         ap.Fracionamento,
                                         ap.Simulacao,
                                         ap.Seguro,
                                         ap.Agente,
                                         ap.ApolicePessoals,
                                         ap.ApoliceSaudes,
                                         ap.ApoliceVeiculos,
                                         ap.CoberturaHasApolices
                                     }).ToList();
                    foreach (var ap in apolicesP)
                    {
                        apolicesS.Add(ap);
                    }
                    var apolicesV = (from ap in _db.Apolices
                                     join apveiculos in _db.ApoliceVeiculos on ap.Id equals apveiculos.ApoliceId
                                     join veiculo in _db.Veiculos on apveiculos.VeiculoId equals veiculo.Id
                                     where veiculo.ClienteId == clienteId
                                     select new
                                     {
                                         ap.Id,
                                         ap.Ativa,
                                         ap.Premio,
                                         ap.Validade,
                                         ap.Fracionamento,
                                         ap.Simulacao,
                                         ap.Seguro,
                                         ap.Agente,
                                         ap.ApolicePessoals,
                                         ap.ApoliceSaudes,
                                         ap.ApoliceVeiculos,
                                         ap.CoberturaHasApolices
                                     }).ToList();
                    foreach (var apv in apolicesV)
                    {
                        apolicesS.Add(apv);
                    }

                    _logger.SetLogInfoGetAll(userId, "Apolice");

                    return Ok(apolicesS);
                default: return BadRequest();
            }
        }

        /// <summary>
        /// Apolice Get by Id Route
        /// This route can only be accessed by authenticad users, any role can access it.
        /// The returned content depends on the role of the user and his permissions to view.
        /// Admins can see any Apolice.
        /// Gestores can only see the Apolice if it Apolice is managed by his Agentes
        /// Agentes can only see the Apolice if it Apolice is managed by himselves
        /// Cliente can only see the Apolice if it is his own.
        /// </summary>
        /// <param name="id">ApoliceId to get</param>
        /// <returns>Apolice List, size one or zero</returns>
        [HttpGet("Apolice/{id}"), Auth(Roles.Admin, Roles.Gestor, Roles.Agente, Roles.Cliente)]
        public IActionResult Get(int id)
        {
            string bearer = Request.Headers.Authorization[0].Replace("Bearer ", "");
            int? userId = _app.GetUserId(bearer);
            var userRole = _app.GetUserRole(bearer);
            if (userId == null || userRole == null) return BadRequest("Invalid User");

            switch (userRole)
            {
                case Roles.Admin:
                    var objApoliceList = (from ap in _db.Apolices
                                          where ap.Id == id
                                          select new
                                          {
                                              ap.Id,
                                              ap.Ativa,
                                              ap.Premio,
                                              ap.Validade,
                                              ap.Fracionamento,
                                              ap.Simulacao,
                                              ap.Seguro,
                                              ap.Agente,
                                              ap.ApolicePessoals,
                                              ap.ApoliceSaudes,
                                              ap.ApoliceVeiculos,
                                              ap.CoberturaHasApolices
                                          }).ToList();

                    _logger.SetLogInfoGet(userId, "Apolice", id);

                    return Ok(objApoliceList);

                case Roles.Gestor:
                    int? equipaId = _app.GetEquipaId(userId);
                    if (equipaId == null) return BadRequest("Invalid Gestor");

                    var apolices = (from ap in _db.Apolices
                                    join agente in _db.Agentes on ap.AgenteId equals agente.Id
                                    where agente.EquipaId == equipaId && ap.Id == id
                                    select new
                                    {
                                        ap.Id,
                                        ap.Ativa,
                                        ap.Premio,
                                        ap.Validade,
                                        ap.Fracionamento,
                                        ap.Simulacao,
                                        ap.Seguro,
                                        ap.Agente,
                                        ap.ApolicePessoals,
                                        ap.ApoliceSaudes,
                                        ap.ApoliceVeiculos,
                                        ap.CoberturaHasApolices
                                    }).ToList();

                    _logger.SetLogInfoGet(userId, "Apolice", id);

                    return Ok(apolices);

                case Roles.Agente:
                    int? agenteId = _app.GetAgenteId(userId);
                    if (agenteId == null) return BadRequest("Invalid Agente");
                    var apolicesAgente = (from ap in _db.Apolices
                                          where ap.AgenteId == agenteId && ap.Id == id
                                          select new
                                          {
                                              ap.Id,
                                              ap.Ativa,
                                              ap.Premio,
                                              ap.Validade,
                                              ap.Fracionamento,
                                              ap.Simulacao,
                                              ap.Seguro,
                                              ap.Agente,
                                              ap.ApolicePessoals,
                                              ap.ApoliceSaudes,
                                              ap.ApoliceVeiculos,
                                              ap.CoberturaHasApolices
                                          }).ToList();

                    _logger.SetLogInfoGet(userId, "Apolice", id);

                    return Ok(apolicesAgente);

                case Roles.Cliente:
                    int? clienteId = _app.GetClienteId(userId);
                    if (clienteId == null) return BadRequest("Invalid Cliente");

                    var apolicesS = (from ap in _db.Apolices
                                     join apsaude in _db.ApoliceSaudes on ap.Id equals apsaude.ApoliceId
                                     where apsaude.ClienteId == clienteId && ap.Id == id
                                     select new
                                     {
                                         ap.Id,
                                         ap.Ativa,
                                         ap.Premio,
                                         ap.Validade,
                                         ap.Fracionamento,
                                         ap.Simulacao,
                                         ap.Seguro,
                                         ap.Agente,
                                         ap.ApolicePessoals,
                                         ap.ApoliceSaudes,
                                         ap.ApoliceVeiculos,
                                         ap.CoberturaHasApolices
                                     }).ToList();
                    var apolicesP = (from ap in _db.Apolices
                                     join appessoal in _db.ApolicePessoals on ap.Id equals appessoal.ApoliceId
                                     where appessoal.ClienteId == clienteId && ap.Id == id
                                     select new
                                     {
                                         ap.Id,
                                         ap.Ativa,
                                         ap.Premio,
                                         ap.Validade,
                                         ap.Fracionamento,
                                         ap.Simulacao,
                                         ap.Seguro,
                                         ap.Agente,
                                         ap.ApolicePessoals,
                                         ap.ApoliceSaudes,
                                         ap.ApoliceVeiculos,
                                         ap.CoberturaHasApolices
                                     }).ToList();
                    foreach (var app in apolicesP)
                    {
                        apolicesS.Add(app);
                    }
                    var apolicesV = (from ap in _db.Apolices
                                     join apveiculos in _db.ApoliceVeiculos on ap.Id equals apveiculos.ApoliceId
                                     join veiculo in _db.Veiculos on apveiculos.VeiculoId equals veiculo.Id
                                     where veiculo.ClienteId == clienteId && ap.Id == id
                                     select new
                                     {
                                         ap.Id,
                                         ap.Ativa,
                                         ap.Premio,
                                         ap.Validade,
                                         ap.Fracionamento,
                                         ap.Simulacao,
                                         ap.Seguro,
                                         ap.Agente,
                                         ap.ApolicePessoals,
                                         ap.ApoliceSaudes,
                                         ap.ApoliceVeiculos,
                                         ap.CoberturaHasApolices
                                     }).ToList();
                    foreach (var apv in apolicesV)
                    {
                        apolicesS.Add(apv);
                    }

                    _logger.SetLogInfoGet(userId, "Apolice", id);

                    return Ok(apolicesS);

                default: return BadRequest();
            }
        }

        [HttpPut("{Id}"), Auth(Roles.Admin, Roles.Gestor, Roles.Agente, Roles.Cliente)]
        public IActionResult UpdateApolicePessoal(int Id, Apolice obj)
        {
            string bearer = Request.Headers.Authorization[0].Replace("Bearer ", "");
            int? userId = _app.GetUserId(bearer);
            var userRole = _app.GetUserRole(bearer);
            if (userId == null || userRole == null) return BadRequest(error: "Invalid User");
            if (!ModelState.IsValid) return BadRequest(error: "Invalid Object");

            var updateObj = _db.Apolices.Find(Id);
            if (updateObj == null) return BadRequest(error: "Invalid Id");

            string errorMessage;

            switch (userRole)
            {
                case Roles.Admin:
                    (errorMessage, updateObj) = _apolice.UpdateApolice(updateObj, obj);
                    if (errorMessage != "") return BadRequest(error: errorMessage);

                    return Ok(updateObj);

                case Roles.Gestor:
                    int? equipaId = _app.GetEquipaId(userId);
                    if (equipaId == null) return BadRequest("Invalid Gestor");

                    var agente = _db.Agentes.Find(updateObj.AgenteId);
                    if (agente.EquipaId != equipaId) return BadRequest("Permission Denied");

                    (errorMessage, updateObj) = _apolice.UpdateApolice(updateObj, obj);
                    if (errorMessage != "") return BadRequest(error: errorMessage);

                    return Ok(updateObj);

                case Roles.Agente:
                    int? agenteId = _app.GetAgenteId(userId);
                    if (agenteId == null) return BadRequest("Invalid Agente");

                    if (updateObj.AgenteId != agenteId) return BadRequest("Permission Denied");

                    (errorMessage, updateObj) = _apolice.UpdateApolice(updateObj, obj);
                    if (errorMessage != "") return BadRequest(error: errorMessage);

                    return Ok(updateObj);


                case Roles.Cliente:
                    int? clienteId = _app.GetClienteId(userId);
                    if (clienteId == null) return BadRequest("Invalid Cliente");

                    if (_apolice.GetClienteId(updateObj.Id) != clienteId) return BadRequest("Permission Denied");

                    (errorMessage, updateObj) = _apolice.UpdateApolice(updateObj, obj);
                    if (errorMessage != "") return BadRequest(error: errorMessage);

                    return Ok(updateObj);

                default: return BadRequest();
            }
        }


        /// <summary>
        /// This route can be accessed by admin and gestor
        /// it is used for them to associate an apolice to an agente.
        /// </summary>
        /// <returns></returns>
        [HttpPut("UpdateAgenteApolice"), Auth(Roles.Admin, Roles.Gestor)]
        public IActionResult UpdateAgenteApolice(UpdateAgenteApolice obj)
        {
            string bearer = Request.Headers.Authorization[0].Replace("Bearer ", "");
            int? userId = _app.GetUserId(bearer);
            var userRole = _app.GetUserRole(bearer);
            if (userId == null || userRole == null) return BadRequest(error: "Invalid User");
            if (!ModelState.IsValid) return BadRequest(error: "Invalid Object");

            //Verifications
            if (_db.Apolices.Find(obj.apoliceId) == null) return BadRequest(error: "Invalid Apolice");
            if (_db.Agentes.Find(obj.agenteId) == null) return BadRequest(error: "Invalid Agente");


            switch (userRole)
            {
                case Roles.Admin:

                    //Finds and updates apolice with new ID
                    var apoliceUpdate = _db.Apolices.Find(obj.apoliceId);
                    apoliceUpdate.AgenteId = obj.agenteId;

                    _db.Apolices.Update(apoliceUpdate);
                    _db.SaveChanges();

                    return Ok(apoliceUpdate);

                case Roles.Gestor:
                    //gets agente id of gestor
                    var gestorId = _app.GetAgenteId(userId);
                    
                    //gets the equipa id of gestor
                    var equipaId = _db.Agentes.Select(x => x).Where(x=> x.Id == gestorId).First().EquipaId;
                    if (gestorId == null || equipaId == null) return BadRequest(error: "Invalid Gestor");

                    //Verifies if gestor is from same equipa as agente
                    var agente = _db.Agentes.Find(obj.agenteId);
                    if (agente.EquipaId != equipaId) return BadRequest(error: "Cannot Associate apolice to that Agente");

                    //Finds and updates apolice with new ID
                    var apolice = _db.Apolices.Find(obj.apoliceId);
                    apolice.AgenteId = obj.agenteId;

                    _db.Apolices.Update(apolice);
                    _db.SaveChanges();

                    return Ok(apolice);

                default: return BadRequest("Error With token");
            }
            

        }





        /// <summary>
        /// Apolice Add Cobertura Route
        /// This route can only be accessed by authenticad users, any role can access it.
        /// The update permissions depends on the role of the user
        /// Admins can update any Apolice.
        /// Gestores can update the Apolice if it Apolice is managed by his Agentes
        /// Agentes can only update the Apolice if it Apolice is managed by himselves
        /// Cliente can only update the Apolice if it is his own.
        /// </summary>
        /// <param name="ApId">ApoliceId</param>
        /// <param name="CId">CoberturaId</param>
        /// <returns>If sucessfull added CoberturaId</returns>
        [HttpPost("Apolice/{ApId}/Coberturas/{CId}/"), Auth(Roles.Admin, Roles.Gestor, Roles.Agente, Roles.Cliente)]
        public IActionResult AddCobertura(int ApId, int CId)
        {
            string bearer = Request.Headers.Authorization[0].Replace("Bearer ", "");
            int? userId = _app.GetUserId(bearer);
            var userRole = _app.GetUserRole(bearer);
            if (userId == null || userRole == null) return BadRequest("Invalid User");

            switch (userRole)
            {
                case Roles.Admin:
                    var apolice = _db.Apolices.Find(ApId);
                    if (!_apolice.CanUpdate(apolice.Id)) return BadRequest("Permission Denied");


                    //check if cobertura is already in this seguro
                    var coberturaCheck = (from co in _db.CoberturaHasApolices
                                          where co.CoberturaId == CId && co.ApoliceId == ApId
                                          select co).ToList().FirstOrDefault();

                    if (coberturaCheck == null)
                    {
                        //check if cobertura can be added to this seguro
                        var seguro = _db.Seguros.Find(apolice.SeguroId);
                        var cob = _db.Coberturas.Find(CId);
                        if (cob.SeguroId != seguro.Id) return BadRequest("Invalid Cobertura");

                        var cobertura = new CoberturaHasApolice();
                        cobertura.CoberturaId = CId;
                        cobertura.ApoliceId = apolice.Id;
                        _db.CoberturaHasApolices.Add(cobertura);
                        _db.SaveChanges();

                        return Ok(cobertura.Id);
                    }
                    else return BadRequest("Cobertura already added");

                case Roles.Gestor:
                    int? equipaId = _app.GetEquipaId(userId);
                    if (equipaId == null) return BadRequest("Invalid Gestor");

                    var apoliceGestor = (from ap in _db.Apolices
                                         join agente in _db.Agentes on ap.AgenteId equals agente.Id
                                         where ap.Id == ApId && agente.EquipaId == equipaId
                                         select ap).ToList().FirstOrDefault();
                    if (apoliceGestor == null) return BadRequest("Permission Denied");
                    if (!_apolice.CanUpdate(apoliceGestor.Id)) return BadRequest("Permission Denied");

                    //check if cobertura is already in this seguro
                    var coberturaGestorCheck = (from co in _db.CoberturaHasApolices
                                                where co.CoberturaId == CId && co.ApoliceId == ApId
                                                select co).ToList().FirstOrDefault();
                    if (coberturaGestorCheck == null)
                    {
                        //check if cobertura can be added to this seguro
                        var seguro = _db.Seguros.Find(apoliceGestor.SeguroId);
                        var cob = _db.Coberturas.Find(CId);
                        if (cob.SeguroId != seguro.Id) return BadRequest("Invalid Cobertura");

                        var cobertura = new CoberturaHasApolice();
                        cobertura.CoberturaId = CId;
                        cobertura.ApoliceId = apoliceGestor.Id;
                        _db.CoberturaHasApolices.Add(cobertura);
                        _db.SaveChanges();

                        return Ok(cobertura.Id);
                    }
                    else return BadRequest("Cobertura already added");

                case Roles.Agente:
                    int? agenteId = _app.GetAgenteId(userId);
                    if (agenteId == null) return BadRequest("Invalid Agente");

                    var apoliceAgente = (from ap in _db.Apolices
                                         where ap.Id == ApId && ap.AgenteId == agenteId
                                         select ap).ToList().FirstOrDefault();
                    if (apoliceAgente == null) return BadRequest("Permission Denied");
                    if (!_apolice.CanUpdate(apoliceAgente.Id)) return BadRequest("Permission Denied");



                    //check if cobertura is already in this seguro
                    var coberturaCheckAgente = (from co in _db.CoberturaHasApolices
                                                where co.CoberturaId == CId && co.ApoliceId == ApId
                                                select co).ToList().FirstOrDefault();

                    if (coberturaCheckAgente == null)
                    {
                        //check if cobertura can be added to this seguro
                        var seguro = _db.Seguros.Find(apoliceAgente.SeguroId);
                        var cob = _db.Coberturas.Find(CId);
                        if (cob.SeguroId != seguro.Id) return BadRequest("Invalid Cobertura");

                        var cobertura = new CoberturaHasApolice();
                        cobertura.CoberturaId = CId;
                        cobertura.ApoliceId = apoliceAgente.Id;
                        _db.CoberturaHasApolices.Add(cobertura);
                        _db.SaveChanges();

                        return Ok(cobertura.Id);
                    }
                    else return BadRequest("Cobertura already added");

                case Roles.Cliente:
                    int? clienteId = _app.GetClienteId(userId);
                    if (clienteId == null) return BadRequest("Invalid Cliente");

                    var apoliceS = (from ap in _db.Apolices
                                    join apS in _db.ApoliceSaudes on ap.Id equals apS.ApoliceId
                                    where ap.Id == ApId && apS.ClienteId == clienteId
                                    select ap).ToList().FirstOrDefault();

                    var apoliceP = (from ap in _db.Apolices
                                    join apP in _db.ApolicePessoals on ap.Id equals apP.ApoliceId
                                    where ap.Id == ApId && apP.ClienteId == clienteId
                                    select ap).ToList().FirstOrDefault();

                    var apoliceV = (from ap in _db.Apolices
                                    join apV in _db.ApoliceVeiculos on ap.Id equals apV.ApoliceId
                                    join veiculo in _db.Veiculos on apV.VeiculoId equals veiculo.Id
                                    where ap.Id == ApId && veiculo.ClienteId == clienteId
                                    select ap).ToList().FirstOrDefault();

                    if (apoliceS == null && apoliceP == null && apoliceV == null) return BadRequest("Permission Denied");

                    var coberturaCheckCliente = (from co in _db.CoberturaHasApolices
                                                 where co.CoberturaId == CId && co.ApoliceId == ApId
                                                 select co).ToList().FirstOrDefault();

                    Apolice? apoliceCliente = null;
                    if (apoliceS != null) apoliceCliente = apoliceS;
                    if (apoliceP != null) apoliceCliente = apoliceP;
                    if (apoliceV != null) apoliceCliente = apoliceV;
                    if (apoliceCliente != null)
                    {
                        apolice = apoliceS;
                        if (coberturaCheckCliente != null)
                        {
                            if (!_apolice.CanUpdate(apoliceCliente.Id)) return BadRequest("Permission Denied");

                            //check if cobertura can be added to this seguro
                            var seguro = _db.Seguros.Find(apoliceCliente.SeguroId);
                            var cob = _db.Coberturas.Find(CId);
                            if (cob.SeguroId != seguro.Id) return BadRequest("Invalid Cobertura");

                            var cobertura = new CoberturaHasApolice();
                            cobertura.CoberturaId = CId;
                            cobertura.ApoliceId = apoliceCliente.Id;
                            _db.CoberturaHasApolices.Add(cobertura);
                            _db.SaveChanges();

                            return Ok(cobertura.Id);
                        }
                        else return BadRequest("Cobertura already added");
                    }
                    else return BadRequest();

                default: return BadRequest();
            }
        }

        /// <summary>
        /// Apolice Delete Cobertura Route
        /// This route can only be accessed by authenticad users, any role can access it.
        /// The delete permissions depends on the role of the user
        /// Admins can delete in any Apolice.
        /// Gestores can delete in the Apolice if it Apolice is managed by his Agentes
        /// Agentes can only delete in the Apolice if it Apolice is managed by himselves
        /// Cliente can only delete in the Apolice if it is his own.
        /// </summary>
        /// <param name="ApId">ApoliceId</param>
        /// <param name="CId">CoberturaId</param>
        /// <returns>If sucessfull deleted CoberturaId</returns>
        [HttpDelete("Apolice/{ApId}/Coberturas/{CId}/"), Auth(Roles.Admin)]
        public IActionResult DeleteCobertura(int ApId, int CId)
        {
            string bearer = Request.Headers.Authorization[0].Replace("Bearer ", "");
            int? userId = _app.GetUserId(bearer);
            var userRole = _app.GetUserRole(bearer);
            if (userId == null || userRole == null) return BadRequest("Invalid User");

            switch (userRole)
            {
                case Roles.Admin:
                    var apolice = _db.Apolices.Find(ApId);
                    if (!_apolice.CanUpdate(apolice.Id)) return BadRequest("Permission Denied");

                    var cobertura = (from co in _db.CoberturaHasApolices
                                     where co.CoberturaId == CId && co.ApoliceId == ApId
                                     select co).ToList().FirstOrDefault();

                    if (cobertura == null) return BadRequest("Cobertura Not Present");

                    _db.CoberturaHasApolices.Remove(cobertura);
                    _db.SaveChanges();

                    _logger.SetLogInfoDelete(userId, "Apolice", ApId);

                    return Ok(cobertura.Id);

                case Roles.Gestor:
                    int? equipaId = _app.GetEquipaId(userId);
                    if (equipaId == null) return BadRequest("Invalid Gestor");

                    var apoliceGestor = (from ap in _db.Apolices
                                         join agente in _db.Agentes on ap.AgenteId equals agente.Id
                                         where ap.Id == ApId && agente.EquipaId == equipaId
                                         select ap).ToList().FirstOrDefault();
                    if (apoliceGestor == null) return BadRequest("Permission Denied");
                    if (!_apolice.CanUpdate(apoliceGestor.Id)) return BadRequest("Permission Denied");

                    var coberturaGestor = (from co in _db.CoberturaHasApolices
                                           where co.CoberturaId == CId && co.ApoliceId == ApId
                                           select co).ToList().FirstOrDefault();
                    if (coberturaGestor == null) return BadRequest("Cobertura Not Present");

                    _db.CoberturaHasApolices.Remove(coberturaGestor);
                    _db.SaveChanges();

                    _logger.SetLogInfoDelete(userId, "Apolice", ApId);

                    return Ok(coberturaGestor.Id);

                case Roles.Agente:
                    int? agenteId = _app.GetAgenteId(userId);
                    if (agenteId == null) return BadRequest("Invalid Agente");

                    var apoliceAgente = (from ap in _db.Apolices
                                         where ap.Id == ApId && ap.AgenteId == agenteId
                                         select ap).ToList().FirstOrDefault();
                    if (apoliceAgente == null) return BadRequest("Permission Denied");
                    if (!_apolice.CanUpdate(apoliceAgente.Id)) return BadRequest("Permission Denied");

                    var coberturaAgente = (from co in _db.CoberturaHasApolices
                                           where co.CoberturaId == CId && co.ApoliceId == ApId
                                           select co).ToList().FirstOrDefault();

                    if (coberturaAgente == null) return BadRequest("Cobertura Not Present");
                    _db.CoberturaHasApolices.Remove(coberturaAgente);
                    _db.SaveChanges();

                    _logger.SetLogInfoDelete(userId, "Apolice", ApId);

                    return Ok(coberturaAgente.Id);

                case Roles.Cliente:
                    int? clienteId = _app.GetClienteId(userId);
                    if (clienteId == null) return BadRequest("Invalid Cliente");

                    var apoliceS = (from ap in _db.Apolices
                                    join apS in _db.ApoliceSaudes on ap.Id equals apS.ApoliceId
                                    where ap.Id == ApId && apS.ClienteId == clienteId
                                    select ap).ToList().FirstOrDefault();

                    var apoliceP = (from ap in _db.Apolices
                                    join apP in _db.ApolicePessoals on ap.Id equals apP.ApoliceId
                                    where ap.Id == ApId && apP.ClienteId == clienteId
                                    select ap).ToList().FirstOrDefault();

                    var apoliceV = (from ap in _db.Apolices
                                    join apV in _db.ApoliceVeiculos on ap.Id equals apV.ApoliceId
                                    join veiculo in _db.Veiculos on apV.VeiculoId equals veiculo.Id
                                    where ap.Id == ApId && veiculo.ClienteId == clienteId
                                    select ap).ToList().FirstOrDefault();

                    if (apoliceS == null && apoliceP == null && apoliceV == null) return BadRequest("Permission Denied");

                    var coberturaCliente = (from co in _db.CoberturaHasApolices
                                            where co.CoberturaId == CId && co.ApoliceId == ApId
                                            select co).ToList().FirstOrDefault();

                    if (coberturaCliente == null) return BadRequest("Cobertura Not Present");

                    Apolice? apoliceCliente = null;
                    if (apoliceS != null) apoliceCliente = apoliceS;
                    if (apoliceP != null) apoliceCliente = apoliceP;
                    if (apoliceV != null) apoliceCliente = apoliceV;
                    if (apoliceCliente != null)
                    {
                        if (!_apolice.CanUpdate(apoliceCliente.Id)) return BadRequest("Permission Denied");
                        _db.CoberturaHasApolices.Remove(coberturaCliente);
                        _db.SaveChanges();
                        return Ok(coberturaCliente.Id);
                    }
                    else return BadRequest();

                default: return BadRequest();
            }
        }

        #region Simulacao - Cliente

        private ApoliceSaude? SimulacaoPessoal()
        {
            throw new NotImplementedException();
        }

        private ApolicePessoal? SimulacaoVida()
        {
            throw new NotImplementedException();
        }
        #endregion
    }


    /// <summary>
    /// Object to update apolice and set new agente.
    /// </summary>
    public class UpdateAgenteApolice
    { 
        public int agenteId { get; set; }
        public int apoliceId { get; set; }
    }
}
