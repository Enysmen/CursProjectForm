using System.Net.Mail;
using System.Net;

using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;

namespace CursProject.Service
{
    public class EmailSender : IEmailSender
    {

        private readonly IConfiguration _configuration;

        public EmailSender(IConfiguration configuration)
        {
            _configuration = configuration;
        }



        public Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            //var smtpClient = new SmtpClient(_configuration["Smtp:Host"])
            //{
            //    Port = int.Parse(_configuration["Smtp:Port"]),
            //    Credentials = new NetworkCredential(_configuration["Smtp:Username"], _configuration["Smtp:Password"]),
            //    EnableSsl = true,
            //};

            //var mailMessage = new MailMessage
            //{
            //    From = new MailAddress(_configuration["Smtp:From"]),
            //    Subject = subject,
            //    Body = htmlMessage,
            //    IsBodyHtml = true,
            //};
            //mailMessage.To.Add(email);

            //return smtpClient.SendMailAsync(mailMessage);

            return Task.CompletedTask;
        }
    }
}
