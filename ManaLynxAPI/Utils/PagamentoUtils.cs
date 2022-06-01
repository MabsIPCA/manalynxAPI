using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ManaLynxAPI.Data;
using ManaLynxAPI.Models;


namespace ManaLynxAPI.Utils
{
    public interface IPagamentoUtils
    {
        bool PagamentoExists(Pagamento pagamento);

        Pagamento? AddPagamento(Pagamento pagamento);


    }
    
    public class PagamentoUtils : IPagamentoUtils
    {
        private ApplicationDbContext _db;

        public PagamentoUtils(ApplicationDbContext db)
        {
            _db = db;
        }

        public bool PagamentoExists(Pagamento pagamento)
        {
            return false;
        }

        public Pagamento? AddPagamento(Pagamento pagamento)
        {
            var createObj = new Pagamento();

            if (pagamento != null)
            {
                //Assign varibles to createObj
                createObj.Metodo = pagamento.Metodo;
                createObj.DataEmissao = pagamento.DataEmissao;
                createObj.DataPagamento = pagamento.DataPagamento;
                createObj.Montante = pagamento.Montante;
                createObj.ApoliceId = pagamento.ApoliceId;

                //Save changes to db
                _db.Pagamentos.Add(createObj);
                _db.SaveChanges();
                return createObj;

            }

            return null;
        }

        public static Tuple<Apolice, Pagamento> CreatePagamento(Apolice expired)
        {
            //Create a new Pagamento for expiring Apolices that were paid
            var createPag = new Pagamento();
            createPag.Metodo = "Cartao";
            createPag.DataEmissao = DateTime.Today;
            createPag.DataPagamento = DateTime.Parse("1900-01-01");
            createPag.ApoliceId = expired.Id;
            expired.Ativa = true;

            //Calculate Montante through premio divided by fracionamento
            //Also change expiredApolice Validade field by fracionamento
            switch (expired.Fracionamento)
            {
                case "Mensal":
                    if (expired.Premio != null) createPag.Montante = (double)expired.Premio / 12;
                    expired.Validade = DateTime.Today.AddMonths(1);

                    break;

                case "Trimestral":
                    if (expired.Premio != null) createPag.Montante = (double)expired.Premio / 4;
                    expired.Validade = DateTime.Today.AddMonths(3);

                    break;

                case "Semestral":
                    if (expired.Premio != null) createPag.Montante = (double)expired.Premio / 2;
                    expired.Validade = DateTime.Today.AddMonths(6);

                    break;

                case "Anual":
                    if (expired.Premio != null) createPag.Montante = (double)expired.Premio;
                    expired.Validade = DateTime.Today.AddMonths(12);

                    break;
                default:
                    createPag.Montante = 9999;
                    break;
            }
            return Tuple.Create(expired, createPag);
        }



        public static void DailyPagamentoVerification(ApplicationDbContext db)
        {
            //Get apolices that are about to expire
            var expiringApolices = (from apolice in db.Apolices
                                    where apolice.Simulacao == "Pagamento Emitido"
                                    where apolice.Validade < DateTime.Today
                                    select apolice).ToList();


            foreach(var expired in expiringApolices)
            {
                //Verify if last pagamento was paid
                //Get the last pagamento of a given apolice
                var pagamentos = (from apolice in db.Apolices
                                  join pagamento in db.Pagamentos on apolice.Id equals pagamento.ApoliceId
                                  where apolice.Id == expired.Id
                                  select pagamento);

                //Gets last pagamento for a given apolice
                var lastPagamento = pagamentos.OrderBy(x => x.DataEmissao).Last();

                //Verification of Pagamentos that were not paid
                if (lastPagamento.DataPagamento == DateTime.Parse("1900-01-01"))
                {
                    //Update Apolice to be Ativa = false
                    expired.Ativa = false;
                    expired.Simulacao = "Cancelada";
                    db.Apolices.Update(expired);
                }
                else //If pagamentos were paid Create a new Pagamento
                {
                    //Calls function to return object of pagamento
                    var ( apolice, createPag) = CreatePagamento(expired);

                    //Add a new Pagamento to the database
                    
                    db.Pagamentos.Add(createPag);
                    db.Apolices.Update(apolice);
                }

            }
            //Save changes to database
            db.SaveChanges();
        }

    }
}
