using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Net.Mime;
using System.Xml.Serialization;

namespace RazorEmail
{
    public class RazorMailer : IDisposable
    {
        private static readonly RazorMailer _staticMailer;
        private readonly IRazorEngine razorEngine;
        private readonly string baseDir;

        static RazorMailer()
        {
            var baseDir = ConfigurationManager.AppSettings["razor.mail.base.dir"];

            if(baseDir != null)
                _staticMailer = new RazorMailer(baseDir);
        }

        public RazorMailer(string baseDir = null, IRazorEngine razorEngine = null)
        {
            if(baseDir == null)
                  baseDir = ConfigurationManager.AppSettings["razor.mail.base.dir"];

            this.baseDir = baseDir;
            this.razorEngine = razorEngine ?? new RazorEngine(baseDir);
        }

        public static Email Build<T>(string templateName, T model, string toAddress = null, string toDisplayname = null)
        {
            if (_staticMailer == null)
                throw new ApplicationException("You must define the razor.mailer.base.dir appSettings in order to use the static method");

            return _staticMailer.Create(templateName, model, toAddress, toDisplayname);
        }

        public virtual Email Create<T>(string templateName, T model, string toAddress =null, string toDisplayName = null)
        {
            if (templateName == null) throw new ArgumentNullException("templateName");

            var email = CreateFromFile(templateName);

            var toAddressList = new List<Email.Address>();

            if(toAddress != null)
                toAddressList.Add(new Email.Address {Email = toAddress, Name = toDisplayName});

            if(email.To != null)
                toAddressList.AddRange(email.To);

            email.To = toAddressList.ToArray();

            email.Subject = razorEngine.RenderContentToString(email.Subject, model);

            if(email.Subject.Contains("\n")) throw new ApplicationException("The subject line cannot contain any newline characters");

            foreach (var view in email.Views)
            {
                var viewTemplateName = templateName + "." + view.MediaType.Replace('/', '_');
                bool templateExists = razorEngine.DoesTemplateExist(viewTemplateName, model);
               
                view.Content = templateExists ? razorEngine.RenderTempateToString(viewTemplateName, model) :
                                                razorEngine.RenderContentToString(view.Content, model)  ;
            }

            return email;
        }

        private Email CreateFromFile(string templateName)
        {
            var templateFilename = Path.Combine(baseDir, templateName + ".xml");

            if (!File.Exists(templateFilename))
                throw new ArgumentException(String.Format("The template {0} could not be found here: {1}", templateName,
                                                          templateFilename));
 
            var serializer = new XmlSerializer(typeof(Email));
            Email template;

            using (Stream stream = File.OpenRead(templateFilename))
            {
                template = serializer.Deserialize(stream) as Email;

                if(template == null)
                    throw new ArgumentException(String.Format("Could not deserialize template file: {0}",templateFilename));

                var defaultTextFilename = templateName + ".text_plain.cshtml";
                var defaultHtmlFilename = templateName + ".text_html.cshtml";

                var defaultViews = new List<Email.View>();

                if (template.Views == null && File.Exists(Path.Combine(baseDir, defaultTextFilename)))
                {
                    defaultViews.Add(new Email.View
                    {
                        MediaType = MediaTypeNames.Text.Plain
                    });
                }

                if (template.Views == null && File.Exists(Path.Combine(baseDir, defaultHtmlFilename)))
                {
                    defaultViews.Add(new Email.View
                    {
                        MediaType = MediaTypeNames.Text.Html,
                    });
                }

                if (template.Views == null)
                    template.Views = defaultViews.ToArray();

            }

            return template;
        }

        public void Dispose()
        {
            this.razorEngine.Dispose();
        }
    }
}