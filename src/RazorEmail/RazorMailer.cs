using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Net.Mime;
using System.Reflection;
using System.Xml.Serialization;
using RazorEngine;
using RazorEngine.Templating;

namespace RazorEmail
{
    public class RazorMailer 
    {
	    private static TemplateResolver _resolver;


        static RazorMailer()
        {
	        _resolver = new TemplateResolver(); 

			Razor.SetTemplateService(new TemplateService(new EmailTemplateConfiguration(_resolver.BaseDir)
                                                             {
                                                                 Resolver = _resolver
                                                             }));







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

                var fileContent = _resolver.Resolve(viewTemplateName);

                var templateExists = fileContent != null;
               
                view.Content = templateExists ?  Razor.Parse(fileContent, model, viewTemplateName) :
                                                Razor.Parse(view.Content, model, viewTemplateName); //razorEngine.RenderContentToString(view.Content, model);
            }

            return email;
        }





        private Email CreateFromFile(string templateName)
        {
            var templateFilename = templateName + ".xml";

			//if (!File.Exists(templateFilename))
			//	throw new ArgumentException(String.Format("The template {0} could not be found here: {1}", templateName,
			//											  templateFilename));
 
            var serializer = new XmlSerializer(typeof(Email));
            Email template;

            using (Stream stream = _resolver.GetStream(templateFilename))
            {
                template = serializer.Deserialize(stream) as Email;

                if(template == null)
                    throw new ArgumentException(String.Format("Could not deserialize template file: {0}",templateFilename));

                var defaultTextFilename = templateName + ".text_plain.cshtml";
                var defaultHtmlFilename = templateName + ".text_html.cshtml";

                var defaultViews = new List<Email.View>();

				if (template.Views == null && _resolver.GetStream(Path.Combine(_resolver.BaseDir, defaultTextFilename)) != null)
                {
                    defaultViews.Add(new Email.View
                    {
                        MediaType = MediaTypeNames.Text.Plain
                    });
                }

				if (template.Views == null && _resolver.GetStream(Path.Combine(_resolver.BaseDir, defaultHtmlFilename)) != null)
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