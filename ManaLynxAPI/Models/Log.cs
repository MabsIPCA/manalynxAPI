using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ManaLynxAPI.Models
{
    public class Log
    {
        [Key]
        public int Id { get; set; }
        public byte[]? LogDate { get; set; }
        public string? Username { get; set; }
        public string? Query { get; set; }
    }
}
