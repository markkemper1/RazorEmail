using System;
using System.Net.Mail;
using System.Threading.Tasks;

namespace RazorEmail
{
    public class SimpleSmtpSender : IEmailSender, IEmailSenderAsync
    {
        public void Send(MailMessage message)
        {
            using(var client = NewClient())
                client.Send(message);
        }

        public Task<T> SendAsync<T>(MailMessage message, T userToken)
        {
            using (var client = NewClient())
                return client.SendTask(message, userToken);
        }

        private static SmtpClient NewClient()
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