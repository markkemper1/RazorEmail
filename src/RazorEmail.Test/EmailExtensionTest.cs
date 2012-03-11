using System;
using System.Diagnostics;
using System.Net.Mail;
using System.Threading;
using NUnit.Framework;

namespace RazorMail.Test
{
    [TestFixture]
    public class EmailExtensionTest
    {
        [Test]
        public void should_send()
        {
            var message = new MailMessage("test@test.com", "test@test.com", "subject", "body");

            message.Send();
        }

        [Test]
        public void should_send_async()
        {
            var message = new MailMessage("test@test.com", "test@test.com", "subject", "body");

            message.SendAsync();
        }

        [Test]
        public void should_send_async_and_call_back()
        {
            var message = new MailMessage("test@test.com", "test@test.com", "subject", "body");

            var wait = new ManualResetEvent(false);
            message.SendAsync(x=>
                                  {
                                      Assert.AreEqual("test1State", x);
                                      Trace.WriteLine(x);
                                      wait.Set();
                                  } , "test1State");

            var result = wait.WaitOne(TimeSpan.FromSeconds(2));
            Assert.IsTrue(result);
        }

        [Test]
        public void should_add_any_headers_to_mail_message()
        {
            var email = new Email()
                            {
                                Headers = new Email.Header[2]
                                              {
                                                  new Email.Header() {Key = "a", Value = "A"},
                                                  new Email.Header() {Key = "b", Value = "B"},
                                              }
                            };

            var message = email.ToMailMessage();


            Assert.AreEqual("A", message.Headers["a"]);
            Assert.AreEqual("B", message.Headers["b"]);


        }
        //[Test]
        //public void should_call_send_mail_async_callback()
        //{
        //    var message = new MailMessage("test@test.com", "test@test.com", "subject", "body");

        //    var smtpClient = new SmtpClient();
        //    smtpClient.SendCompleted
        //}
    }
}
