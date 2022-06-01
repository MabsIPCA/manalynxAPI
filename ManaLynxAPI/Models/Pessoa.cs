using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ManaLynxAPI.Models
{
    /// <summary>
    /// Marital Status Enum
    /// </summary>
    public enum EstadoCivil
    {
        /// <summary>
        /// Single
        /// </summary>
        Solteiro,
        /// <summary>
        /// Married
        /// </summary>
        Casado,
        /// <summary>
        /// Non-Marital Partnership
        /// </summary>
        UniaoDeFacto,
        /// <summary>
        /// Divorced
        /// </summary>
        Divorciado,
        /// <summary>
        /// Widow
        /// </summary>
        Viúvo,
    }
    /// <summary>
    /// Represents Pessoa Instance
    /// </summary>
    public class Pessoa
    {
        /// <summary>
        /// Default Constructor
        /// </summary>
        public Pessoa()
        {
            Agentes = new HashSet<Agente>();
            Clientes = new HashSet<Cliente>();
            Contactos = new HashSet<Contacto>();
            ManaUsers = new HashSet<ManaUser>();
        }

        #region Properties
        /// <summary>
        /// Primary Key
        /// </summary>
        [Key]
        public int Id { get; set; } = 0;
        /// <summary>
        /// Full Name
        /// </summary>
        public string? Nome { get; set; }
        /// <summary>
        /// Birth of Date
        /// </summary>
        public DateTime? DataNascimento { get; set; }
        /// <summary>
        /// Nationality
        /// </summary>
        public string? Nacionalidade { get; set; }
        /// <summary>
        /// Citizen Card Number
        /// </summary>
        public string? Cc { get; set; }
        /// <summary>
        /// Citizen Card Expiral Date
        /// </summary>
        public DateTime? ValidadeCc { get; set; }
        /// <summary>
        /// Fiscal Identification Number
        /// </summary>
        public string? Nif { get; set; }
        /// <summary>
        /// Social Security Number
        /// </summary>
        public string? Nss { get; set; }
        /// <summary>
        /// Individual Healthcare Number
        /// </summary>
        public string? Nus { get; set; }
        /// <summary>
        /// EstadoCivil Enum Values
        /// </summary>
        public string? EstadoCivil { get; set; }

        /// <summary>
        /// Collection of related Agentes
        /// Should be only one
        /// </summary>
        public ICollection<Agente> Agentes { get; set; }
        /// <summary>
        /// Collection of related Clientes
        /// Should be only one
        /// </summary>
        public ICollection<Cliente> Clientes { get; set; }
        /// <summary>
        /// Collection of related Contactos
        /// </summary>
        public ICollection<Contacto> Contactos { get; set; }
        /// <summary>
        /// Collection of related ManaUsers 
        /// Should be one or none
        /// </summary>
        public ICollection<ManaUser> ManaUsers { get; set; }
        #endregion

    }
}
