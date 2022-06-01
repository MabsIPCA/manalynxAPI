using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ManaLynxAPI.Models
{
    /// <summary>
    /// Represents an Instance of a Cliente
    /// </summary>
    public class Cliente
    {
        /// <summary>
        /// Default Constructor
        /// </summary>
        public Cliente()
        {
            ApolicePessoals = new HashSet<ApolicePessoal>();
            ApoliceSaudes = new HashSet<ApoliceSaude>();
            Veiculos = new HashSet<Veiculo>();
        }

        /// <summary>
        /// Primary Key
        /// </summary>
        [Key]
        public int Id { get; set; }
        /// <summary>
        /// Profissao Name
        /// </summary>
        public string? Profissao { get; set; }
        /// <summary>
        /// ProfissaoRisco Flag.
        /// Affects Apolices.Premio.
        /// </summary>
        public bool? ProfissaoRisco { get; set; }
        /// <summary>
        /// IsLead Flag. 
        /// Determines wether it is a user with an account.
        /// Can only be set by Roles: Agente, Gestor, Admin.
        /// </summary>
        public int? IsLead { get; set; }

        //Foreign Keys
        /// <summary>
        /// Agente Foreign Key
        /// </summary>
        public int? AgenteId { get; set; }
        /// <summary>
        /// Agente Instance
        /// </summary>
        public Agente? Agente { get; set; }
        /// <summary>
        /// DadoClinico Foreign Key
        /// </summary>
        public int? DadoClinicoId { get; set; }
        /// <summary>
        /// DadoClinico Instance
        /// </summary>
        public DadoClinico? DadoClinico { get; set; }
        /// <summary>
        /// Pessoa Foreign Key
        /// </summary>
        public int? PessoaId { get; set; }
        /// <summary>
        /// Pessoa Instance
        /// </summary>
        public Pessoa? Pessoa { get; set; }


        //One to Many Relations
        /// <summary>
        /// Collection of related ApolicePessoals.
        /// </summary>
        public ICollection<ApolicePessoal> ApolicePessoals { get; set; }
        /// <summary>
        /// Collection of related ApoliceSaudes.
        /// </summary>
        public ICollection<ApoliceSaude> ApoliceSaudes { get; set; }
        /// <summary>
        /// Collection of related Veiculos.
        /// </summary>
        public ICollection<Veiculo> Veiculos { get; set; }

    }
}
