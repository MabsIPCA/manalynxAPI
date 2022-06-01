using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ManaLynxAPI.Models
{
    public class DadosClinicoHasDoenca
    {
        [Key]
        public int Id { get; set; }

        //Foreign Keys
        public int? DadoClinicoId { get; set; }
        public DadoClinico? DadoClinico { get; set; }

        public int? DoencaId { get; set; }
        public Doenca? Doenca { get; set; }
    }
}
