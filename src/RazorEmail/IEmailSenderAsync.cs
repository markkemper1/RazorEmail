using System.Net.Mail;
using System.Threading.Tasks;

namespace RazorEmail
{
    public interface IEmailSenderAsync
    {
        Task<T> SendAsync<T>(MailMessage message, T userToken);
    }
}