using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ManaLynxAPI.Models
{
    public enum Fracionamento
    {
        Mensal,
        Trimestral,
        Semestral,
        Anual,
    }

    public enum SimulacaoState
    {
        NaoValidada,
        Validada,
        Aprovada,
        PagamentoEmitido,
        Cancelada
    }

    public class Apolice
    {
        public Apolice()
        {
            ApolicePessoals = new HashSet<ApolicePessoal>();
            ApoliceSaudes = new HashSet<ApoliceSaude>();
            ApoliceVeiculos = new HashSet<ApoliceVeiculo>();
            CoberturaHasApolices = new HashSet<CoberturaHasApolice>();
            Pagamentos = new HashSet<Pagamento>();
        }
        /// <summary>
        /// This is the model for the Apolice table from the database
        /// </summary>
        [Key]
        public int Id { get; set; }
        public bool Ativa { get; set; }
        public double? Premio { get; set; }
        public DateTime? Validade { get; set; }
        public string? Fracionamento { get; set; }
        public string? Simulacao { get; set; }

        //Foreign Keys
        public int? AgenteId { get; set; }
        public Agente? Agente { get; set; }

        public int? SeguroId { get; set; }
        public Seguro? Seguro { get; set; }

        //One to many Relations
        public ICollection<ApolicePessoal> ApolicePessoals { get; set; }
        public ICollection<ApoliceSaude> ApoliceSaudes { get; set; }
        public ICollection<ApoliceVeiculo> ApoliceVeiculos { get; set; }
        public ICollection<CoberturaHasApolice> CoberturaHasApolices { get; set; }
        public ICollection<Pagamento> Pagamentos { get; set; }


    }
}
