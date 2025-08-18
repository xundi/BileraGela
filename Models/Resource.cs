using System.ComponentModel.DataAnnotations;

namespace Reservas.Models
{
    public class Resource
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "El nombre en euskera es obligatorio.")]
        public string NameEuskera { get; set; }

        [Required(ErrorMessage = "El nombre en castellano es obligatorio.")]
        public string NameSpanish { get; set; }

        [Required(ErrorMessage = "Debe seleccionar un centro.")]
        [Range(1, int.MaxValue, ErrorMessage = "Debe seleccionar un centro válido.")]
        public int CenterId { get; set; }

        [Required(ErrorMessage = "Debe seleccionar un tipo de recurso.")]
        [Range(1, int.MaxValue, ErrorMessage = "Debe seleccionar un tipo válido.")]
        public int ResourceTypeId { get; set; }


        // Relaciones
        public virtual Center Center { get; set; }
        public virtual ResourceType ResourceType { get; set; }

        public ICollection<ResourceValidator> Validadores { get; set; }

    }
}
