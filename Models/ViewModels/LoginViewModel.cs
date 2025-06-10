using System.ComponentModel.DataAnnotations;

namespace Reservas.Models.ViewModels
{
    public class LoginViewModel
    {
        [Required(ErrorMessage = "El DNI es obligatorio")]
        [Display(Name = "DNI")]
        public string Dni { get; set; }
    }
}