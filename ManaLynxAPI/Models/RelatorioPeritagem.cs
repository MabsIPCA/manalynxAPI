using System;
using System.Collections.Generic;

namespace ManaLynxAPI.Models
{
    public partial class RelatorioPeritagem
    {
        public int Id { get; set; }
        public string? Conteudo { get; set; }
        public DateTime DataRelatorio { get; set; }
        public bool Deferido { get; set; }
        public int? SinistroId { get; set; }

        public Sinistro? Sinistro { get; set; }
    }
}
