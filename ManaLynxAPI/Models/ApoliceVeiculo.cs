using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ManaLynxAPI.Models
{
    public class ApoliceVeiculo
    {
        public ApoliceVeiculo()
        {
            SinistroVeiculos = new HashSet<SinistroVeiculo>();
        }

        [Key]
        public int Id { get; set; }

        public DateTime? DataCartaConducao { get; set; }
        public int? AcidentesRecentes { get; set; }

        //Foreign Keys
        public int? ApoliceId { get; set; }
        public Apolice? Apolice { get; set; }

        public int? VeiculoId { get; set; }
        public Veiculo? Veiculo { get; set; }

        //One to many Relations
        public ICollection<SinistroVeiculo> SinistroVeiculos { get; set; }
    }
}
