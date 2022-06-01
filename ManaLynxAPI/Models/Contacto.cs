using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ManaLynxAPI.Models
{
    /// <summary>
    /// Enumerable to Define Contacto.Tipo with method ToString()
    /// </summary>
    public enum TipoContacto
    {
        /// <summary>
        /// Represents Email address
        /// abcdef@ghijk.lmn
        /// </summary>
        Email,
        /// <summary>
        /// Represents Telemovel Number
        /// xxx-xxx-xxxxx
        /// </summary>
        Telemovel,
        /// <summary>
        /// Represents a Telefone Number
        /// xxx-xxx-xxxxx
        /// </summary>
        Telefone,
        /// <summary>
        /// Represents a Morada (home address)
        /// Street, floor, door, ZIP-code
        /// </summary>
        Morada,
    }
    /// <summary>
    /// Represents Contacto Instance
    /// </summary>
    public class Contacto
    {
        /// <summary>
        /// Primary Key
        /// </summary>
        [Key]
        public int Id { get; set; }
        /// <summary>
        /// Valor of Contacto        /// </summary>
        public string Valor { get; set; } = null!;
        /// <summary>
        /// Tipo de Contacto
        /// </summary>
        public string Tipo { get; set; } = null!;
        /// <summary>
        /// Pessoa Foreign Key
        /// </summary>
        public int? PessoaId { get; set; }
        /// <summary>
        /// Pessoa Instance
        /// </summary>
        public Pessoa? Pessoa { get; set; }
    }
}
