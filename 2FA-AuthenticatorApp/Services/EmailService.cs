using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using System.Net.Mail;
using System.Net;

namespace TwoFA_AuthenticatorApp.Services
{
    public class EmailService : IEmailSender
    {
        private readonly string _smtpServer = "smtp.example.com"; // Replace with your SMTP server
        private readonly string _smtpUsername = "your_email@example.com"; // Replace with your email
        private readonly string _smtpPassword = "your_password"; // Replace with your email password
        private readonly int _smtpPort = 587; // Replace with your SMTP port

        public async Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            var smtpClient = new SmtpClient(_smtpServer)
            {
                Port = _smtpPort,
                Credentials = new NetworkCredential(_smtpUsername, _smtpPassword),
                EnableSsl = true,
            };

            var mailMessage = new MailMessage
            {
                From = new MailAddress(_smtpUsername),
                Subject = subject,
                Body = htmlMessage,
                IsBodyHtml = true,
            };

            mailMessage.To.Add(email);

            await smtpClient.SendMailAsync(mailMessage);
        }
    }
    }

