﻿using System.Threading.Tasks;
using Reservas.Services;

namespace Reservas.Services
{
    public interface IEmailSender
    {
        Task SendAsync(string toEmail, string subject, string htmlBody);
    }
}
