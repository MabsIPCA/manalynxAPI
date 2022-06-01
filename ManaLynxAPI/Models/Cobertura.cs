using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ManaLynxAPI.Models
{
    /// <summary>
    /// This is the model for the Cobertura table from the database
    /// </summary>
    public class Cobertura
    {
        public Cobertura()
        {
            CoberturaHasApolices = new HashSet<CoberturaHasApolice>();
        }
        
        [Key]
        public int Id { get; set; }
        public string DescricaoCobertura { get; set; } = null!;

        //Foreign Keys
        public int? SeguroId { get; set; }
        public Seguro? Seguro { get; set; }

        //One to many Relations
        public ICollection<CoberturaHasApolice> CoberturaHasApolices { get; set; }
    }
}
