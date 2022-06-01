using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ManaLynxAPI.Models
{
    /// <summary>
    /// This enum serves as the definition for the Tipo enum from the database
    /// present in the table Seguro.
    /// </summary>
    public enum Tipo
    {
        Saude,
        Vida,
        Veiculo
    }

    /// <summary>
    /// This is the model for the Seguro table from the database.
    /// </summary>
    public class Seguro
    {

        public Seguro()
        {
            Apolices = new HashSet<Apolice>();
            Coberturas = new HashSet<Cobertura>();
        }

        [Key]
        public int Id { get; set; }
        public string Nome { get; set; } = null!;
        public bool Ativo { get; set; }
        public string Tipo { get; set; } = null!;

        //One to Many Relations
        public ICollection<Apolice> Apolices { get; set; }
        public ICollection<Cobertura> Coberturas { get; set; }
    }
}
