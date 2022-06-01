using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ManaLynxAPI.Models
{
    public class SinistroPessoal
    {
        [Key]
        public int Id { get; set; }

        //Foreign Keys
        public int? ApolicePessoalId { get; set; }
        public ApolicePessoal? ApolicePessoal { get; set; }
        
        public int? SinistroId { get; set; }
        public Sinistro? Sinistro { get; set; }


    }
}
