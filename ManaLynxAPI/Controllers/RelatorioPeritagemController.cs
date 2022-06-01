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

    [Authorize]
    [ApiController, Route("[controller]")]
    public class RelatorioPeritagemController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly IAppUtils _appUtils;
        private readonly SinistroUtils _sinistroUtils;
        private readonly ILoggerUtils _logger;
        private readonly IRelatorioPeritagemUtils _rpUtils;

        public RelatorioPeritagemController(ApplicationDbContext db, IAppUtils app, ILoggerUtils logger, IRelatorioPeritagemUtils relatorio)
        {
            _db = db;
            _appUtils = app;
            _sinistroUtils = new SinistroUtils(db);
            _logger = logger;
            _rpUtils = relatorio;
        }

        /// <summary>
        /// RelatorioPeritagem index Route
        /// This route can only be accessed by authenticad users, everyone can access it.
        /// </summary>
        /// <returns>RelatorioPeritagem List, possibly empty</returns>
        [HttpGet, Auth]
        public IActionResult Index()
        {
            var token = Request.Headers.Authorization[0].Replace("Bearer ", "");
            var jwtSecurityToken = new JwtSecurityTokenHandler().ReadJwtToken(Request.Headers.Authorization[0].Replace("Bearer ", ""));
            var idToken = jwtSecurityToken.Claims.First(claim => claim.Type == "Id").Value;
            var roleToken = jwtSecurityToken.Claims.First(claim => claim.Type == "role").Value;
            int id;
            bool success = Int32.TryParse(idToken, out id);
            if (!success)
            {
                return NotFound("Token Format invalid");
            }
            switch (roleToken)
            {
                case "Admin":
                    if (_db.RelatorioPeritagems != null)
                    {
                        var objRelat = (from relat in _db.RelatorioPeritagems
                                        select new
                                        {
                                            relat.Id,
                                            relat.Conteudo,
                                            relat.DataRelatorio,
                                            relat.Deferido,
                                            relat.Sinistro
                                        }).ToList();

                        _logger.SetLogInfoGetAll(_appUtils.GetUserId(token), "RelatorioPeritagem");
                        return Ok(objRelat);
                    }
                    else return NotFound();
                case "Gestor":
                    var equipaId = (from equipa in _db.Equipas
                                    join agente in _db.Agentes on equipa.Id equals agente.EquipaId
                                    join pessoa in _db.Pessoas on agente.PessoaId equals pessoa.Id
                                    join manauser in _db.ManaUsers on pessoa.Id equals manauser.PessoaId
                                    where manauser.Id == id
                                    select equipa.Id).ToList().FirstOrDefault();
                    if (equipaId == 0)
                    {
                        return NotFound();
                    }
                    var objRelatPessoalGestor = (from relat in _db.RelatorioPeritagems
                                                 join sinistro in _db.Sinistros on relat.SinistroId equals sinistro.Id
                                                 join sPessoal in _db.SinistroPessoals on sinistro.Id equals sPessoal.SinistroId
                                                 join apPessoal in _db.ApolicePessoals on sPessoal.ApolicePessoalId equals apPessoal.Id
                                                 join apolice in _db.Apolices on apPessoal.ApoliceId equals apolice.Id
                                                 join agente in _db.Agentes on apolice.AgenteId equals agente.Id
                                                 where agente.EquipaId == equipaId
                                                 select new
                                                 {
                                                     relat.Id,
                                                     relat.Conteudo,
                                                     relat.DataRelatorio,
                                                     relat.Deferido,
                                                     relat.Sinistro
                                                 }).ToList();
                    var objRelatVeiculoGestor = (from relat in _db.RelatorioPeritagems
                                                 join sinistro in _db.Sinistros on relat.SinistroId equals sinistro.Id
                                                 join sVeiculo in _db.SinistroVeiculos on sinistro.Id equals sVeiculo.SinistroId
                                                 join apVeiculo in _db.ApoliceVeiculos on sVeiculo.ApoliceVeiculoId equals apVeiculo.Id
                                                 join apolice in _db.Apolices on apVeiculo.ApoliceId equals apolice.Id
                                                 join agente in _db.Agentes on apolice.AgenteId equals agente.Id
                                                 where agente.EquipaId == equipaId
                                                 select new
                                                 {
                                                     relat.Id,
                                                     relat.Conteudo,
                                                     relat.DataRelatorio,
                                                     relat.Deferido,
                                                     relat.Sinistro
                                                 }).ToList();
                    foreach (var sin in objRelatVeiculoGestor)
                    {
                        objRelatPessoalGestor.Add(sin);
                    }

                    _logger.SetLogInfoGetAll(_appUtils.GetUserId(token), "RelatorioPeritagem");
                    return Ok(objRelatPessoalGestor);
                case "Agente":
                    var agenteId = (from agente in _db.Agentes
                                    join pessoa in _db.Pessoas on agente.PessoaId equals pessoa.Id
                                    join manauser in _db.ManaUsers on pessoa.Id equals manauser.PessoaId
                                    where manauser.Id == id
                                    select agente.Id).ToList().FirstOrDefault();
                    if (agenteId == 0)
                    {
                        return NotFound();
                    }
                    var objRelatPessoalAgente = (from relat in _db.RelatorioPeritagems
                                                 join sinistro in _db.Sinistros on relat.SinistroId equals sinistro.Id
                                                 join sPessoal in _db.SinistroPessoals on sinistro.Id equals sPessoal.SinistroId
                                                 join apPessoal in _db.ApolicePessoals on sPessoal.ApolicePessoalId equals apPessoal.Id
                                                 join apolice in _db.Apolices on apPessoal.ApoliceId equals apolice.Id
                                                 join agente in _db.Agentes on apolice.AgenteId equals agente.Id
                                                 where agente.Id == agenteId
                                                 select new
                                                 {
                                                     relat.Id,
                                                     relat.Conteudo,
                                                     relat.DataRelatorio,
                                                     relat.Deferido,
                                                     relat.Sinistro
                                                 }).ToList();
                    var objRelatVeiculoAgente = (from relat in _db.RelatorioPeritagems
                                                 join sinistro in _db.Sinistros on relat.SinistroId equals sinistro.Id
                                                 join sVeiculo in _db.SinistroVeiculos on sinistro.Id equals sVeiculo.SinistroId
                                                 join apVeiculo in _db.ApoliceVeiculos on sVeiculo.ApoliceVeiculoId equals apVeiculo.Id
                                                 join apolice in _db.Apolices on apVeiculo.ApoliceId equals apolice.Id
                                                 join agente in _db.Agentes on apolice.AgenteId equals agente.Id
                                                 where agente.Id == agenteId
                                                 select new
                                                 {
                                                     relat.Id,
                                                     relat.Conteudo,
                                                     relat.DataRelatorio,
                                                     relat.Deferido,
                                                     relat.Sinistro
                                                 }).ToList();
                    foreach (var sin in objRelatVeiculoAgente)
                    {
                        objRelatPessoalAgente.Add(sin);
                    }

                    _logger.SetLogInfoGetAll(_appUtils.GetUserId(token), "RelatorioPeritagem");
                    return Ok(objRelatPessoalAgente);
                case "Cliente":
                    var clienteId = (from cliente in _db.Clientes
                                     join pessoa in _db.Pessoas on cliente.PessoaId equals pessoa.Id
                                     join manauser in _db.ManaUsers on pessoa.Id equals manauser.PessoaId
                                     where manauser.Id == id
                                     select cliente.Id).ToList().FirstOrDefault();
                    if (clienteId == 0)
                    {
                        return NotFound();
                    }
                    var objRelatPessoalCliente = (from relat in _db.RelatorioPeritagems
                                                 join sinistro in _db.Sinistros on relat.SinistroId equals sinistro.Id
                                                 join sPessoal in _db.SinistroPessoals on sinistro.Id equals sPessoal.SinistroId
                                                 join apPessoal in _db.ApolicePessoals on sPessoal.ApolicePessoalId equals apPessoal.Id
                                                 join apolice in _db.Apolices on apPessoal.ApoliceId equals apolice.Id
                                                 where apPessoal.ClienteId == clienteId
                                                 select new
                                                 {
                                                     relat.Id,
                                                     relat.Conteudo,
                                                     relat.DataRelatorio,
                                                     relat.Deferido,
                                                     relat.Sinistro
                                                 }).ToList();
                    var objRelatVeiculoCliente = (from relat in _db.RelatorioPeritagems
                                                 join sinistro in _db.Sinistros on relat.SinistroId equals sinistro.Id
                                                 join sVeiculo in _db.SinistroVeiculos on sinistro.Id equals sVeiculo.SinistroId
                                                 join apVeiculo in _db.ApoliceVeiculos on sVeiculo.ApoliceVeiculoId equals apVeiculo.Id
                                                 join veiculo in _db.Veiculos on apVeiculo.VeiculoId equals veiculo.Id
                                                 where veiculo.ClienteId == clienteId
                                                  select new
                                                 {
                                                     relat.Id,
                                                     relat.Conteudo,
                                                     relat.DataRelatorio,
                                                     relat.Deferido,
                                                     relat.Sinistro
                                                 }).ToList();


                    _logger.SetLogInfoGetAll(_appUtils.GetUserId(token), "RelatorioPeritagem");
                    return Ok(objRelatPessoalCliente);
                default: return NotFound();

            }
            return BadRequest();
        }

        /// <summary>
        /// RelatorioPeritagem ViewById Route
        /// This route can only be accessed by authenticad users, everyone can access it.
        /// </summary>
        /// <param name="Id">id of RelatorioPeritagem</param>
        /// <returns>RelatorioPeritagem List, one or zero</returns>
        [HttpGet("{Id}"), Auth]
        public IActionResult ViewById(int? Id)
        {
            if (Id == null || Id == 0)
            {
                return NotFound();
            }

            var token = Request.Headers.Authorization[0].Replace("Bearer ", "");
            var jwtSecurityToken = new JwtSecurityTokenHandler().ReadJwtToken(Request.Headers.Authorization[0].Replace("Bearer ", ""));
            var idToken = jwtSecurityToken.Claims.First(claim => claim.Type == "Id").Value;
            var roleToken = jwtSecurityToken.Claims.First(claim => claim.Type == "role").Value;
            int id;
            bool success = Int32.TryParse(idToken, out id);
            if (!success)
            {
                return NotFound("Token Format invalid");
            }
            switch (roleToken)
            {
                case "Admin":
                    if (_db.RelatorioPeritagems != null)
                    {
                        var objRelat = (from relat in _db.RelatorioPeritagems
                                        where relat.Id == Id
                                        select new
                                        {
                                            relat.Id,
                                            relat.Conteudo,
                                            relat.DataRelatorio,
                                            relat.Deferido,
                                            relat.Sinistro
                                        }).ToList();

                        _logger.SetLogInfoGet(_appUtils.GetUserId(token), "RelatorioPeritagem", Id);
                        return Ok(objRelat);
                    }
                    else return NotFound();
                case "Gestor":
                    var equipaId = (from equipa in _db.Equipas
                                    join agente in _db.Agentes on equipa.Id equals agente.EquipaId
                                    join pessoa in _db.Pessoas on agente.PessoaId equals pessoa.Id
                                    join manauser in _db.ManaUsers on pessoa.Id equals manauser.PessoaId
                                    where manauser.Id == id
                                    select equipa.Id).ToList().FirstOrDefault();
                    if (equipaId == 0)
                    {
                        return NotFound();
                    }
                    var objRelatPessoalGestor = (from relat in _db.RelatorioPeritagems
                                                 join sinistro in _db.Sinistros on relat.SinistroId equals sinistro.Id
                                                 join sPessoal in _db.SinistroPessoals on sinistro.Id equals sPessoal.SinistroId
                                                 join apPessoal in _db.ApolicePessoals on sPessoal.ApolicePessoalId equals apPessoal.Id
                                                 join apolice in _db.Apolices on apPessoal.ApoliceId equals apolice.Id
                                                 join agente in _db.Agentes on apolice.AgenteId equals agente.Id
                                                 where agente.EquipaId == equipaId && relat.Id == Id
                                                 select new
                                                 {
                                                     relat.Id,
                                                     relat.Conteudo,
                                                     relat.DataRelatorio,
                                                     relat.Deferido,
                                                     relat.Sinistro
                                                 }).ToList();
                    var objRelatVeiculoGestor = (from relat in _db.RelatorioPeritagems
                                                 join sinistro in _db.Sinistros on relat.SinistroId equals sinistro.Id
                                                 join sVeiculo in _db.SinistroVeiculos on sinistro.Id equals sVeiculo.SinistroId
                                                 join apVeiculo in _db.ApoliceVeiculos on sVeiculo.ApoliceVeiculoId equals apVeiculo.Id
                                                 join apolice in _db.Apolices on apVeiculo.ApoliceId equals apolice.Id
                                                 join agente in _db.Agentes on apolice.AgenteId equals agente.Id
                                                 where agente.EquipaId == equipaId && relat.Id == Id
                                                 select new
                                                 {
                                                     relat.Id,
                                                     relat.Conteudo,
                                                     relat.DataRelatorio,
                                                     relat.Deferido,
                                                     relat.Sinistro
                                                 }).ToList();
                    foreach (var sin in objRelatVeiculoGestor)
                    {
                        objRelatPessoalGestor.Add(sin);
                    }

                    _logger.SetLogInfoGet(_appUtils.GetUserId(token), "RelatorioPeritagem", Id);
                    return Ok(objRelatPessoalGestor);
                case "Agente":
                    var agenteId = (from agente in _db.Agentes
                                    join pessoa in _db.Pessoas on agente.PessoaId equals pessoa.Id
                                    join manauser in _db.ManaUsers on pessoa.Id equals manauser.PessoaId
                                    where manauser.Id == id
                                    select agente.Id).ToList().FirstOrDefault();
                    if (agenteId == 0)
                    {
                        return NotFound();
                    }
                    var objRelatPessoalAgente = (from relat in _db.RelatorioPeritagems
                                                 join sinistro in _db.Sinistros on relat.SinistroId equals sinistro.Id
                                                 join sPessoal in _db.SinistroPessoals on sinistro.Id equals sPessoal.SinistroId
                                                 join apPessoal in _db.ApolicePessoals on sPessoal.ApolicePessoalId equals apPessoal.Id
                                                 join apolice in _db.Apolices on apPessoal.ApoliceId equals apolice.Id
                                                 join agente in _db.Agentes on apolice.AgenteId equals agente.Id
                                                 where agente.Id == agenteId && relat.Id == Id
                                                 select new
                                                 {
                                                     relat.Id,
                                                     relat.Conteudo,
                                                     relat.DataRelatorio,
                                                     relat.Deferido,
                                                     relat.Sinistro
                                                 }).ToList();
                    var objRelatVeiculoAgente = (from relat in _db.RelatorioPeritagems
                                                 join sinistro in _db.Sinistros on relat.SinistroId equals sinistro.Id
                                                 join sVeiculo in _db.SinistroVeiculos on sinistro.Id equals sVeiculo.SinistroId
                                                 join apVeiculo in _db.ApoliceVeiculos on sVeiculo.ApoliceVeiculoId equals apVeiculo.Id
                                                 join apolice in _db.Apolices on apVeiculo.ApoliceId equals apolice.Id
                                                 join agente in _db.Agentes on apolice.AgenteId equals agente.Id
                                                 where agente.Id == agenteId && relat.Id == Id
                                                 select new
                                                 {
                                                     relat.Id,
                                                     relat.Conteudo,
                                                     relat.DataRelatorio,
                                                     relat.Deferido,
                                                     relat.Sinistro
                                                 }).ToList();
                    foreach (var sin in objRelatVeiculoAgente)
                    {
                        objRelatPessoalAgente.Add(sin);
                    }

                    _logger.SetLogInfoGet(_appUtils.GetUserId(token), "RelatorioPeritagem", Id);
                    return Ok(objRelatPessoalAgente);
                case "Cliente":
                    var clienteId = (from cliente in _db.Clientes
                                     join pessoa in _db.Pessoas on cliente.PessoaId equals pessoa.Id
                                     join manauser in _db.ManaUsers on pessoa.Id equals manauser.PessoaId
                                     where manauser.Id == id
                                     select cliente.Id).ToList().FirstOrDefault();
                    if (clienteId == 0)
                    {
                        return NotFound();
                    }
                    var objRelatPessoalCliente = (from relat in _db.RelatorioPeritagems
                                                  join sinistro in _db.Sinistros on relat.SinistroId equals sinistro.Id
                                                  join sPessoal in _db.SinistroPessoals on sinistro.Id equals sPessoal.SinistroId
                                                  join apPessoal in _db.ApolicePessoals on sPessoal.ApolicePessoalId equals apPessoal.Id
                                                  join apolice in _db.Apolices on apPessoal.ApoliceId equals apolice.Id
                                                  where apPessoal.ClienteId == clienteId && relat.Id == Id
                                                  select new
                                                  {
                                                      relat.Id,
                                                      relat.Conteudo,
                                                      relat.DataRelatorio,
                                                      relat.Deferido,
                                                      relat.Sinistro
                                                  }).ToList();
                    var objRelatVeiculoCliente = (from relat in _db.RelatorioPeritagems
                                                  join sinistro in _db.Sinistros on relat.SinistroId equals sinistro.Id
                                                  join sVeiculo in _db.SinistroVeiculos on sinistro.Id equals sVeiculo.SinistroId
                                                  join apVeiculo in _db.ApoliceVeiculos on sVeiculo.ApoliceVeiculoId equals apVeiculo.Id
                                                  join veiculo in _db.Veiculos on apVeiculo.VeiculoId equals veiculo.Id
                                                  where veiculo.ClienteId == clienteId && relat.Id == Id
                                                  select new
                                                  {
                                                      relat.Id,
                                                      relat.Conteudo,
                                                      relat.DataRelatorio,
                                                      relat.Deferido,
                                                      relat.Sinistro
                                                  }).ToList();

                    _logger.SetLogInfoGet(_appUtils.GetUserId(token), "RelatorioPeritagem", Id);
                    return Ok(objRelatPessoalCliente);
                default: return NotFound();

            }
            return BadRequest();
        }

        /// <summary>
        /// RelatorioPeritagem create Route
        /// This route can only be accessed by authenticad users, only Admin can access it.
        /// Admins can post RelatorioPeritagem.
        /// </summary>
        /// <param name="obj">RelatorioPeritagem object</param>
        /// <returns>Send RelatorioPeritagem object.</returns>
        [HttpPost, Auth(Roles.Admin)]
        public IActionResult Create(RelatorioPeritagem obj)
        {
            if (ModelState.IsValid)
            {
                //Initializes response variables
                var objectUtils = new RelatorioPeritagem();
                var responseUtils = string.Empty;

                //Calls function from utils
                (objectUtils, responseUtils) = _rpUtils.CreateRelatorio(obj);
                if (objectUtils == null) return BadRequest(error: responseUtils);

                var json = JsonConvert.SerializeObject(objectUtils, new JsonSerializerSettings() { MaxDepth = 1, ReferenceLoopHandling = ReferenceLoopHandling.Ignore });

                var token = Request.Headers.Authorization[0].Replace("Bearer ", "");
                _logger.SetLogInfoPost(_appUtils.GetUserId(token), "RelatorioPeritagem", json);

                return Ok(objectUtils);
            }
            return BadRequest(obj);
        }

        /// <summary>
        /// RelatorioPeritagem delete Route
        /// This route can only be accessed by authenticad users, only Admin can access it.
        /// Admins can delete Doenca.
        /// </summary>
        /// <param name="obj">RelatorioPeritagem object</param>
        /// <returns>Updated RelatorioPeritagem if update is successful, if not, return the sent object.</returns>
        [HttpDelete("{Id}"), Auth(Roles.Admin)]
        public IActionResult Delete(int Id)
        {
            var obj = _db.RelatorioPeritagems.Find(Id);
            if (obj == null)
            {
                return NotFound();
            }
            _db.RelatorioPeritagems.Remove(obj);
            _db.SaveChanges();

            var token = Request.Headers.Authorization[0].Replace("Bearer ", "");
            _logger.SetLogInfoDelete(_appUtils.GetUserId(token), "RelatorioPeritagem", Id);

            return Ok(obj);
        }
    }
}
