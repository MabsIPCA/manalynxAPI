using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace ManaLynxAPI.Models
{
    public class Sinistro
    {
        public Sinistro()
        {
            Provas = new HashSet<Prova>();
            RelatorioPeritagems = new HashSet<RelatorioPeritagem>();
            SinistroPessoals = new HashSet<SinistroPessoal>();
            SinistroVeiculos = new HashSet<SinistroVeiculo>();
        }

        public int Id { get; set; }
        public string Descricao { get; set; } = null!;
        public string Estado { get; set; } = null!;
        public double? Reembolso { get; set; }
        public DateTime? DataSinistro { get; set; }
        public bool? Valido { get; set; }
        public bool? Deferido { get; set; }

        public ICollection<Prova> Provas { get; set; }
        public ICollection<RelatorioPeritagem> RelatorioPeritagems { get; set; }
        public ICollection<SinistroPessoal> SinistroPessoals { get; set; }
        public ICollection<SinistroVeiculo> SinistroVeiculos { get; set; }
    }
}
