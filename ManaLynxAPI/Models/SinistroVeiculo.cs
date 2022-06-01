using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ManaLynxAPI.Models
{
    public class SinistroVeiculo
    {
        [Key]
        public int Id { get; set; }
        
        //Foreign Key
        public int? ApoliceVeiculoId { get; set; }
        public ApoliceVeiculo? ApoliceVeiculo { get; set; }

        public int? SinistroId { get; set; }
        public Sinistro? Sinistro { get; set; }
    }
}
