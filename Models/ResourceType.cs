using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Reservas.Models
{
    public class ResourceType
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "El nombre en euskera es obligatorio.")]
        [StringLength(100, ErrorMessage = "Máximo 100 caracteres.")]
        public string NameEuskera { get; set; }

        [Required(ErrorMessage = "El nombre en castellano es obligatorio.")]
        [StringLength(100, ErrorMessage = "Máximo 100 caracteres.")]
        public string NameSpanish { get; set; }

        // Relación con recursos
        public ICollection<Resource> Resources { get; set; } = new List<Resource>();
    }
}
