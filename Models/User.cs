using System.ComponentModel.DataAnnotations;

namespace Reservas.Models
{
    public class User
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "El DNI es obligatorio.")]
        [RegularExpression(@"^[0-9]{8}[A-Za-z]$", ErrorMessage = "Formato de DNI no válido.")]
        public string Dni { get; set; } = string.Empty;

        [Required(ErrorMessage = "El tipo de usuario es obligatorio.")]
        public int UserTypeId { get; set; }
        public UserType? UserType { get; set; }

        // 👇 NUEVO
        [Required, EmailAddress]
        [MaxLength(255)]
        public string Email { get; set; } = string.Empty;

        public ICollection<ResourceValidator> RecursosQueValida { get; set; } = new List<ResourceValidator>();
    }
}
