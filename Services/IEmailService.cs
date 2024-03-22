namespace hushazvillany_backend.Services
{
    public interface IEmailService
    {
        string SendEmail(string email, string subject, string plainTextContent, string htmlContent);
    }
}
