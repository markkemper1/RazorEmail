using System;
using System.IO;
using System.Linq;
using System.Net.Mail;
using System.Net.Mime;
using System.Xml.Serialization;
using NUnit.Framework;

namespace RazorEmail.Test
{
    [TestFixture]
    public class RazorMailerTest
    {
        public class TestModel { public string Link; }
        public TestModel DefaultModel = new TestModel { Link = "http://testing.com" };

        [Test]
        public void ctor_should_valid_dir_exists()
        {
            Assert.Throws<ArgumentException>(() =>
                                                 {
                                                     new RazorMailer("doesn't exist");
                                                 });
        }

        [Test]
        public void Render_should_throw_if_null_template_name_passed()
        {
            var mailer = CreateTarget();
            Assert.Throws<ArgumentNullException>(() => mailer.Create(null, new object(), "test@test.com"));
        }

        [Test]
        public void Render_should_throw_if_template_not_found()
        {
            var mailer = CreateTarget();
            Assert.Throws<ArgumentException>(() => mailer.Create(@"not found", new object(), "test@test.com"));
        }

        [Test]
        public void Render_should_template_plain_text_body_from_file()
        {
            var message = CreateForgotPasswordResult().ToMailMessage();
            var result = GetViewContent(message, MediaTypeNames.Text.Plain);
            Assert.AreEqual(
                @"
Hello

If you have forgotten your password, just click the link below and we will reset it for you.

http://testing.com

Regards,

The cool z's".CleanUpNewLines(),
                result.Replace("\r\n", "\n"));

        }

        [Test]
        public void Render_should_template_inline_plain_text()
        {

            var razorMailer = this.CreateTarget();

            var message = razorMailer.Create("PlainTextInline", DefaultModel, "test@test.com").ToMailMessage();

            var result = GetViewContent(message, MediaTypeNames.Text.Plain);

            Assert.AreEqual(@"This is the
body http://testing.com".Replace("\r\n", "\n"), result);
        }

        [Test]
        public void Render_should_template_inline_html()
        {
            var razorMailer = this.CreateTarget();

            var message = razorMailer.Create("HtmlTextInline", DefaultModel, "test@test.com").ToMailMessage();

            var result = GetViewContent(message, MediaTypeNames.Text.Html);

            Assert.AreEqual(@"<html>
        
       is the
body http://testing.com
</html>".Replace("\r\n", "\n"), result);
        }

        [Test, Explicit]
        public void GenerateXml()
        {
            var template = new Email
                               {
                                             From = new Email.Address
                                             {
                                                 Email = "test@test.com",
                                                 Name = "Tester",
                                             },
                                             Bcc = new[] { new Email.Address
                                                               {
                                                                                          Email = "test@bcc.com",
                                                                                          Name = "spooky"
                                                                                      }},
                                             Subject = "The subject line",
                                             Views = new[]
                                                         {
                                                             new Email.View
                                                                 {
                                                                     MediaType = "text/plain",
                                                                     Content = "This is a test template",
                                                                }
                                                         }
                                         };

            var serializer = new XmlSerializer(typeof(Email));
            using (var ms = new MemoryStream())
            {
                serializer.Serialize(ms, template);

                ms.Position = 0;

                var reader = new StreamReader(ms);

                Console.WriteLine(reader.ReadToEnd());

            }

        }

        [Test]
        public void Render_should_preserve_bcc_in_email()
        {
            var razorMailer = this.CreateTarget();

            MailMessage message = razorMailer.Create("Bcc", DefaultModel, "test@test.com").ToMailMessage();

            Assert.AreEqual(1, message.Bcc.Count);
            Assert.AreEqual("test@bcc.com", message.Bcc[0].Address);
            Assert.AreEqual("spooky", message.Bcc[0].DisplayName);
        }

        [Test]
        public void Render_should_preserve_cc_in_email()
        {
            var razorMailer = this.CreateTarget();

            MailMessage message = razorMailer.Create("cc", DefaultModel, "test@test.com").ToMailMessage();

            Assert.AreEqual(3, message.CC.Count);
            Assert.AreEqual("test@bcc.com", message.CC[0].Address);
            Assert.AreEqual("visible person", message.CC[0].DisplayName);

            Assert.AreEqual("test3@bcc.com", message.CC[2].Address);
            Assert.AreEqual("visible person 3", message.CC[2].DisplayName);
        }

        [Test]
        public void Render_should_set_the_to_addresss()
        {
            var razorMailer = this.CreateTarget();

            MailMessage message = razorMailer.Create("cc", DefaultModel, "test@test.com").ToMailMessage();

            Assert.AreEqual(1, message.To.Count);
            Assert.AreEqual("test@test.com", message.To[0].Address);
        }

        [Test]
        public void Render_should_set_any_extra_addresss_set_in_the_template()
        {
            var razorMailer = this.CreateTarget();

            MailMessage message = razorMailer.Create("multpleToAddresses", DefaultModel, "test@test.com", "primary to").ToMailMessage();

            Assert.AreEqual(4, message.To.Count);
            Assert.AreEqual("test@test.com", message.To[0].Address);
            Assert.AreEqual("primary to", message.To[0].DisplayName);

            Assert.AreEqual("test3@to.com", message.To[3].Address);
            Assert.AreEqual("visible person 3", message.To[3].DisplayName);
        }

        [Test]
        public void Render_should_set_the_from_address()
        {
            var message = CreateForgotPasswordResult();

            Assert.AreEqual("noreply@jobping.com", message.From.Email);
            Assert.AreEqual("jobping", message.From.Name);
        }

        [Test]
        public void Render_should_template_subject_line()
        {
            var message = CreateForgotPasswordResult();

            Assert.AreEqual("Reset Password Request http://testing.com", message.Subject);
        }

        private string GetViewContent(MailMessage message, string mediaType)
        {
            var view = message.AlternateViews.FirstOrDefault(x => x.ContentType.MediaType == mediaType);

            if (view == null)
                return null;

            using (TextReader reader = new StreamReader(view.ContentStream))
                return reader.ReadToEnd();
        }

        [Test]
        public void Render_should_create_html_body()
        {
            var message = CreateForgotPasswordResult().ToMailMessage();

            var result = GetViewContent(message, "text/html");

            Assert.AreEqual(
                @"<html>
<title>Reset Password Request http://testing.com</title>

<body>


Hello

If you have forgotten your password, just click the link below and we will reset it for you.

<a href=""http://testing.com"" >http://testing.com</a>

Regards,

The cool z's

</body>
</html>".CleanUpNewLines(),
                result.CleanUpNewLines());

        }

        private Email CreateForgotPasswordResult()
        {
            var target = this.CreateTarget();
            return target.Create("ForgotPassword", DefaultModel, "test@test.com");
        }

        private RazorMailer CreateTarget()
        {
            return new RazorMailer(@"..\..\Templates");
        }
    }
}