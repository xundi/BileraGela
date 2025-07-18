﻿using System;
using System.ComponentModel.DataAnnotations;

namespace Reservas.Models
{
    public class Booking
    {
        public int Id { get; set; }

        public int ResourceId { get; set; }
        public Resource Resource { get; set; } = null!;

        public int? UserId { get; set; }  // 👈 NUEVO
        public User User { get; set; } = null!;  // 👈 NUEVO

        public DateTime FechaCreacion { get; set; } = DateTime.Now;
        [DataType(DataType.DateTime)]
        public DateTime FechaInicio { get; set; }

        [DataType(DataType.DateTime)]
        public DateTime FechaFin { get; set; }


        public string Estado { get; set; } = string.Empty;
        public string Sala { get; set; } = string.Empty;
        public string Usuario { get; set; } = string.Empty;
    }

}


