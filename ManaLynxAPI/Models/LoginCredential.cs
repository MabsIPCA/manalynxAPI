using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ManaLynxAPI.Models
{
    /// <summary>
    /// Represents LoginCredential Instance
    /// </summary>
    public class LoginCredential
    {
        /// <summary>
        /// Default Constructor
        /// </summary>
        public LoginCredential()
        {
            ManaUsers = new HashSet<ManaUser>();
        }

        /// <summary>
        /// Primary Key
        /// </summary>
        [Key]
        public int Id { get; set; }
        /// <summary>
        /// Password Hash
        /// </summary>
        public string ManaHash { get; set; } = string.Empty;
        /// <summary>
        /// Password Salt
        /// </summary>
        public string ManaSalt { get; set; } = string.Empty;

        // One to Many Relations
        /// <summary>
        /// Collection of related ManaUsers.
        /// Can never have more than one.
        /// </summary>
        public ICollection<ManaUser> ManaUsers { get; set; }
    }
}
