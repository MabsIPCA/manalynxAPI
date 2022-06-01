using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ManaLynxAPI.Data;
using ManaLynxAPI.Models;
using ManaLynxAPI;
using System.Security.Claims;
using System.Data.SqlClient;
using Microsoft.AspNetCore.Authorization;
using Auth = ManaLynxAPI.Authentication.Auth;
using Roles = ManaLynxAPI.Models.Roles;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.EntityFrameworkCore;
using ManaLynxAPI.Utils;
using Newtonsoft.Json;

namespace ManaLynxAPI.Controllers
{   

    /// <summary>
    /// Controller for the Agente table from the database
    /// </summary>
    [Authorize]
    [ApiController, Route("[controller]")]
    public class AgenteController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly IAgenteUtils _ageUtils;
        private readonly IPessoaUtils _pesUtils;
        private readonly ILoggerUtils _logger;
        private readonly IAppUtils _app;

        public AgenteController(ApplicationDbContext db, IAgenteUtils ageUtils, IPessoaUtils pesUtils, ILoggerUtils logger, IAppUtils app)
        {
            _db = db;
            _pesUtils = pesUtils;
            _logger = logger;
            _app = app;
            _ageUtils = ageUtils;
        }


        #region Rotas
        /// <summary>
        /// Gets all Agentes present in the Database
        /// This route can be accessed by two roles: Admin and Gestor
        /// Admin can view all agentes in the DB while Gestor can only view Agentes that are from his own Equipa
        /// </summary>
        /// <returns></returns>
        [HttpGet, Auth(Roles.Admin, Roles.Gestor)]
        public IActionResult Index()
        {

            //Gets the Bearer token info from request
            var jwtSecurityToken = new JwtSecurityTokenHandler().ReadJwtToken(Request.Headers.Authorization[0].Replace("Bearer ", ""));
            var idToken = jwtSecurityToken.Claims.First(claim => claim.Type == "Id").Value;
            var roleToken = jwtSecurityToken.Claims.First(claim => claim.Type == "role").Value;
            var nameToken = jwtSecurityToken.Claims.First(claim => claim.Type == "name").Value;
            int id;


            if (roleToken == "Admin")
            {
                if (_db.Agentes != null)
                {
                    var objAgenteList = _db.Agentes.Select(c => new { c.Id, c.Nagente, c.EquipaId, c.Pessoa }).ToList();
                    var token = Request.Headers.Authorization[0].Replace("Bearer ", "");
                    _logger.SetLogInfoGetAll(_app.GetUserId(token), "Agente");
                    return Ok(objAgenteList);

                }
                else return BadRequest(error:"No Agente Found");
            }

            else
            {
                //Tries to parse token 
                bool success = Int32.TryParse(idToken, out id);
                if (!success) return BadRequest(error: "Token Format invalid");
                var equipaId = _app.GetEquipaId(id);


                if (_db.Agentes != null)
                {
                    //Returns only the Agentes that are from the same Equipa as the Gestor acessing the route
                    var objAgenteList = _db.Agentes.Where(x => x.EquipaId == equipaId).Select(c => new { c.Id, c.Nagente, c.EquipaId, c.Pessoa }).ToList();
                    var token = Request.Headers.Authorization[0].Replace("Bearer ", "");
                    _logger.SetLogInfoGetAll(_app.GetUserId(token), "Agente");
                    return Ok(objAgenteList);

                }
                else return BadRequest(error: "No Agente Found");
            }
            
        }

        /// <summary>
        /// Gets an Agente present in the DB by id
        /// Only Admin can access this route
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        [HttpGet("{Id}"), Auth(Roles.Admin)]
        public IActionResult ViewByEquipaId(int Id)
        { 
            if (Id == null || Id == 0) return BadRequest(error: "No Agente Found");

            var objAgenteList = _db.Agentes.Where(x => x.EquipaId == Id).Select(c => new { c.Id, nome = c.Pessoa.Nome, c.Nagente, apolices = c.Apolices.Count(), isGestor = _db.Gestors.Where(g => g.AgenteId == c.Id).Any() }).ToList();
            

            if (objAgenteList == null) return BadRequest(error: "No Agentes Found");


            var token = Request.Headers.Authorization[0].Replace("Bearer ", "");
            _logger.SetLogInfoGet(_app.GetUserId(token), "Agente", Id);
            return Ok(objAgenteList);
        }

        /// <summary>
        /// Creates an Agente for the DB
        /// Both Admin and Gestor can access this route
        /// Admin can create Agente in wichever team he wishes, as long as it exists (duh!)
        /// Gestor can only create Agentes for his own team
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        [HttpPost("create"), Auth(Roles.Admin, Roles.Gestor)]
        public IActionResult Create(Agente obj)
        {
            //Gets the Bearer token info from request
            var jwtSecurityToken = new JwtSecurityTokenHandler().ReadJwtToken(Request.Headers.Authorization[0].Replace("Bearer ", ""));
            var idToken = jwtSecurityToken.Claims.First(claim => claim.Type == "Id").Value;
            var roleToken = jwtSecurityToken.Claims.First(claim => claim.Type == "role").Value;
            var nameToken = jwtSecurityToken.Claims.First(claim => claim.Type == "name").Value;
            int id;
            
            //Initializes response variables
            var objectUtils = new Agente();
            var responseUtils = string.Empty;


            //Actions if Admin is accessing route
            if (roleToken == "Admin")
            {

                if (ModelState.IsValid && obj.EquipaId != 0)
                {
                    //Calls function from utils
                    (objectUtils, responseUtils) = _ageUtils.CreateAgente(obj);

                    if (objectUtils == null) return BadRequest(error: responseUtils);
                    
                    //Logs
                    var json = JsonConvert.SerializeObject(objectUtils, new JsonSerializerSettings() { MaxDepth = 1, ReferenceLoopHandling = ReferenceLoopHandling.Ignore });
                    var token = Request.Headers.Authorization[0].Replace("Bearer ", "");
                    _logger.SetLogInfoPost(_app.GetUserId(token), "Agente", json);

                    return Ok(objectUtils);
                }
                return BadRequest(obj);
            }


            //Actions if Gestor is accessing route if(roleToken == "Gestor")
            else
            {
                //Tries to parse token 
                bool success = Int32.TryParse(idToken, out id);
                if (!success) return BadRequest(error: "Token Format invalid");

                //Verifies who the user is and if he is in the same Equipa as the Agente he wants to remov
                var equipaId = (from pessoa in _db.Pessoas
                                join manauser in _db.ManaUsers on pessoa.Id equals manauser.PessoaId
                                where manauser.Id == id
                                join agente in _db.Agentes on pessoa.Id equals agente.PessoaId
                                join equipa in _db.Equipas on agente.EquipaId equals equipa.Id
                                select equipa.Id)
                                .ToList()
                                .FirstOrDefault();

                if (obj.EquipaId != equipaId) return BadRequest(error: "There is no Gestor associated with that Equipa");

                if (ModelState.IsValid)
                {
                    //Calls function from utils
                    (objectUtils, responseUtils) = _ageUtils.CreateAgente(obj);

                    if (objectUtils == null) return BadRequest(error: responseUtils);

                    //Logs
                    var json = JsonConvert.SerializeObject(objectUtils, new JsonSerializerSettings() { MaxDepth = 1, ReferenceLoopHandling = ReferenceLoopHandling.Ignore });
                    var token = Request.Headers.Authorization[0].Replace("Bearer ", "");
                    _logger.SetLogInfoPost(_app.GetUserId(token), "Agente", json);

                    return Ok(objectUtils);

                }
                return BadRequest(error: "model state invalid");
            }

        }

        /// <summary>
        /// Edits an agente from the DB
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        [HttpPut("edit"), Auth(Roles.Admin, Roles.Gestor)]
        public IActionResult Edit(Agente obj)
        {
            //Gets the Bearer token info from request
            var jwtSecurityToken = new JwtSecurityTokenHandler().ReadJwtToken(Request.Headers.Authorization[0].Replace("Bearer ", ""));
            var idToken = jwtSecurityToken.Claims.First(claim => claim.Type == "Id").Value;
            var roleToken = jwtSecurityToken.Claims.First(claim => claim.Type == "role").Value;
            var nameToken = jwtSecurityToken.Claims.First(claim => claim.Type == "name").Value;
            int id;

            //Initializes response variables
            var objectUtils = new Agente();
            var responseUtils = string.Empty;


            //Actions if Admin is accessing route
            if (roleToken == "Admin")
            {
                if (ModelState.IsValid)
                {
                    //Calls function from utils
                    (objectUtils, responseUtils) = _ageUtils.UpdateAgente(obj);

                    if (objectUtils == null) return BadRequest(error: responseUtils);

                    //Logs
                    var json = JsonConvert.SerializeObject(objectUtils, new JsonSerializerSettings() { MaxDepth = 1, ReferenceLoopHandling = ReferenceLoopHandling.Ignore });
                    var token = Request.Headers.Authorization[0].Replace("Bearer ", "");
                    _logger.SetLogInfoPost(_app.GetUserId(token), "Agente", json);

                    return Ok(objectUtils);

                }
                return BadRequest(error:"Model state invalid");
            }


            //Actions if Gestor is accessing route if(roleToken == "Gestor")
            else
            {
                //Tries to parse token 
                bool success = Int32.TryParse(idToken, out id);
                if (!success) return BadRequest(error: "Token Format invalid");

                //Verifies who the user is and if he is in the same Equipa as the Agente he wants to remove
                var equipaId = (from pessoa in _db.Pessoas
                                join manauser in _db.ManaUsers on pessoa.Id equals manauser.PessoaId
                                where manauser.Id == id
                                join agente in _db.Agentes on pessoa.Id equals agente.PessoaId
                                join equipa in _db.Equipas on agente.EquipaId equals equipa.Id
                                select equipa.Id)
                                .ToList()
                                .FirstOrDefault();

                if (obj.EquipaId != equipaId) return BadRequest(error: "There is no Gestor associated with that Agente");
                
                if (ModelState.IsValid)
                {
                    //Calls function from utils
                    (objectUtils, responseUtils) = _ageUtils.UpdateAgente(obj);

                    if (objectUtils == null) return BadRequest(error: responseUtils);

                    //Logs
                    var json = JsonConvert.SerializeObject(objectUtils, new JsonSerializerSettings() { MaxDepth = 1, ReferenceLoopHandling = ReferenceLoopHandling.Ignore });
                    var token = Request.Headers.Authorization[0].Replace("Bearer ", "");
                    _logger.SetLogInfoPost(_app.GetUserId(token), "Agente", json);

                    return Ok(objectUtils);

                }
                return BadRequest(error:"Model State Invalid");
            }

            
        }

        /// <summary>
        /// Completely Deletes an Agente from the DB
        /// Only Admin can access this route
        /// It should be done with extra care as it might break something
        /// because of reference deletion
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        [HttpDelete("delete"), Auth(Roles.Admin)]
        public IActionResult Delete(int Id)
        {
            var obj = _db.Agentes.Find(Id);
            if (obj == null)
            {
                return BadRequest(error: "No Agente Found");
            }
            _db.Agentes.Remove(obj);
            _db.SaveChanges();

            var token = Request.Headers.Authorization[0].Replace("Bearer ", "");
            _logger.SetLogInfoDelete(_app.GetUserId(token), "Agente", Id);

            return Ok();
        }

        /// <summary>
        /// Get agente from a given User Id
        /// </summary>
        /// <returns>Agente List with 0 or 1</returns>
        [HttpGet("AgenteByUserId"), Auth(Roles.Gestor, Roles.Agente, Roles.Admin)]
        public IActionResult AgenteByUserId()
        {
            var token = Request.Headers.Authorization[0].Replace("Bearer ", "");
            var reqId = _app.GetUserId(token);

            var agenteId = _app.GetAgenteId(reqId);
            if (agenteId == null) return BadRequest(error: "Not valid Agente");
            var a = from agente in _db.Agentes
                    where agente.Id == agenteId
                    select new { agente.Id, agente.Nagente, agente.EquipaId, agente.PessoaId };
            return Ok(a);
        }

        /// <summary>
        /// Get agente from a given User Id
        /// </summary>
        /// <returns>Agente List with 0 or 1</returns>
        [HttpGet("AgentePessoaId/{id}"), Auth(Roles.Gestor, Roles.Agente, Roles.Cliente,Roles.Admin)]
        public IActionResult AgentePessoaId(int id)
        {
            var agente = _db.Agentes.Find(id);
            if (agente == null) return BadRequest(error: "Not valid Agente");
            var a = from pessoa in _db.Pessoas
                    where pessoa.Id == agente.PessoaId
                    select new { pessoa.Id, pessoa.Nome, pessoa.DataNascimento, pessoa.Nacionalidade, pessoa.Cc, pessoa.ValidadeCc, pessoa.Nif, pessoa.Nss, pessoa.Nus, pessoa.EstadoCivil};
            return Ok(a);
        }
        #endregion



    }

}
