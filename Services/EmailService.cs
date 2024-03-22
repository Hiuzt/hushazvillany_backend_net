using SendGrid.Helpers.Mail;
using SendGrid;
using System.Net;
using System.Net.Mail;

namespace hushazvillany_backend.Services
{
    public class EmailService:IEmailService
    {
        public string SendEmail(string toEmail, string emailSubject, string plainTextContent, string htmlContent)
        {
            var apiKey = "SENDGRID_API_KEY";
            var client = new SendGridClient(apiKey);
            var from = new EmailAddress("kerner.daniel1@gmail.com", "Kerner Dániel");
            var to = new EmailAddress(toEmail);
            var msg = MailHelper.CreateSingleEmail(from, to, emailSubject, plainTextContent, htmlContent);
            var response = client.SendEmailAsync(msg);

            return "Ok";
        }
    }
}
