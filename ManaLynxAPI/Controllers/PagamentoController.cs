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
    [Authorize]
    [ApiController, Route("[controller]")]
    public class PagamentoController : Controller
    {

        private readonly ApplicationDbContext _db;
        private readonly ILoggerUtils _logger;
        private readonly IAppUtils _app;

        public PagamentoController(ApplicationDbContext db, ILoggerUtils logger, IAppUtils app)
        {
            _db = db;
            _logger = logger;
            _app = app;
        }


        /// <summary>
        /// Gets all Pagamentos from the DB
        /// Everyone can access this route
        /// Admin Gestor and Agente can view all pagamentos
        /// Cliente can only view his own
        /// </summary>
        /// <returns></returns>
        [HttpGet, Auth]
        public IActionResult Index()
        {
            //Gets the Bearer token info from request
            var jwtSecurityToken = new JwtSecurityTokenHandler().ReadJwtToken(Request.Headers.Authorization[0].Replace("Bearer ", ""));
            var idToken = jwtSecurityToken.Claims.First(claim => claim.Type == "Id").Value;
            var roleToken = jwtSecurityToken.Claims.First(claim => claim.Type == "role").Value;
            var nameToken = jwtSecurityToken.Claims.First(claim => claim.Type == "name").Value;
            int id;

            if (_db.Pagamentos != null)
            {
                switch (roleToken)
                {
                    // For everyone except Cliente
                    case "Admin":
                    case "Gestor":
                    case "Agente":
                        var objPagamentoList = _db.Pagamentos.Select(c => new { c.Id, c.Metodo, c.DataEmissao, c.DataPagamento, c.Montante, c.ApoliceId }).ToList();

                        var token = Request.Headers.Authorization[0].Replace("Bearer ", "");
                        _logger.SetLogInfoGetAll(_app.GetUserId(token), "Pagamento");

                        return Ok(objPagamentoList);
                    // For Cliente
                    case "Cliente":
        
                        //Parse role token
                        bool success = Int32.TryParse(idToken, out id);
                        if (!success)
                        {
                            return BadRequest("Token Format invalid");
                        }

                        // Get the pagamentos only from the pessoa that is User
                        // Get Pagametos for ApolicePessoal
                        var objPagamentoPessoalList = (from manaUser in _db.ManaUsers
                                                     join pessoa in _db.Pessoas on manaUser.PessoaId equals pessoa.Id
                                                     where manaUser.Id == id
                                                     join cliente in _db.Clientes on pessoa.Id equals cliente.Id
                                                     join apoliceP in _db.ApolicePessoals on cliente.Id equals apoliceP.ClienteId
                                                     join apolice in _db.Apolices on apoliceP.ApoliceId equals apolice.Id
                                                     join pagamento in _db.Pagamentos on apolice.Id equals pagamento.ApoliceId
                                                     select pagamento).ToList();

                        // Get Pagametos for ApoliceSaude
                        var objPagamentoSaudeList = (from manaUser in _db.ManaUsers
                                                     join pessoa in _db.Pessoas on manaUser.PessoaId equals pessoa.Id
                                                     where manaUser.Id == id
                                                     join cliente in _db.Clientes on pessoa.Id equals cliente.Id
                                                     join apoliceS in _db.ApoliceSaudes on cliente.Id equals apoliceS.ClienteId
                                                     join apolice in _db.Apolices on apoliceS.ApoliceId equals apolice.Id
                                                     join pagamento in _db.Pagamentos on apolice.Id equals pagamento.ApoliceId
                                                     select pagamento).ToList();

                        // Get Pagametos for ApoliceVeiculo
                        var objPagamentoVeiculoList = (from manaUser in _db.ManaUsers
                                                join pessoa in _db.Pessoas on manaUser.PessoaId equals pessoa.Id
                                                where manaUser.Id == id
                                                join cliente in _db.Clientes on pessoa.Id equals cliente.Id
                                                join veiculo in _db.Veiculos on cliente.Id equals veiculo.ClienteId
                                                join apoliceV in _db.ApoliceVeiculos on veiculo.Id equals apoliceV.VeiculoId
                                                join apolice in _db.Apolices on apoliceV.ApoliceId equals apolice.Id
                                                join pagamento in _db.Pagamentos on apolice.Id equals pagamento.ApoliceId
                                                select pagamento).ToList();


                        // Concatenate Results
                        foreach(var pagamento in objPagamentoSaudeList)
                        {
                            objPagamentoPessoalList.Add(pagamento);
                        }

                        foreach(var pagamento in objPagamentoVeiculoList)
                        {
                            objPagamentoPessoalList.Add(pagamento);
                        }

                        // Verify if isnt null
                        if(objPagamentoPessoalList != null)
                        {
                            var token_ = Request.Headers.Authorization[0].Replace("Bearer ", "");
                            _logger.SetLogInfoGetAll(_app.GetUserId(token_), "Pagamento");

                            return Ok(objPagamentoPessoalList);
                        }
                        return NotFound();
                    default:
                        return BadRequest("Bad Token");

                }
            }
            else return NotFound();
        }

        /// <summary>
        /// Creates a Pagamento to DB
        /// Only Admin can access this route
        /// Should never be accessed as pagamentos are created autommatically
        /// with the method present in PagamentoUtils
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        [HttpPost("create"), Auth(Roles.Admin)]
        public IActionResult Create(Pagamento obj)
        {
            if (ModelState.IsValid)
            {
                var createObj = new Pagamento();

                //Assigns variables to the createObj
                createObj.Metodo = obj.Metodo;
                createObj.DataEmissao = obj.DataEmissao;
                createObj.DataPagamento = obj.DataPagamento;
                createObj.Montante = obj.Montante;
                createObj.ApoliceId = obj.ApoliceId;

                //Updates Seguros with the data given
                _db.Pagamentos.Add(createObj);
                _db.SaveChanges();

                var json = JsonConvert.SerializeObject(createObj, new JsonSerializerSettings() { MaxDepth = 1, ReferenceLoopHandling = ReferenceLoopHandling.Ignore });

                var token = Request.Headers.Authorization[0].Replace("Bearer ", "");
                _logger.SetLogInfoPost(_app.GetUserId(token), "Pagamento", json);

                return Ok(createObj);
            }
            return BadRequest(obj);
        }

        /// <summary>
        /// This route serves as a way for admin
        /// to simulate the Pagamento of an Apolice
        /// </summary>
        /// <param name="apoliceId"></param>
        /// <returns></returns>
        [HttpPut("pagar"), AllowAnonymous]
        public IActionResult PagarApolice(int apoliceId)
        {
            //Get the last pagamento of a given apolice
            var pagamentos = (from apolice in _db.Apolices
                                 join pagamento in _db.Pagamentos on apolice.Id equals pagamento.ApoliceId
                                 where apolice.Id == apoliceId
                                 where apolice.Ativa == true
                                 select pagamento);

            //Return error if no pagamento was found
            if (pagamentos.Count() == 0) return BadRequest(error: "No pagamento found for that apolice");

            //Gets last pagamento for a given apolice
            var lastPagamento = pagamentos.OrderBy(x => x.DataEmissao).Last();

            lastPagamento.DataPagamento = DateTime.Today;

            //Updates pagamento
            _db.Pagamentos.Update(lastPagamento);
            _db.SaveChanges();

            return Ok(lastPagamento);

        }


        /// <summary>
        /// Edits a pagamento from the DB
        /// Only Admin can access this route
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        [HttpPut("edit"), Auth(Roles.Admin)]
        public IActionResult Edit(Pagamento obj)
        {
            if (ModelState.IsValid)
            {
                var updateObj = _db.Pagamentos.Find(obj.Id);

                if (updateObj != null)
                {
                    //Assigns variables to the updateObj
                    if (obj.Metodo != null) updateObj.Metodo = obj.Metodo;
                    if (obj.DataEmissao != null) updateObj.DataEmissao = obj.DataEmissao;
                    if (obj.DataPagamento != null) updateObj.DataPagamento = obj.DataPagamento;
                    if (obj.Montante != null) updateObj.Montante = obj.Montante;
                    if (obj.ApoliceId != null) updateObj.ApoliceId = obj.ApoliceId;

                    //Updates Seguros with the data given
                    _db.Pagamentos.Update(updateObj);
                    _db.SaveChanges();

                    var json = JsonConvert.SerializeObject(updateObj, new JsonSerializerSettings() { MaxDepth = 1, ReferenceLoopHandling = ReferenceLoopHandling.Ignore });

                    var token = Request.Headers.Authorization[0].Replace("Bearer ", "");
                    _logger.SetLogInfoPut(_app.GetUserId(token), "Pagamento", json);

                    return Ok(updateObj);
                }
                else return NotFound(obj);

            }
            return View(obj);
        }

        /// <summary>
        /// Deletes a pagamento from the DB
        /// Only Admin can access this route
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        [HttpDelete("delete"), Auth(Roles.Admin)]
        public IActionResult Delete(int Id)
        {
            var obj = _db.Pagamentos.Find(Id);
            if (obj == null)
            {
                return NotFound();
            }
            _db.Pagamentos.Remove(obj);
            _db.SaveChanges();

            var token = Request.Headers.Authorization[0].Replace("Bearer ", "");
            _logger.SetLogInfoDelete(_app.GetUserId(token), "Pagamento", Id);

            return Ok();
        }

    }
}
