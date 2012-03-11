using System;
using System.IO;
using RazorEngine.Templating;

namespace RazorMail
{
    public class TemplateResolver : ITemplateResolver
    {
        private readonly string baseDir;

        public TemplateResolver(string baseDir)
        {
            if (baseDir == null) throw new ArgumentNullException("baseDir");
            this.baseDir = baseDir;

            if (!Directory.Exists(baseDir))
                throw new ArgumentException(String.Format("The template directory does not exist! ", baseDir)); 
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
    }
}