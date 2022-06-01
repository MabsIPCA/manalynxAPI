using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ManaLynxAPI.Models
{
    public class ApoliceSaude
    {
        public ApoliceSaude()
        {
            Transacaos = new HashSet<Transacao>();
        }

        [Key]
        public int Id { get; set; }

        //Foreign Keys
        public int? ApoliceId { get; set; }
        public Apolice? Apolice { get; set; }

        public int? ClienteId { get; set; }
        public Cliente? Cliente { get; set; }

        //One to many Relations
        public ICollection<Transacao> Transacaos { get; set; }
    }
}
