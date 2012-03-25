using System;
using System.Net.Mail;

namespace RazorEmail
{
    public interface IEmailSender
    {
        void Send(MailMessage message);
    }

    public class SimpleSmtpSender : IEmailSender
    {
        public void Send(MailMessage message)
        {
            using(var client = new SmtpClient())
                client.Send(message);
        }
    }
}
