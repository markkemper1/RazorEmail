using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Mime;
using System.Xml.Serialization;
using RazorEngine.Templating;

namespace RazorEmail
{
    public class DefaultResolver : ITemplateResolver, IEmailResolver
    {
        private readonly string baseDir;

        public DefaultResolver(string baseDir)
        {
            if (baseDir == null) 
                throw new ArgumentNullException("baseDir");

            this.baseDir = baseDir;

            if (!Directory.Exists(baseDir))
                throw new ArgumentException(String.Format("The template directory does not exist! - {0} , Full path: {1}", baseDir, Path.GetFullPath(baseDir)));
        }

        string ITemplateResolver.Resolve(string name)
        {
            var path = Path.Combine(baseDir, name);

            if (File.Exists(path))
                return File.ReadAllText(path);

            if (File.Exists(path + ".cshtml"))
                return File.ReadAllText(path + ".cshtml");

            throw new ArgumentException(String.Format("The templated name \"{0}\" could not be resolved", name));
        }

        Email IEmailResolver.Resolve(string templateName)
        {
            var templateFilename = Path.Combine(baseDir, templateName + ".xml");

            if (!File.Exists(templateFilename))
                throw new ArgumentException(String.Format("The template {0} could not be found here: {1}", templateName,
                                                          templateFilename));

            var serializer = new XmlSerializer(typeof(Email));
            using (Stream stream = File.OpenRead(templateFilename))
            {
                Email template = serializer.Deserialize(stream) as Email;

                if (template == null)
                    throw new ArgumentException(String.Format("Could not deserialize template file: {0}", templateFilename));

                var defaultTextFilename = templateName + ".text_plain.cshtml";
                var defaultHtmlFilename = templateName + ".text_html.cshtml";

                var defaultViews = new List<Email.View>();

                if (template.Views == null)
                {
                    if (File.Exists(Path.Combine(baseDir, defaultTextFilename)))
                    {
                        defaultViews.Add(new Email.View
                        {
                            MediaType = MediaTypeNames.Text.Plain
                        });
                    }

                    if (File.Exists(Path.Combine(baseDir, defaultHtmlFilename)))
                    {
                        defaultViews.Add(new Email.View
                        {
                            MediaType = MediaTypeNames.Text.Html,
                        });
                    }
                }

                if (template.Views == null)
                    template.Views = defaultViews.ToArray();

                return template;
            }
        }
    }
}