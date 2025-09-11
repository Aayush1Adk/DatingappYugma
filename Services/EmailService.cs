using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using System;

namespace DatingAppBackend.Services
{
    public class EmailService
    {
        private readonly string _host;
        private readonly int _port;
        private readonly string _user;
        private readonly string _pass;
        private readonly string _from;
        private readonly string _fromName;

        public EmailService(IConfiguration config)
        {
            // Prefer environment variables, fallback to appsettings.json
            _host     = Environment.GetEnvironmentVariable("SMTP_HOST")     ?? config["Smtp:Host"]     ?? throw new InvalidOperationException("SMTP host missing");
            _port     = int.Parse(Environment.GetEnvironmentVariable("SMTP_PORT") ?? config["Smtp:Port"] ?? "587");
            _user     = Environment.GetEnvironmentVariable("SMTP_USER")     ?? config["Smtp:User"]     ?? throw new InvalidOperationException("SMTP user missing");
            _pass     = Environment.GetEnvironmentVariable("SMTP_PASS")     ?? config["Smtp:Pass"]     ?? throw new InvalidOperationException("SMTP password missing");
            _from     = Environment.GetEnvironmentVariable("SMTP_FROM")     ?? config["Smtp:From"]     ?? _user;
            _fromName = Environment.GetEnvironmentVariable("SMTP_FROMNAME") ?? config["Smtp:FromName"] ?? "YUGMA";
        }

        public async Task SendOtpEmail(string toEmail, string otp)
        {
            using var client = new SmtpClient(_host, _port)
            {
                Credentials = new NetworkCredential(_user, _pass),
                EnableSsl = true // Most SMTP servers require TLS on port 587
            };

            var message = new MailMessage
            {
                From = new MailAddress(_from, _fromName),
                Subject = "Your YUGMA verification code",
                Body = $"From YUGMA, your email verification code is: {otp}\nThis code expires in 2 minutes.",
                IsBodyHtml = false
            };
            message.To.Add(toEmail);

            await client.SendMailAsync(message);
        }
    }
}
