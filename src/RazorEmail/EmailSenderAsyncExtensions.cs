using System.Net.Mail;
using System.Threading.Tasks;

namespace RazorEmail
{
    public static class EmailSenderAsyncExtensions
    {
        public static Task SendAsync(this IEmailSenderAsync sender, MailMessage message)
        {
            return sender.SendAsync(message, (object) null);
        }
    }
}