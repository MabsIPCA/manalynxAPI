using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ManaLynxAPI.Models
{   
    /// <summary>
    /// This is the model for the Doenca table from the database
    /// </summary>
    public class Doenca
    {
        
        public Doenca()
        {
            DadosClinicoHasDoencas = new HashSet<DadosClinicoHasDoenca>();
        }
        [Key]
        public int Id { get; set; }
        public string NomeDoenca { get; set; }
        public string? Descricao { get; set; }

        //One to many Relations
        public ICollection<DadosClinicoHasDoenca> DadosClinicoHasDoencas { get; set; }
    }
}
