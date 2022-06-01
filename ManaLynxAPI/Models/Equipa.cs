using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ManaLynxAPI.Models
{
    public class Equipa
    {
        public Equipa()
        {
            Agentes = new HashSet<Agente>();
        }

        [Key]
        public int Id { get; set; }
        public string Nome { get; set; } = null!;
        public string Regiao { get; set; } = null!;

        //Foreign Keys
        public int? GestorId { get; set; }
        public Gestor? Gestor { get; set; }

        //One to many relations
        public ICollection<Agente> Agentes { get; set; }
    }
}
