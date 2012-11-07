using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Net.Mime;
using System.Xml.Serialization;
using RazorEngine;
using RazorEngine.Templating;

namespace RazorEmail
{
    public class RazorMailer 
    {
        private readonly string baseDir;

        public RazorMailer()
            :this(null)
        {
        }

        static RazorMailer()
        {
            var baseDir = GetBaseDir();

            Razor.SetTemplateService(new TemplateService(new EmailTemplateConfiguration(baseDir)
                                                             {
                                                                 Resolver = new TemplateResolver(baseDir)
                                                             }));
        }

        private static string GetBaseDir(string baseDir = null)
        {
             if(baseDir == null)
                baseDir = ConfigurationManager.AppSettings["razor.email.base.dir"];

            if (baseDir == null)
                throw new ApplicationException("You must supply have a AppSetting called 'razor.email.base.dir'");

            if (baseDir.Contains("|DataDirectory|"))
                baseDir = baseDir.Replace("|DataDirectory|", (string)AppDomain.CurrentDomain.GetData("DataDirectory"));

            return baseDir;
        }

        public RazorMailer(string baseDir = null)
        {
            this.baseDir = GetBaseDir(baseDir); ;

            if(!Directory.Exists(this.baseDir))
                throw new ArgumentException("The baseDir supplied doesn't exist: " + this.baseDir);
        }

        public static Email Build<T>(string templateName, T model, string toAddress = null, string toDisplayname = null)
        {
            var mailer = new RazorMailer();
            return mailer.Create(templateName, model, toAddress, toDisplayname);
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

            email.Subject = Razor.Parse(email.Subject, model, email.Subject);// razorEngine.RenderContentToString(email.Subject, model);

            if(email.Subject.Contains("\n")) throw new ApplicationException("The subject line cannot contain any newline characters");

            foreach (var view in email.Views)
            {
                var viewTemplateName = templateName + "." + view.MediaType.Replace('/', '_');

                var fileContent = Resolve(viewTemplateName);

                bool templateExists = fileContent != null;
               
                view.Content = templateExists ?  Razor.Parse(fileContent, model, viewTemplateName) :
                                                Razor.Parse(view.Content, model, viewTemplateName); //razorEngine.RenderContentToString(view.Content, model);
            }

            return email;
        }

        public string Resolve(string name)
        {
            var path = Path.Combine(baseDir, name);

            if (File.Exists(path))
                return File.ReadAllText(path);

            if (File.Exists(path + ".cshtml"))
                return File.ReadAllText(path + ".cshtml");

            return null;
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

        
    }
}