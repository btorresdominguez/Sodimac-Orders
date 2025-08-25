using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace SodimacOrders.Domain.Entities.SodimacOrders.Domain.Entities
{
    [Table("Usuarios")]
    public class User
    {
        [Key]
        public int IdUsuario { get; set; }

        [Required]
        [StringLength(150)]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        [StringLength(500)]
        public string Contraseña { get; set; } = string.Empty;
        public bool Estado { get; set; } = true;

        // Navigation properties
        public virtual ICollection<UserTokens> UsuarioTokens { get; set; } = new List<UserTokens>();
        public virtual ICollection<UserRol> UsuarioRoles { get; set; } = new List<UserRol>();

        // Computed property - not mapped to database
        [NotMapped]
        public List<string> Role { get; set; } = new List<string>();
    }
}