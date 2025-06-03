using System.Net.Mail;
using System.Net;
using Microsoft.Extensions.Configuration;

namespace Framework_Project.Areas.Admin.Repository
{
    public class EmailSender : IEmailSender
    {
        private readonly IConfiguration _configuration;

        public EmailSender(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public Task SendEmailAsync(string email, string subject, string message)
        {
            var client = new SmtpClient("smtp.gmail.com", 587)
            {
                EnableSsl = true,
                UseDefaultCredentials = false,
                Credentials = new NetworkCredential(
                    _configuration["GoogleMail:Email"],
                    _configuration["GoogleMail:AppPassword"])
            };

            return client.SendMailAsync(
                new MailMessage(from: _configuration["GoogleMail:Email"],
                                to: email,
                                subject,
                                message
                                ));
        }
    }
}
