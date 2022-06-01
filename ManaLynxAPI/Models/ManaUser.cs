using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using ManaLynxAPI.Data;

namespace ManaLynxAPI.Models
{
    /// <summary>
    /// User Application Roles.
    /// </summary>
    [Flags]
    public enum Roles
    {
        /// <summary>
        /// System Administrator
        /// </summary>
        Admin,
        /// <summary>
        /// Team Manager
        /// </summary>
        Gestor,
        /// <summary>
        /// Insurance Agente
        /// </summary>
        Agente,
        /// <summary>
        /// Client
        /// </summary>
        Cliente,
    }
    /// <summary>
    /// Represents ManaUser Instance
    /// </summary>
    public class ManaUser
    {
        /// <summary>
        /// Gets RegisterRequest Values
        /// </summary>
        /// <param name="register">Model Used for the Registry data</param>
        public ManaUser(RegisterRequest register)
        {
            Username = register.Username;
            Email = register.Email;
            UserRole = register.UserRole;
        }
        
        /// <summary>
        /// Gets RegisterRoleRequest Values
        /// </summary>
        /// <param name="register"></param>
        public ManaUser(RegisterRoleRequest register)
        {
            Username = register.Username;
            Email = register.Email;
            UserRole = register.UserRole;
        }

        /// <summary>
        /// Default Constructor
        /// </summary>
        public ManaUser() { }

        #region Properties
        /// <summary>
        /// Primary Key
        /// </summary>
        [Key]
        public int Id { get; set; } = 0;
        /// <summary>
        /// Registered Username
        /// </summary>
        public string Username { get; set; } = string.Empty;
        /// <summary>
        /// Registered Email
        /// </summary>
        public string Email { get; set; } = string.Empty;
        /// <summary>
        /// User Role
        /// </summary>
        public string UserRole { get; set; } = string.Empty;


        // Foreign Keys
        /// <summary>
        /// LoginCredential Foreign Key
        /// </summary>
        public int? LoginCredential { get; set; }
        /// <summary>
        /// LoginCredential Instance
        /// </summary>
        public LoginCredential? LoginCredentialNavigation { get; set; }
        /// <summary>
        /// Pessoa Foreign Key
        /// </summary>
        public int? PessoaId { get; set; } = null;
        /// <summary>
        /// Pessoa Instance
        /// </summary>
        public Pessoa? Pessoa { get; set; } = null;
        #endregion
    }
}
