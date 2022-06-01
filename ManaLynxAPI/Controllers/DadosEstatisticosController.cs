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
using System.Diagnostics;

namespace ManaLynxAPI.Controllers
{
    [Authorize]
    [ApiController, Route("[controller]/[action]")]
    public class DadosEstatisticosController : Controller
    {

        private readonly ApplicationDbContext _db;
        private readonly IAppUtils _app;

        public DadosEstatisticosController(ApplicationDbContext db, IAppUtils app)
        {
            _db = db;
            _app = app;
        }


        #region DashboardAdmin

        /// <summary>
        /// Request to get all the data necessary 
        /// to draw the Admin dashboard to frontend
        /// </summary>
        /// <returns></returns>
        [HttpGet, Auth(Roles.Admin)]
        public IActionResult DashboardAdmin()
        {

            var dashboardObj = new DashboardAdminObj();

            //Gets the amount earned by the All in pagamentos
            var ganhoAmount = (from pagamento in _db.Pagamentos
                               join apolice in _db.Apolices on pagamento.ApoliceId equals apolice.Id
                               join agente in _db.Agentes on apolice.AgenteId equals agente.Id
                               select pagamento.Montante).ToList().Sum();
            dashboardObj.GanhoTotal = Math.Round(ganhoAmount);

            //Gets the amount of apolices total
            var apolicetotal = (from apolice in _db.Apolices
                                 where apolice.Ativa == true
                                 join agente in _db.Agentes on apolice.AgenteId equals agente.Id
                                 select apolice).ToList().Count();

            dashboardObj.ApoliceTotal = apolicetotal;


            //Gets the amount of Leads
            var leadsAmount = (from cliente in _db.Clientes
                               where cliente.IsLead == 1
                               select cliente).ToList().Count();
            dashboardObj.Leads = leadsAmount;


            //Gets the amount of Clientes
            var clientesamount = (from cliente in _db.Clientes
                                  join agente in _db.Agentes on cliente.AgenteId equals agente.Id
                                  select cliente).ToList().Count();
            dashboardObj.Clientes = clientesamount;


            //Gets the total amount earned by year 
            var ganhoAno = (from pagamento in _db.Pagamentos
                                  join apolice in _db.Apolices on pagamento.ApoliceId equals apolice.Id
                                  join agente in _db.Agentes on apolice.AgenteId equals agente.Id
                                  select new
                                  {
                                      pagamento.Montante,
                                      pagamento.DataEmissao
                                  }).ToList();

            var ganhoOrdered = ganhoAno.GroupBy(i => i.DataEmissao).Select(i => new { Data = i.Key, Montante = i.Sum(item => item.Montante) });

            //Add ganhosAno to the list of ganhoByAno
            dashboardObj.GanhoTempo = new List<GanhoTempoSend>();
            foreach (var ganho in ganhoOrdered)
            {
                var ganhoTempoSend = new GanhoTempoSend();
                ganhoTempoSend.data = DateTime.Parse(ganho.Data.Year + "-" + ganho.Data.Month + "-01");
                ganhoTempoSend.montante = ganho.Montante;
                dashboardObj.GanhoTempo.Add(ganhoTempoSend);
            }


            //Gets the amount of clientes by agente
            var agenteCliente = (from agente in _db.Agentes
                                 join cliente in _db.Clientes on agente.Id equals cliente.AgenteId
                                 select new
                                 {
                                     id = agente.Id,
                                     nome = agente.Pessoa.Nome,
                                     cliente = cliente.Id,
                                 }).ToList();

            var agenteOrdered = agenteCliente.GroupBy(i=> i.nome).Select(i => new {nome = i.Key, qtd = i.Count(item => item.cliente > 0)});


            dashboardObj.ClientesAgente = new List<ClientesAgenteSend>();
            foreach (var agente in agenteOrdered)
            {
                var agenteSend = new ClientesAgenteSend();
                agenteSend.nomeAgente = agente.nome;
                agenteSend.amountCliente = agente.qtd;
                dashboardObj.ClientesAgente.Add(agenteSend);

            }

            //Gets the list of Clientes
            var clientesList = (from cliente in _db.Clientes
                                where cliente.IsLead == 0
                                  join pessoa in _db.Pessoas on cliente.PessoaId equals pessoa.Id
                                  select new
                                  {
                                      clienteId = cliente.Id,
                                      apoliceNum = cliente.ApolicePessoals.Count()
                                                    + cliente.ApoliceSaudes.Count()
                                                    + (from veiculo in _db.Veiculos
                                                       where veiculo.ClienteId == cliente.Id
                                                       select veiculo.ApoliceVeiculos).Count(),
                                      profissao = cliente.Profissao,
                                      nome = pessoa.Nome

                                  }).ToList();

            //Adds the clientes to a cliente list
            dashboardObj.ClientesList = new List<ClienteSend>();
            foreach (var cliente in clientesList)
            {
                var clienteSend = new ClienteSend();
                clienteSend.ID = cliente.clienteId;
                clienteSend.Nome = cliente.nome;
                clienteSend.NApolices = cliente.apoliceNum;
                clienteSend.Profissao = cliente.profissao;
                dashboardObj.ClientesList.Add(clienteSend);
            }


            //Gets the list of Agentes 
            var agenteList = (from agente in _db.Agentes
                              join pessoa in _db.Pessoas on agente.PessoaId equals pessoa.Id
                              select new
                              {
                                  agenteId = agente.Id,
                                  agenteNum = agente.Nagente,
                                  pessoaNome = pessoa.Nome
                              }).ToList();

            //Adds the agentes to an agente list
            dashboardObj.AgenteList = new List<AgenteSend>();
            foreach (var agente in agenteList)
            {
                var agenteSend = new AgenteSend();
                agenteSend.ID = agente.agenteId;
                agenteSend.NAgente = agente.agenteNum;
                agenteSend.Nome = agente.pessoaNome;
                dashboardObj.AgenteList.Add(agenteSend);
            }

            //Gets amount of agentes
            dashboardObj.ElementosEquipa = agenteList.Count();


            return Ok(dashboardObj);
        }

#endregion

        #region DashboardGestor
        /// <summary>
        /// Request to get all the data necessary 
        /// to draw the Gestor dashboard to frontend
        /// </summary>
        /// <returns></returns>
        [HttpGet, Auth(Roles.Gestor)]
        public IActionResult DashboardGestor()
        {

            //Get the token of the person accessing route
            var token = Request.Headers.Authorization[0].Replace("Bearer ", "");
            var tokenId = _app.GetUserId(token);
            if (tokenId == null) BadRequest("Invalid Token");
            var id = _app.GetAgenteId(tokenId);
            if (id == null) return BadRequest("Invalid Token");
            var equipaId = _app.GetEquipaId(tokenId);
            

            var dashboardObj = new DashboardGestorObj();

            //Gets the amount earned by the Equipa that Gestor is part of
            var ganhoAmount = (from pagamento in _db.Pagamentos
                               join apolice in _db.Apolices on pagamento.ApoliceId equals apolice.Id
                               join agente in _db.Agentes on apolice.AgenteId equals agente.Id
                               where agente.EquipaId == equipaId
                               select pagamento.Montante).ToList().Sum();
            dashboardObj.GanhoEquipa = Math.Round(ganhoAmount);

            //Gets the amount of apolices by equipa
            var apoliceEquipa = (from apolice in _db.Apolices
                                 where apolice.Ativa == true
                                 join agente in _db.Agentes on apolice.AgenteId equals agente.Id
                                 where agente.EquipaId == equipaId
                                 select apolice).ToList().Count();

            dashboardObj.ApolicesEquipa = apoliceEquipa;


            //Gets the amount of Leads the Gestor has
            var leadsAmount = (from cliente in _db.Clientes
                              where cliente.IsLead == 1
                              where cliente.AgenteId == id
                              select cliente).ToList().Count();
            dashboardObj.Leads = leadsAmount;


            //Gets the amount of Clientes associated with Equipa
            var clientesEquipa = (from cliente in _db.Clientes
                                  join agente in _db.Agentes on cliente.AgenteId equals agente.Id
                                  where agente.EquipaId == equipaId
                                  select cliente).ToList().Count();
            dashboardObj.ClientesEquipa = clientesEquipa;



            //Gets the ganho by ano for the equipa that gestor is part of
            var ganhoAnoEquipa = (from pagamento in _db.Pagamentos
                                  join apolice in _db.Apolices on pagamento.ApoliceId equals apolice.Id
                                  join agente in _db.Agentes on apolice.AgenteId equals agente.Id
                                  where agente.EquipaId == equipaId
                              select new
                              {
                                  pagamento.Montante,
                                  pagamento.DataEmissao
                              }).ToList();

            var ganhoOrdered = ganhoAnoEquipa.GroupBy(i => i.DataEmissao).Select(i => new { Data = i.Key, Montante = i.Sum(item => item.Montante) });

            //Add ganhosAnoEquipa to the list of ganhoByAno
            dashboardObj.GanhoTempo = new List<GanhoTempoSend>();
            foreach(var ganho in ganhoOrdered)
            {
                var ganhoTempoSend = new GanhoTempoSend();
                ganhoTempoSend.data = DateTime.Parse(ganho.Data.Year + "-" + ganho.Data.Month + "-01"); ;
                ganhoTempoSend.montante = ganho.Montante;
                dashboardObj.GanhoTempo.Add(ganhoTempoSend);
            }


            //Gets the amount of clientes by agente
            var agenteCliente = (from agente in _db.Agentes
                                 join cliente in _db.Clientes on agente.Id equals cliente.AgenteId
                                 where agente.EquipaId == equipaId
                                 select new
                                 {
                                     id = agente.Id,
                                     nome = agente.Pessoa.Nome,
                                     cliente = cliente.Id,
                                 }).ToList();

            var agenteOrdered = agenteCliente.GroupBy(i => i.nome).Select(i => new { nome = i.Key, qtd = i.Count(item => item.cliente > 0) });


            dashboardObj.ClientesAgente = new List<ClientesAgenteSend>();
            foreach (var agente in agenteOrdered)
            {
                var agenteSend = new ClientesAgenteSend();
                agenteSend.nomeAgente = agente.nome;
                agenteSend.amountCliente = agente.qtd;
                dashboardObj.ClientesAgente.Add(agenteSend);

            }




            //Gets the list of Clientes that are from the Gestor
            var clientesGestor = (from cliente in _db.Clientes
                                  join pessoa in _db.Pessoas on cliente.PessoaId equals pessoa.Id
                                  where cliente.IsLead == 0
                                  where cliente.AgenteId == id
                                  select new
                                  {
                                      clienteId = cliente.Id,
                                      apoliceNum = cliente.ApolicePessoals.Count() 
                                                    + cliente.ApoliceSaudes.Count() 
                                                    + (from veiculo in _db.Veiculos
                                                      where veiculo.ClienteId == cliente.Id
                                                      select veiculo.ApoliceVeiculos).Count(),
                                     profissao = cliente.Profissao,
                                     nome = pessoa.Nome

                                  }).ToList();

            //Adds the clientes to a cliente list
            dashboardObj.ClientesList = new List<ClienteSend>();
            foreach (var cliente in clientesGestor)
            {
                var clienteSend = new ClienteSend();
                clienteSend.ID = cliente.clienteId;
                clienteSend.Nome = cliente.nome;
                clienteSend.NApolices = cliente.apoliceNum;
                clienteSend.Profissao = cliente.profissao;
                dashboardObj.ClientesList.Add(clienteSend);
            }


            //Gets the list of Agentes that are from the same Equipa as the Gestor Accessing route
            var agenteList = (from agente in _db.Agentes
                              join pessoa in _db.Pessoas on agente.PessoaId equals pessoa.Id
                              where agente.EquipaId == equipaId
                              select new
                              {
                                  agenteId = agente.Id,
                                  agenteNum = agente.Nagente,
                                  pessoaNome = pessoa.Nome
                              }).ToList();

            //Adds the agentes to an agente list
            dashboardObj.AgenteList = new List<AgenteSend>();
            foreach(var agente in agenteList)
            {
                var agenteSend = new AgenteSend();
                agenteSend.ID = agente.agenteId;
                agenteSend.NAgente = agente.agenteNum;
                agenteSend.Nome = agente.pessoaNome;
                dashboardObj.AgenteList.Add(agenteSend);
            }

            //Gets the amount of agentes for the Gestor Equipa
            dashboardObj.elementosEquipa = agenteList.Count();

            //Gets info on equipa for gestor
            var equipaInfo = (from agente in _db.Agentes
                              where agente.Id == id
                              join equipa in _db.Equipas on agente.EquipaId equals equipa.Id
                              select new
                              {
                                  nome = equipa.Nome,
                                  regiao = equipa.Regiao
                              }).ToList();

            dashboardObj.regiaoEquipa = equipaInfo[0].regiao;
            dashboardObj.nomeEquipa = equipaInfo[0].nome;



            return Ok(dashboardObj);
        }

        #endregion

        #region DashboardAgente


        /// <summary>
        /// Request to get all data necessary to draw
        /// Agente dashboard in frontend
        /// </summary>
        /// <returns></returns>
        [HttpGet, Auth(Roles.Agente)]
        public IActionResult DashboardAgente()
        {
            //Get the token of the person accessing route
            var token = Request.Headers.Authorization[0].Replace("Bearer ", "");
            var tokenId = _app.GetUserId(token);
            if (tokenId == null) BadRequest("Invalid Token");
            var id = _app.GetAgenteId(tokenId);
            if (id == null) return BadRequest("Invalid Token");
            var equipaId = _app.GetEquipaId(tokenId);

            var dashboardObj = new DashboardAgenteObj();

            //Gets the amount earned by the Agente
            var ganhoAmount = (from pagamento in _db.Pagamentos
                               join apolice in _db.Apolices on pagamento.ApoliceId equals apolice.Id
                               where apolice.AgenteId == id
                               select pagamento.Montante).ToList().Sum();

            dashboardObj.GanhoPessoal = Math.Round(ganhoAmount);

            //Gets the apolices for this agente
            var apolices = (from apolice in _db.Apolices
                            where apolice.AgenteId == id
                            select apolice).ToList().Count();

            dashboardObj.ApolicesEncargo = apolices;


            //Gets the leads for this agente
            var leads = (from cliente in _db.Clientes
                         where cliente.IsLead == 1
                         where cliente.AgenteId == id
                         select cliente).ToList().Count();

            dashboardObj.Leads = leads;

            //Gets the clientes for this agente
            var clientes = (from cliente in _db.Clientes
                            where cliente.AgenteId == id
                            select cliente).ToList().Count();

            dashboardObj.ClientesEncargo = clientes;


            //Gets ganho by ano for the agente
            var ganhoAno = (from pagamento in _db.Pagamentos
                                  join apolice in _db.Apolices on pagamento.ApoliceId equals apolice.Id
                                  where apolice.AgenteId == id
                                  select new
                                  {
                                      pagamento.Montante,
                                      pagamento.DataEmissao
                                  }).ToList();

            var ganhoOrdered = ganhoAno.GroupBy(i => i.DataEmissao).Select(i => new { Data = i.Key, Montante = i.Sum(item => item.Montante) });

            //Add ganhosAnoEquipa to the list of ganhoByAno
            dashboardObj.GanhoTempo = new List<GanhoTempoSend>();
            foreach (var ganho in ganhoOrdered)
            {
                var ganhoTempoSend = new GanhoTempoSend();
                ganhoTempoSend.data = DateTime.Parse(ganho.Data.Year+"-"+ganho.Data.Month+"-01");
                ganhoTempoSend.montante = ganho.Montante;
                dashboardObj.GanhoTempo.Add(ganhoTempoSend);
            }

            //Gets the amount of clientes by agente
            var agenteCliente = (from agente in _db.Agentes
                                 join cliente in _db.Clientes on agente.Id equals cliente.AgenteId
                                 where agente.EquipaId == equipaId
                                 select new
                                 {
                                     id = agente.Id,
                                     nome = agente.Pessoa.Nome,
                                     cliente = cliente.Id,
                                 }).ToList();

            var agenteOrdered = agenteCliente.GroupBy(i => i.nome).Select(i => new { nome = i.Key, qtd = i.Count(item => item.cliente > 0) });


            dashboardObj.ClientesAgente = new List<ClientesAgenteSend>();
            foreach (var agente in agenteOrdered)
            {
                var agenteSend = new ClientesAgenteSend();
                agenteSend.nomeAgente = agente.nome;
                agenteSend.amountCliente = agente.qtd;
                dashboardObj.ClientesAgente.Add(agenteSend);

            }




            //Gets the list of Clientes that are from the Agente
            var clientesGestor = (from cliente in _db.Clientes
                                  join pessoa in _db.Pessoas on cliente.PessoaId equals pessoa.Id
                                  where cliente.IsLead == 0
                                  where cliente.AgenteId == id
                                  select new
                                  {
                                      clienteId = cliente.Id,
                                      apoliceNum = cliente.ApolicePessoals.Count()
                                                    + cliente.ApoliceSaudes.Count()
                                                    + (from veiculo in _db.Veiculos
                                                       where veiculo.ClienteId == cliente.Id
                                                       select veiculo.ApoliceVeiculos).Count(),
                                      profissao = cliente.Profissao,
                                      nome = pessoa.Nome

                                  }).ToList();

            //Adds the clientes to a cliente list
            dashboardObj.ClientesList = new List<ClienteSend>();
            foreach (var cliente in clientesGestor)
            {
                var clienteSend = new ClienteSend();
                clienteSend.ID = cliente.clienteId;
                clienteSend.Nome = cliente.nome;
                clienteSend.NApolices = cliente.apoliceNum;
                clienteSend.Profissao = cliente.profissao;
                dashboardObj.ClientesList.Add(clienteSend);
            }

            //Gets the amount of agentes for the Gestor Equipa
            var agenteList = (from agente in _db.Agentes
                              where agente.EquipaId == equipaId
                              select agente).ToList();

            dashboardObj.elementosEquipa = agenteList.Count();

            //Gets info on equipa for gestor
            var equipaInfo = (from agente in _db.Agentes
                              where agente.Id == id
                              join equipa in _db.Equipas on agente.EquipaId equals equipa.Id
                              select new
                              {
                                  nome = equipa.Nome,
                                  regiao = equipa.Regiao
                              }).ToList();

            dashboardObj.regiaoEquipa = equipaInfo[0].regiao;
            dashboardObj.nomeEquipa = equipaInfo[0].nome;



            return Ok(dashboardObj);
        }


        #endregion

        #region DashboardCliente

        /// <summary>
        /// Request to get all data necessary to draw
        /// cliente dashboard in frontend
        /// </summary>
        /// <returns></returns>
        [HttpGet, Auth(Roles.Cliente)]
        public IActionResult DashboardCliente()
        {
            //Get the token of the person accessing route
            var token = Request.Headers.Authorization[0].Replace("Bearer ", "");
            var tokenId = _app.GetUserId(token);
            if (tokenId == null) BadRequest("Invalid Token");
            var id = _app.GetClienteId(tokenId);
            if (id == null) return BadRequest("Invalid Token");

            var dashboardObj = new DashboardClienteObj();

            //Gets the amount in pagamentos for cliente
            var gastoP = (from cliente in _db.Clientes
                          where cliente.Id == id
                          join apoliceP in _db.ApolicePessoals on cliente.Id equals apoliceP.ClienteId
                         join apolice in _db.Apolices on apoliceP.ApoliceId equals apolice.Id
                         join pagamento in _db.Pagamentos on apolice.Id equals pagamento.ApoliceId
                         select new
                         {
                             pagamento.Montante,
                             pagamento.DataPagamento,
                             pagamento.DataEmissao
                         }).ToList();

            var gastoS = (from cliente in _db.Clientes
                          where cliente.Id == id
                          join apoliceS in _db.ApoliceSaudes on cliente.Id equals apoliceS.ClienteId
                          join apolice in _db.Apolices on apoliceS.ApoliceId equals apolice.Id
                          join pagamento in _db.Pagamentos on apolice.Id equals pagamento.ApoliceId
                          select new
                          {
                              pagamento.Montante,
                              pagamento.DataPagamento,
                              pagamento.DataEmissao
                          }).ToList();

            var gastoV = (from cliente in _db.Clientes
                          where cliente.Id == id
                          join veiculo in _db.Veiculos on cliente.Id equals veiculo.ClienteId
                          join apoliceV in _db.ApoliceVeiculos on veiculo.Id equals apoliceV.VeiculoId
                          join apolice in _db.Apolices on apoliceV.ApoliceId equals apolice.Id
                          join pagamento in _db.Pagamentos on apolice.Id equals pagamento.ApoliceId
                          select new
                          {
                              pagamento.Montante,
                              pagamento.DataPagamento,
                              pagamento.DataEmissao
                          }).ToList();

            dashboardObj.GastoPessoal = Math.Round(gastoP.Sum(x=> x.Montante) + gastoS.Sum(x => x.Montante) + gastoV.Sum(x => x.Montante));


            //Gets the amount of apolices per Cliente
            var apolicesP = (from cliente in _db.Clientes
                          where cliente.Id == id
                          join apoliceP in _db.ApolicePessoals on cliente.Id equals apoliceP.ClienteId
                          join apolice in _db.Apolices on apoliceP.ApoliceId equals apolice.Id
                          where apolice.Ativa == true
                          select apoliceP).ToList().Count();


            var apolicesS = (from cliente in _db.Clientes
                             where cliente.Id == id
                             join apoliceS in _db.ApoliceSaudes on cliente.Id equals apoliceS.ClienteId
                             join apolice in _db.Apolices on apoliceS.ApoliceId equals apolice.Id
                             where apolice.Ativa == true
                             select apoliceS).ToList().Count();

            var apolicesV = (from cliente in _db.Clientes
                            where cliente.Id == id
                            join veiculo in _db.Veiculos on cliente.Id equals veiculo.ClienteId
                            join apoliceV in _db.ApoliceVeiculos on veiculo.Id equals apoliceV.VeiculoId
                             join apolice in _db.Apolices on apoliceV.ApoliceId equals apolice.Id
                             where apolice.Ativa == true
                             select apoliceV).ToList().Count();


            dashboardObj.QtdApolices = (apolicesP + apolicesS + apolicesV);

            //Gets the amount of pagamentos em falta per Cliente
            foreach(var s in gastoS)
            {
                if (s.DataPagamento == DateTime.Parse("1900-01-01")) dashboardObj.QtdPagamentosPagar += 1;
            }
            foreach (var p in gastoP)
            {
                if (p.DataPagamento == DateTime.Parse("1900-01-01")) dashboardObj.QtdPagamentosPagar += 1;
            }
            foreach (var v in gastoV)
            {
                if (v.DataPagamento == DateTime.Parse("1900-01-01")) dashboardObj.QtdPagamentosPagar += 1;
            }



            //Gets the amount of veiculos this Cliente has
            var veiculos = (from cliente in _db.Clientes
                            where cliente.Id == id
                            join veiculo in _db.Veiculos on cliente.Id equals veiculo.ClienteId
                            select veiculo).ToList().Count();


            dashboardObj.QtdVeiculos = veiculos;


            //Gets the amount spent by Cliente per Time
            var gastoOrderedS = gastoS.GroupBy(i => i.DataEmissao).Select(i => new { Data = i.Key, Montante = i.Sum(item => item.Montante) });
            var gastoOrderedP = gastoP.GroupBy(i => i.DataEmissao).Select(i => new { Data = i.Key, Montante = i.Sum(item => item.Montante) });
            var gastoOrderedV = gastoV.GroupBy(i => i.DataEmissao).Select(i => new { Data = i.Key, Montante = i.Sum(item => item.Montante) });


            var gastoOrdered = gastoOrderedS.Concat(gastoOrderedP)
                                            .Concat(gastoOrderedV)
                                            .ToList()
                                            .GroupBy(i => i.Data).Select(i => new { Data = i.Key, Montante = i.Sum(item => item.Montante) });


            //Add ganhostempo to the list of ganhotempo
            dashboardObj.GastoTempo = new List<GanhoTempoSend>();
            foreach (var ganho in gastoOrdered)
            {
                var gastoTempoSend = new GanhoTempoSend();
                gastoTempoSend.data = DateTime.Parse(ganho.Data.Year + "-" + ganho.Data.Month + "-01");
                gastoTempoSend.montante = ganho.Montante;
                dashboardObj.GastoTempo.Add(gastoTempoSend);
            }




            //Gets the amount of apolice per Type
            dashboardObj.ApoliceTipo = new List<ApoliceTipoSend>
            {
                new ApoliceTipoSend(){tipo="Pessoal", amount=apolicesP},
                new ApoliceTipoSend(){tipo="Saúde", amount=apolicesS},
                new ApoliceTipoSend(){tipo="Veiculos", amount=apolicesV}
            };

            //Adds useful info for cliente
            var info = (from cliente in _db.Clientes
                        join pessoa in _db.Pessoas on cliente.PessoaId equals pessoa.Id
                        where cliente.Id == id
                        select new
                        {
                            nif = pessoa.Nif,
                            nss = pessoa.Nss,
                            nus = pessoa.Nus
                        }).ToList();

            dashboardObj.nif = info[0].nif;
            dashboardObj.nss = info[0].nss;
            dashboardObj.nus = info[0].nus;


            //Gets the list of apolices for this cliente
            var apolicesListP = (from cliente in _db.Clientes
                                 where cliente.Id == id
                                 join apoliceP in _db.ApolicePessoals on cliente.Id equals apoliceP.ClienteId
                                 join apolice in _db.Apolices on apoliceP.ApoliceId equals apolice.Id
                                 join seguro in _db.Seguros on apolice.SeguroId equals seguro.Id
                                 select new
                                 {
                                     nome = seguro.Nome,
                                     tipo = "Pessoal",
                                     estado = apolice.Ativa,
                                     premio = apolice.Premio,
                                     fracionamento = apolice.Fracionamento,
                                     validade = apolice.Validade
                                 }).ToList();

            var apolicesListS = (from cliente in _db.Clientes
                                 where cliente.Id == id
                                 join apoliceS in _db.ApoliceSaudes on cliente.Id equals apoliceS.ClienteId
                                 join apolice in _db.Apolices on apoliceS.ApoliceId equals apolice.Id
                                 join seguro in _db.Seguros on apolice.SeguroId equals seguro.Id
                                 select new
                                 {
                                     nome = seguro.Nome,
                                     tipo = "Saúde",
                                     estado = apolice.Ativa,
                                     premio = apolice.Premio,
                                     fracionamento = apolice.Fracionamento,
                                     validade = apolice.Validade
                                 }).ToList();

            var apolicesListV = (from cliente in _db.Clientes
                                where cliente.Id == id
                                join veiculo in _db.Veiculos on cliente.Id equals veiculo.ClienteId
                                join apoliceV in _db.ApoliceVeiculos on veiculo.Id equals apoliceV.VeiculoId
                                join apolice in _db.Apolices on apoliceV.ApoliceId equals apolice.Id
                                join seguro in _db.Seguros on apolice.SeguroId equals seguro.Id
                                select new
                                {
                                    nome = seguro.Nome,
                                    tipo = "Veículo",
                                    estado = apolice.Ativa,
                                    premio = apolice.Premio,
                                    fracionamento = apolice.Fracionamento,
                                    validade = apolice.Validade
                                }).ToList();


            var apoliceList = apolicesListP.Concat(apolicesListS)
                                            .Concat(apolicesListV)
                                            .ToList();

            //Adds the clientes to a cliente list
            dashboardObj.ApoliceList = new List<ApoliceSend>();
            foreach (var apolice in apoliceList)
            {
                var apoliceSend = new ApoliceSend();
                apoliceSend.nome = apolice.nome;
                apoliceSend.tipo = apolice.tipo;
                if (apolice.estado) apoliceSend.estado = "Ativa";
                else apoliceSend.estado = "Inativa";
                apoliceSend.premio = apolice.premio;
                apoliceSend.fracionamento = apolice.fracionamento;
                apoliceSend.validade = apolice.validade;
                dashboardObj.ApoliceList.Add(apoliceSend);
            }


            return Ok(dashboardObj);
        }


        #endregion

    }

    #region Useful Classes
    public class AgenteSend
    {
        public int ID { get; set; }
        public int NAgente { get; set; }
        public string Nome { get; set; }
    }

    public class ClienteSend
    {
        public int ID { get; set; }
        public string Nome { get; set; }    
        public string Profissao { get; set; }
        public int NApolices { get; set; }
    }

    public class GanhoTempoSend
    {
        public DateTime data { get; set; }
        public double montante { get; set; }
    }

    public class ClientesAgenteSend
    {
        public string nomeAgente { get; set; } 
        public int amountCliente { get; set; }

    }

    public class ApoliceSend
    {
        public string nome { get; set; }
        public string tipo { get; set; }
        public string estado { get; set; }
        public double? premio { get; set; }
        public string fracionamento { get; set; }
        public DateTime? validade { get; set; }
    }

    public class ApoliceTipoSend
    {
        public string tipo { get; set; }
        public int amount { get; set; }
    }


    public class DashboardAdminObj
    {
        public double GanhoTotal { get; set; }
        public int ElementosEquipa { get; set; }
        public int ApoliceTotal { get; set; }
        public int Leads { get; set; }
        public int Clientes { get; set; }
        public List<GanhoTempoSend> GanhoTempo { get; set; }
        public List<ClientesAgenteSend> ClientesAgente { get; set; }
        public List<ClienteSend> ClientesList { get; set; }
        public List<AgenteSend> AgenteList { get; set; }

    }

    public class DashboardGestorObj
    {
        public double GanhoEquipa { get; set; }
        public string nomeEquipa { get; set; }
        public string regiaoEquipa { get; set; }
        public int elementosEquipa { get; set; }
        public int ApolicesEquipa { get; set; }
        public int Leads { get; set; }
        public int ClientesEquipa { get; set; }
        public List<GanhoTempoSend> GanhoTempo {get; set;}
        public List<ClientesAgenteSend> ClientesAgente { get; set;}
        public List<ClienteSend> ClientesList { get; set; }
        public List<AgenteSend> AgenteList { get; set; }

    }

    public class DashboardAgenteObj
    {
        public double GanhoPessoal { get; set; }
        public string nomeEquipa { get; set; }
        public string regiaoEquipa { get; set; }
        public int elementosEquipa { get; set; }
        public int ApolicesEncargo { get; set; }
        public int Leads { get; set; }
        public int ClientesEncargo { get; set; }
        public List<GanhoTempoSend> GanhoTempo { get; set;}
        public List<ClientesAgenteSend> ClientesAgente { get; set; }
        public List<ClienteSend> ClientesList { get; set; }

    }

    public class DashboardClienteObj
    {
        public double GastoPessoal { get; set; }
        public string nif { get; set; }
        public string nss { get; set; }
        public string nus { get; set; }

        public int QtdApolices { get; set; }

        public int QtdPagamentosPagar { get; set; }

        public int QtdVeiculos { get; set; }

        public List<GanhoTempoSend> GastoTempo { get; set; }

        public List<ApoliceTipoSend> ApoliceTipo { get; set; }
        public List<ApoliceSend> ApoliceList { get; set; }

    }
    #endregion
}
