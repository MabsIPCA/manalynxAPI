using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ManaLynxAPI.Models
{
    /// <summary>
    /// This is the model for the Agente table from the database.
    /// </summary>
    public class Agente
    {
        /// <summary>
        /// Default Constructor;
        /// </summary>
        public Agente()
        {
            Apolices = new HashSet<Apolice>();
            Clientes = new HashSet<Cliente>();
            Gestors = new HashSet<Gestor>();
        }

        #region Properties
        [Key]
        public int Id { get; set; } = 0;
        public int Nagente { get; set; } = 0;


        //Foreign Keys
        [Required]
        public int? EquipaId { get; set; } = null;
        public Equipa? Equipa { get; set; } = null;
        public int? PessoaId { get; set; } = null;
        public Pessoa? Pessoa { get; set; } = null;

        //One to many Relations
        public ICollection<Apolice> Apolices { get; set; }
        public ICollection<Cliente> Clientes { get; set; }
        public ICollection<Gestor> Gestors { get; set; }
        #endregion

    }
}
