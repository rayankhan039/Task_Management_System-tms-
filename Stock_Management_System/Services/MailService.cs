namespace Task_Management_System.Services
{
    using System.Net;
    using System.Net.Mail;
    using Microsoft.Extensions.Configuration;
    public class MailService
    {
        private readonly IConfiguration _config;

        public MailService(IConfiguration config)
        {
            _config = config;
        }

        public async Task SendEmailAsync(string toEmail,string subject,string body)
        {
            var smtpClient = new SmtpClient(_config["EmailSettings:SMTPServer"])
            {
                Port = int.Parse(_config["EmailSettings:SMTPPort"]!),
                Credentials = new NetworkCredential(
                   _config["EmailSettings:SenderEmail"],
                   _config["EmailSettings:SenderPassword"]),
                EnableSsl = true,
            };

            var message = new MailMessage
            {
                From = new MailAddress(_config["EmailSettings:SenderEmail"]!),
                Subject = subject,
                Body = body,
                IsBodyHtml = true,
            };

            message.To.Add(toEmail);
            await smtpClient.SendMailAsync(message);
        }
    }
}
