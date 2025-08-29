using System.ComponentModel.DataAnnotations;

namespace Reservas.Models.ViewModels
{
    public class ReservaViewModel : IValidatableObject
    {
        public int? CentroId { get; set; }
        public int? TipoId { get; set; }
        public int? RecursoId { get; set; }

        // 👇 Nuevos (solo lectura en la vista)
        public string? CentroNombre { get; set; }
        public string? TipoNombre { get; set; }
        public string? RecursoNombre { get; set; }

        [Required, Display(Name = "Fecha inicio")]
        public DateTime FechaInicio { get; set; }

        [Required, Display(Name = "Fecha fin")]
        public DateTime FechaFin { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext _)
        {
            if (FechaFin <= FechaInicio)
                yield return new ValidationResult(
                    "La fecha fin debe ser posterior a la fecha inicio.",
                    new[] { nameof(FechaFin) });
        }
    }
}
