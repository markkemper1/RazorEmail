using System;
using System.Linq;
using System.Net.Mail;
using System.Text;

namespace RazorEmail
{
    public static class EmailExtensions
    {
        public static Email WithHeader(this Email email, string key, string value)
        {
            email.Headers = email.Headers.Union(new[] {new Email.Header {Key = key, Value = value}}).ToArray();
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
            using (var client = NewClient())
            {
                client.Send(message);
            }
        }

        public static void SendAsync(this MailMessage message)
        {
            message.SendAsync(x => { }, 0);
        }

        public static void SendAsync<T>(this MailMessage message, Action<T> action, T actionStateArgument)
        {
            message.SendAsync((arg, m) => action(arg), actionStateArgument);
        }

        public static void SendAsync<T>(this MailMessage message, Action<T, MailMessage> action, T actionStateArgument)
        {
            var client = NewClient();

            client.SendCompleted += (sender, args) =>
            {
                action(actionStateArgument, message);
                client.Dispose();
            };
            client.SendAsync(message, actionStateArgument);
        }

        internal static SmtpClient NewClient()
        { 
            var client = new SmtpClient();
            /* Disposing with a blank host causes an exception */
            if( 
                (client.DeliveryMethod == SmtpDeliveryMethod.SpecifiedPickupDirectory
                || client.DeliveryMethod == SmtpDeliveryMethod.PickupDirectoryFromIis
                )
                && String.IsNullOrEmpty(client.Host))
                client.Host = "localhost";
            return client;
        }
    }
}
