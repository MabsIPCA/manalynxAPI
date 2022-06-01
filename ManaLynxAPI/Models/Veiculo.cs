using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ManaLynxAPI.Models
{
    /// <summary>
    /// Represents Veiculo Instance
    /// </summary>
    public class Veiculo
    {
        /// <summary>
        /// Default Constructor
        /// </summary>
        public Veiculo()
        {
            ApoliceVeiculos = new HashSet<ApoliceVeiculo>();
        }

        /// <summary>
        /// Primary Key
        /// </summary>
        [Key]
        public int Id { get; set; } = 0;
        /// <summary>
        /// Vehicle Information Number
        /// </summary>
        public string Vin { get; set; } = string.Empty;
        /// <summary>
        /// Vehicle Plate Number
        /// </summary>
        public string Matricula { get; set; } = string.Empty;
        /// <summary>
        /// Vehicle Year of Factory
        /// </summary>
        public int Ano { get; set; } = 0;
        /// <summary>
        /// Vehicle Month of Factory
        /// </summary>
        public int Mes { get; set; } = 0;
        /// <summary>
        /// Vehicle Brand
        /// </summary>
        public string Marca { get; set; } = string.Empty;
        /// <summary>
        /// Vehicle Model
        /// </summary>
        public string Modelo { get; set; } = string.Empty;
        /// <summary>
        /// Vehicle Engine Capacity
        /// </summary>
        public int? Cilindrada { get; set; } = null;
        /// <summary>
        /// Vehicle Doors Number
        /// </summary>
        public int? Portas { get; set; } = null;
        /// <summary>
        /// Vehicle Seats Number
        /// </summary>
        public int? Lugares { get; set; } = null;
        /// <summary>
        /// Vehicle Power
        /// </summary>
        public int? Potencia { get; set; } = null;
        /// <summary>
        /// Vehicle Weight
        /// </summary>
        public int? Peso { get; set; } = null;

        //Foreign Keys
        /// <summary>
        /// Cliente Foreign Key
        /// </summary>
        public int? ClienteId { get; set; } = null;
        /// <summary>
        /// Cliente Instance
        /// </summary>
        public Cliente? Cliente { get; set; } = null;
        /// <summary>
        /// CategoriaVeiculo Foreign Key
        /// </summary>
        public int? CategoriaVeiculoId { get; set; } = null;
        /// <summary>
        /// CategoriaVeiculo Instance
        /// </summary>
        public CategoriaVeiculo? CategoriaVeiculo { get; set; } = null;


        //One to many Relations
        /// <summary>
        /// Collection of related ApoliceVeiculo
        /// </summary>
        public ICollection<ApoliceVeiculo> ApoliceVeiculos { get; set; }
    }
}
