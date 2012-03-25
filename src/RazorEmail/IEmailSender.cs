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
            using(var client = NewClient())
                client.Send(message);
        }

        internal static SmtpClient NewClient()
        {
            var client = new SmtpClient();
            /* Disposing with a blank host causes an exception */
            if (
                (client.DeliveryMethod == SmtpDeliveryMethod.SpecifiedPickupDirectory
                || client.DeliveryMethod == SmtpDeliveryMethod.PickupDirectoryFromIis
                )
                && String.IsNullOrEmpty(client.Host))
                client.Host = "localhost";
            return client;
        }
    }
}
