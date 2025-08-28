using System;
using System.Collections.Generic;              // Necesario para IEnumerable<>
using System.ComponentModel.DataAnnotations;

namespace Reservas.Models
{
    public class Booking : IValidatableObject     // Implementa validación
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

        public string Estado { get; set; } = string.Empty;
        public string Sala { get; set; } = string.Empty;
        public string Usuario { get; set; } = string.Empty;

        // 🔎 Validación de negocio: FechaFin >= FechaInicio
        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (FechaFin < FechaInicio)
            {
                yield return new ValidationResult(
                    "La fecha fin debe ser igual o posterior a la fecha de inicio.",
                    new[] { nameof(FechaFin) }   // Marca el campo FechaFin en la vista
                );
            }
        }
    }
}


