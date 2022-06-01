using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ManaLynxAPI.Models
{
    public class Gestor
    {
        public Gestor()
        {
            Equipas = new HashSet<Equipa>();
        }

        [Key]
        public int Id { get; set; }

        //Foreign Keys
        public int? AgenteId { get; set; }
        public Agente? Agente { get; set; }

        //One to Many Relations
        public ICollection<Equipa> Equipas { get; set; }
    }
}
