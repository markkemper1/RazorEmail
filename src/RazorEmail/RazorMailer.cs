using System;
using System.Collections.Generic;
using System.Configuration;
using RazorEngine.Templating;

namespace RazorEmail
{
    public class RazorMailer 
    {
        private readonly ITemplateResolver templateResolver;
        private readonly ITemplateService templateService;
        private readonly IEmailResolver emailResolver;

        public RazorMailer(ITemplateResolver templateResolver, IEmailResolver emailResolver)
        {
            if(templateResolver == null)
                throw new ArgumentNullException("templateResolver");
            if(emailResolver == null)
                throw new ArgumentNullException("emailResolver");

            this.templateResolver = templateResolver;
            this.emailResolver = emailResolver;
        }

        public RazorMailer(string baseDir = null)
        {
            var defaultResolver = new DefaultResolver(baseDir ?? GetDefaultBaseDir());

            this.templateResolver = defaultResolver;
            this.emailResolver = defaultResolver;
            this.templateService = new TemplateService(new EmailTemplateConfiguration(baseDir)
            {
                Resolver = defaultResolver
            });
        }

        private static string GetDefaultBaseDir()
        {
             string baseDir = ConfigurationManager.AppSettings["razor.email.base.dir"];

            if (baseDir == null)
                throw new ApplicationException("You must supply have a AppSetting called 'razor.email.base.dir'");

            if (baseDir.Contains("|DataDirectory|"))
                baseDir = baseDir.Replace("|DataDirectory|", (string)AppDomain.CurrentDomain.GetData("DataDirectory"));

            if (baseDir.Contains("~"))
                baseDir = System.Web.Hosting.HostingEnvironment.MapPath(baseDir);

            return baseDir;
        }
        
        public static Email Build<T>(string templateName, T model, string toAddress = null, string toDisplayname = null)
        {
            var mailer = new RazorMailer();
            return mailer.Create(templateName, model, toAddress, toDisplayname);
        }

        public virtual Email Create<T>(string templateName, T model, string toAddress =null, string toDisplayName = null)
        {
            if (templateName == null) 
                throw new ArgumentNullException("templateName");

            var email = emailResolver.Resolve(templateName);

            var toAddressList = new List<Email.Address>();

            if(toAddress != null)
                toAddressList.Add(new Email.Address {Email = toAddress, Name = toDisplayName});

            if(email.To != null)
                toAddressList.AddRange(email.To);

            email.To = toAddressList.ToArray();

            email.Subject = templateService.Parse(email.Subject, model, email.Subject);// razorEngine.RenderContentToString(email.Subject, model);

            if(email.Subject.Contains("\n")) throw new ApplicationException("The subject line cannot contain any newline characters");

            foreach (var view in email.Views)
            {
                var viewTemplateName = templateName + "." + view.MediaType.Replace('/', '_');

                var fileContent = templateResolver.Resolve(viewTemplateName);

                bool templateExists = fileContent != null;

                view.Content = templateExists ? templateService.Parse(fileContent, model, viewTemplateName) :
                                                templateService.Parse(view.Content, model, viewTemplateName); //razorEngine.RenderContentToString(view.Content, model);
            }

            return email;
        }
        
    }
}