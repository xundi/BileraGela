using System.ComponentModel.DataAnnotations;

namespace Reservas.Models
{
    public class User
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "El DNI es obligatorio.")]
        [RegularExpression(@"^[0-9]{8}[A-Za-z]$", ErrorMessage = "Formato de DNI no válido.")]
        public string Dni { get; set;  }

        [Required(ErrorMessage = "El tipo de usuario es obligatorio.")]
        public int UserTypeId { get; set; }

        public UserType? UserType { get; set; }  // <- Recomendación: Nullable si puede venir nulo
    }
}
