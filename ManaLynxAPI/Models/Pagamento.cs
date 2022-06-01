using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ManaLynxAPI.Models
{
    /// <summary>
    /// This is the model for the Pagamento table from the database
    /// </summary>
    public class Pagamento
    {
        
        [Key]
        public int Id { get; set; }
        public string Metodo { get; set; } = null!;
        public DateTime DataEmissao { get; set; }
        public DateTime DataPagamento { get; set; }
        public double Montante { get; set; }

        //Foreign Keys
        public int? ApoliceId { get; set; }
        public Apolice? Apolice { get; set; }

    }
}
