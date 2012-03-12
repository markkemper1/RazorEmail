using System;
using System.IO;
using RazorEngine.Templating;

namespace RazorEmail
{
    public interface IRazorEngine : IDisposable
    {
        bool DoesTemplateExist<T>(string tempalteName, T model);
        string RenderContentToString<T>(string content, T model);
        string RenderTempateToString<T>(string templateName, T model);
    }

    public class RazorEngine : IRazorEngine
    {
        private readonly ITemplateService templateService;

        public static Func<string, ITemplateService> DefaultTemplateServiceUsingBaseDir = baseDir =>
                                                                      new TemplateService(
                                                                          new EmailTemplateConfiguration(baseDir)
                                                                              {
                                                                                  Resolver =
                                                                                      new TemplateResolver(baseDir)
                                                                              });

       
        public RazorEngine(string baseDir)
            : this(baseDir, DefaultTemplateServiceUsingBaseDir(baseDir))
        {
        }

        public RazorEngine(string baseDir, ITemplateService templateService)
        {
            if (baseDir == null) throw new ArgumentNullException("baseDir");
            if (templateService == null) throw new ArgumentNullException("templateService");

            if(!Directory.Exists(baseDir))
                throw new ArgumentException("The base directory does not exist: " + baseDir);

            this.templateService = templateService;
        }

        public bool DoesTemplateExist<T>(string templateName, T model)
        {
            return this.templateService.Resolve(templateName, model) != null;
        }

        public string RenderContentToString<T>(string content, T model)
        {
            var template = templateService.CreateTemplate(content, model);
            return template.Run(new ExecuteContext());
        }

        public string RenderTempateToString<T>(string templateName, T model)
        {
            var template = templateService.Resolve(templateName, model);
            return template.Run(new ExecuteContext());
        }

        public void Dispose()
        {
            this.templateService.Dispose();
        }
    }
}
