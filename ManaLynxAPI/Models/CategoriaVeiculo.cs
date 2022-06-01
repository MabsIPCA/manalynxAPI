using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ManaLynxAPI.Models
{
    public class CategoriaVeiculo
    {
        public CategoriaVeiculo()
        {
            Veiculos = new HashSet<Veiculo>();
        }

        [Key]
        public int Id { get; set; }
        public string Categoria { get; set; } = null!;

        //One to many Relations
        public ICollection<Veiculo> Veiculos { get; set; }
    }
}
