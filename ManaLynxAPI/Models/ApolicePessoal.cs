using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ManaLynxAPI.Models
{
    /// <summary>
    /// Represents an Instance of SeguroPessoal
    /// </summary>
    public class ApolicePessoal
    {
        /// <summary>
        /// Default Constructor
        /// </summary>
        public ApolicePessoal()
        {
            SinistroPessoals = new HashSet<SinistroPessoal>();
        }

        /// <summary>
        /// Instance Primary Key
        /// </summary>
        [Key]
        public int Id { get; set; }

        //Foreign Keys
        /// <summary>
        /// Apolice Foreign Key
        /// </summary>
        public int? ApoliceId { get; set; }
        /// <summary>
        /// Apolice Instance
        /// </summary>
        public Apolice? Apolice { get; set; }

        /// <summary>
        /// Cliente Foreign Key
        /// </summary>
        public int? ClienteId { get; set; }
        /// <summary>
        /// Cliente Instance
        /// </summary>
        public Cliente? Cliente { get; set; }
        /// <summary>
        /// Insurance 'reward' in case of disability/death.
        /// </summary>
        public double? Valor { get; set; }

        //One to many relations
        /// <summary>
        /// Collection of SinistroPessoal that are Related to this specific ApolicePessoal
        /// </summary>
        public ICollection<SinistroPessoal> SinistroPessoals { get; set; }

    }
}
