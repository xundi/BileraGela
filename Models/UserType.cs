using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Reservas.Models
{
    public class UserType
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "El nombre del tipo de usuario es obligatorio.")]
        [StringLength(50, ErrorMessage = "El nombre debe tener máximo 50 caracteres.")]
        public required string Name { get; set; } // Ej: "Administrador", "Profesor"    

        // Relación con usuarios    
        public ICollection<User> Users { get; set; } = new List<User>();
    }
}
