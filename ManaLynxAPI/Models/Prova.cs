using System;
using System.Collections.Generic;

namespace ManaLynxAPI.Models
{
    public partial class Prova
    {
        public int Id { get; set; }
        public string? Conteudo { get; set; }
        public DateTime DataSubmissao { get; set; }
        public int? SinistroId { get; set; }

        public Sinistro? Sinistro { get; set; }
    }
}
