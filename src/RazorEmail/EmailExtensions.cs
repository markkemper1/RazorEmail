using System;
using System.Linq;
using System.Net.Mail;
using System.Text;

namespace RazorMail
{
    public static class EmailExtensions
    {
        public static Email WithHeader(this Email email, string key, string value)
        {
            email.Headers = email.Headers.Union(new[] {new Email.Header() {Key = key, Value = value}}).ToArray();
            return email;
        }

        public static MailMessage ToMailMessage(this Email email)
        {
            var message = new MailMessage();

            if(email.From != null)
                message.From = new MailAddress(email.From.Email, email.From.Name);

            if (email.To != null)
            {
                foreach (var to in email.To)
                    message.To.Add(new MailAddress(to.Email, to.Name));
            }

            if (email.Bcc != null)
            {
                foreach (var bcc in email.Bcc)
                    message.Bcc.Add(new MailAddress(bcc.Email, bcc.Name));
            }

            if (email.CC != null)
            {
                foreach (var cc in email.CC)
                {
                    message.CC.Add(new MailAddress(cc.Email, cc.Name));
                }
            }

            if(email.Headers != null)
            {
                foreach (var header in email.Headers)
                {
                    message.Headers.Add(header.Key, header.Value);
                }
            }

            message.Subject = email.Subject;

            if (email.Views != null)
            {
                foreach (var view in email.Views)
                {
                    message.AlternateViews.Add(AlternateView.CreateAlternateViewFromString(view.Content,
                                                                                           view.Encoding ??
                                                                                           Encoding.Default,
                                                                                           view.MediaType));
                }
            }

            return message;
        }

        public static void Send(this MailMessage message)
        {
            var client = new SmtpClient();
            client.Send(message);
        }

        public static void SendAsync(this MailMessage message)
        {
            var client = new SmtpClient();

            client.SendAsync(message, String.Empty);
        }

        public static void SendAsync<T>(this MailMessage message, Action<T> action, T actionStateArgument)
        {
            var client= new SmtpClient();
            client.SendCompleted += (sender, args) => action(actionStateArgument);
            client.SendAsync(message, actionStateArgument);
        }

        public static void SendAsync<T>(this MailMessage message, Action<T, MailMessage> action, T actionStateArgument)
        {
            var args = Tuple.Create(actionStateArgument, message);
            SendAsync(message, x=> action(actionStateArgument, message), args);
        }
    }
}
