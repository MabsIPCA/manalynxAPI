using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ManaLynxAPI.Models
{
    public class CoberturaHasApolice
    {
        [Key]
        public int Id { get; set; }
        
        //Foreign Keys
        public int? ApoliceId { get; set; }
        public Apolice? Apolice { get; set; }

        public int? CoberturaId { get; set; }
        public Cobertura? Cobertura { get; set; }
    }
}
