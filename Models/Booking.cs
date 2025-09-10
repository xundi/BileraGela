using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Reservas.Models
{
    public enum AmbitoActividad
    {
        [Display(Name = "Reunión de trabajo")] ReunionTrabajo = 1,
        [Display(Name = "Formación continuada")] FormacionContinuada = 2,
        [Display(Name = "Información - Divulgación")] InformacionDivulgacion = 3,
        [Display(Name = "Otros")] Otros = 4
    }

    public class Booking : IValidatableObject
    {
        public int Id { get; set; }

        public int ResourceId { get; set; }
        public Resource Resource { get; set; } = null!;

        public int? UserId { get; set; }
        public User User { get; set; } = null!;

        public DateTime FechaCreacion { get; set; } = DateTime.Now;

        [Display(Name = "Fecha Inicio")]
        [DataType(DataType.DateTime)]
        public DateTime FechaInicio { get; set; }

        [Display(Name = "Fecha Fin")]
        [DataType(DataType.DateTime)]
        public DateTime FechaFin { get; set; }

        public string Estado { get; set; } = "PendienteDatos";
        public string Sala { get; set; } = string.Empty;
        public string Usuario { get; set; } = string.Empty;

        // ====== CAMPOS NUEVOS (coinciden con el ViewModel y el controlador) ======
        [MaxLength(12)]
        public string? DNI { get; set; }

        [MaxLength(120)]
        public string? NombreApellidos { get; set; }

        [Phone, MaxLength(30)]
        public string? Telefono { get; set; }

        [EmailAddress, MaxLength(150)]
        public string? Email { get; set; }

        public int? NumeroAsistentes { get; set; }

        [MaxLength(120)]
        public string? NombreServicio { get; set; }

        [MaxLength(1000)]
        public string? DescripcionActividad { get; set; }

        public AmbitoActividad? Ambito { get; set; }

        [MaxLength(1000)]
        public string? Observaciones { get; set; }

        // Guardaremos los checkboxes como CSV: "Proyector,PC,Altavoces"
        [MaxLength(500)]
        public string? EquiposUtilizar { get; set; }
        // ========================================================================

        // Validación de negocio
        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (FechaFin < FechaInicio)
            {
                yield return new ValidationResult(
                    "La fecha fin debe ser igual o posterior a la fecha de inicio.",
                    new[] { nameof(FechaFin) });
            }

            if (NumeroAsistentes.HasValue && NumeroAsistentes.Value < 1)
            {
                yield return new ValidationResult(
                    "El número de asistentes debe ser mayor que 0.",
                    new[] { nameof(NumeroAsistentes) });
            }
        }
    }
}
