using Microsoft.Extensions.Configuration;
using System;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;

namespace ContractMonthlyClaimSystem.Services
{
    /// <summary>
    /// Service to send email notifications via SMTP.
    /// </summary>
    public class EmailNotificationService
    {
        private readonly IConfiguration _configuration;

        public EmailNotificationService(IConfiguration configuration)
        {
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        }

        /// <summary>
        /// Sends an email asynchronously.
        /// </summary>
        /// <param name="toEmail">Recipient email address</param>
        /// <param name="subject">Email subject</param>
        /// <param name="body">Email body (HTML allowed)</param>
        public async Task SendEmailAsync(string toEmail, string subject, string body)
        {
            if (string.IsNullOrWhiteSpace(toEmail))
                throw new ArgumentException("Recipient email cannot be empty.", nameof(toEmail));

            if (string.IsNullOrWhiteSpace(subject))
                subject = "(No Subject)";

            if (string.IsNullOrWhiteSpace(body))
                body = string.Empty;

            var smtpHost = _configuration["Email:SmtpHost"];
            var smtpPortStr = _configuration["Email:SmtpPort"];
            var smtpUser = _configuration["Email:SmtpUser"];
            var smtpPass = _configuration["Email:SmtpPass"];
            var fromEmail = _configuration["Email:FromEmail"];

            if (!int.TryParse(smtpPortStr, out int smtpPort))
                smtpPort = 587; // default SMTP port if config is invalid

            using var client = new SmtpClient(smtpHost, smtpPort)
            {
                Credentials = new NetworkCredential(smtpUser, smtpPass),
                EnableSsl = true
            };

            var mailMessage = new MailMessage
            {
                From = new MailAddress(fromEmail),
                Subject = subject,
                Body = body,
                IsBodyHtml = true
            };

            mailMessage.To.Add(toEmail);

            await client.SendMailAsync(mailMessage);
        }
    }
}
