using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ManaLynxAPI.Models
{
    public enum Tensao
    {
        Hipotenso,
        Normal,
        Hipertenso
    }
    /// <summary>
    /// This is the model for the DadosClinicos table from the database
    /// </summary>
    public class DadoClinico
    {
        
        public DadoClinico()
        {
            Clientes = new HashSet<Cliente>();
            DadosClinicoHasDoencas = new HashSet<DadosClinicoHasDoenca>();
            Tratamentos = new HashSet<Tratamento>();
        }

        [Key]
        public int Id { get; set; } = 0;
        public double? Altura { get; set; } = null;
        public double? Peso { get; set; } = null;
        public string? Tensao { get; set; } = Models.Tensao.Normal.ToString();

        //One to many Relations 
        public ICollection<Cliente> Clientes { get; set; }
        public ICollection<DadosClinicoHasDoenca> DadosClinicoHasDoencas { get; set; }
        public ICollection<Tratamento> Tratamentos { get; set; }

    }
}
