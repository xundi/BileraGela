using System;
using System.Collections.Generic;              // Necesario para IEnumerable<>
using System.ComponentModel.DataAnnotations;
namespace Reservas.Models
{
    public class EmailSettings
    {
        public string SmtpServer { get; set; } = "";
        public int SmtpPort { get; set; }
        public bool EnableSsl { get; set; }
        public string UserName { get; set; } = "";
        public string Password { get; set; } = "";
        public string FromName { get; set; } = "";
        public string FromAddress { get; set; } = "";
    }
}


