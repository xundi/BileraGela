using System.ComponentModel.DataAnnotations;

namespace Reservas.Models.ViewModels
{
    public class RecursoOptionVM
    {
        public string Value { get; set; } = "";
        public string Text { get; set; } = "";
        public int CenterId { get; set; }
        public int ResourceTypeId { get; set; }
    }
}
