using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Reservas.Models.ViewModels;

namespace Reservas.Models.ViewModels
{
    public class ReservaViewModel
    {
        public int? CentroId { get; set; }
        public int? TipoId { get; set; }
        public int? RecursoId { get; set; }

        public DateTime FechaInicio { get; set; }
        public DateTime FechaFin { get; set; }
    }
}
