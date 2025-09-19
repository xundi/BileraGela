// File: ViewModels/BookingFormViewModel.cs
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Reservas.Models; // <- para AmbitoActividad (vive en Reservas.Models)

namespace Reservas.Models.ViewModels
{
    public class BookingFormViewModel
    {
        public int BookingId { get; set; }
        public int ResourceId { get; set; }

        [Required(ErrorMessage = "El DNI es obligatorio")]
        [RegularExpression(@"^([XYZxyz]-?\d{7}-?[A-Za-z]|[0-9]{8}-?[A-Za-z])$",
            ErrorMessage = "DNI/NIE no válido")]
        [Display(Name = "DNI")]
        public string DNI { get; set; }

        [Required(ErrorMessage = "El nombre y apellidos son obligatorios")]
        [StringLength(120)]
        [Display(Name = "Nombre y Apellidos")]
        public string NombreApellidos { get; set; }

        [Required(ErrorMessage = "El teléfono es obligatorio")]
        [Phone(ErrorMessage = "Teléfono no válido")]
        [Display(Name = "Número de teléfono")]
        public string Telefono { get; set; }

        [Required(ErrorMessage = "El correo es obligatorio")]
        [EmailAddress(ErrorMessage = "Correo no válido")]
        [Display(Name = "Correo electrónico")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Indica el número de asistentes")]
        [Range(1, 1000, ErrorMessage = "Debe ser entre 1 y 1000")]
        [Display(Name = "Número de asistentes")]
        public int NumeroAsistentes { get; set; }

        [Required(ErrorMessage = "El nombre del servicio es obligatorio")]
        [StringLength(120)]
        [Display(Name = "Nombre del servicio")]
        public string NombreServicio { get; set; }

        [Required(ErrorMessage = "Describe la actividad")]
        [StringLength(1000)]
        [Display(Name = "Descripción de la actividad a realizar ")]
        public string DescripcionActividad { get; set; }

        [Required(ErrorMessage = "Selecciona el ámbito")]
        [Display(Name = "Ámbito de la actividad")]
        public AmbitoActividad? Ambito { get; set; }

        [Display(Name = "Observaciones")]
        [StringLength(1000)]
        public string Observaciones { get; set; }

        [Display(Name = "Equipos a utilizar")]
        public List<string> EquiposUtilizar { get; set; } = new();

        [Display(Name = "Fecha Inicio")] public DateTime Start { get; set; }
        [Display(Name = "Fecha Fin")] public DateTime End { get; set; }
    }
}
