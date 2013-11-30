using System.Net.Mail;

namespace RazorEmail
{
    public interface IEmailSender
    {
        void Send(MailMessage message);
    }
}
