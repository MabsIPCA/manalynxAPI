using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ManaLynxAPI.Models
{
    /// <summary>
    /// This is the model for the Tratamento table from the database
    /// </summary>
    public class Tratamento
    {
        
        [Key]
        public int Id { get; set; }
        public string? NomeTratamento { get; set; }
        public string? Frequencia { get; set; }
        public DateTime? UltimaToma { get; set; }

        //Foreign Keys
        public int? DadoClinicoId { get; set; }
        public DadoClinico? DadoClinico { get; set; }
    }
}
