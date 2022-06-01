using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ManaLynxAPI.Models
{
    public class Transacao
    {
        [Key]
        public int Id { get; set; }
        public string Descricao { get; set; } = null!;
        public DateTime DataTransacao { get; set; }
        public double Montante { get; set; }

        //Foreign Keys
        public int? ApoliceSaudeId { get; set; }
        public ApoliceSaude? ApoliceSaude { get; set; }

    }
}
