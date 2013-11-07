using System;
using System.Configuration;
using System.IO;
using System.Reflection;
using RazorEngine.Templating;

namespace RazorEmail
{
    public class TemplateResolver : ITemplateResolver
    {
        protected internal readonly string BaseDir;
		private readonly string _assemblyName;
		private readonly Assembly _assembly;
		private readonly bool _useEmbeddedResource;


        public TemplateResolver(string baseDir = null)
        {
			if (baseDir == null)
				baseDir = ConfigurationManager.AppSettings["razor.email.base.dir"];

			if (baseDir == null)
				throw new ApplicationException("You must supply have a AppSetting called 'razor.email.base.dir'");

			if (baseDir.Contains("|DataDirectory|"))
				baseDir = baseDir.Replace("|DataDirectory|", (string)AppDomain.CurrentDomain.GetData("DataDirectory"));

			if (baseDir.Contains("~"))
				baseDir = System.Web.Hosting.HostingEnvironment.MapPath(baseDir);

	        this.BaseDir = baseDir;

			if (!String.IsNullOrEmpty(ConfigurationManager.AppSettings["razor.email.embedded"]))
			{
				var embedded = false;
				Boolean.TryParse(ConfigurationManager.AppSettings["razor.email.embedded"], out embedded);
				_useEmbeddedResource = embedded;
				this._assemblyName = ConfigurationManager.AppSettings["razor.email.assemblyName"];
				this._assembly = Assembly.Load(this._assemblyName);
			}
			else
			{
				if (!Directory.Exists(this.BaseDir))
					throw new ArgumentException("The baseDir supplied doesn't exist: " + this.BaseDir);
			}

        }

	    public Stream GetStream(string name)
	    {
			if (this._useEmbeddedResource)
			{
				if (this._assembly == null)
					throw new ApplicationException("Assembly not found");

				return _assembly.GetManifestResourceStream(name);
			}

			var path = Path.Combine(BaseDir, name);
			if (File.Exists(path))
				return File.OpenRead(path);

			if (File.Exists(path + ".cshtml"))
				return File.OpenRead(path + ".cshtml");

			return null;
		}

        public string Resolve(string name)
        {
	        var stream = this.GetStream(name);
	        if (stream == null)
		        return "";
			using (var reader = new StreamReader(stream))
			        return reader.ReadToEnd();

        }
    }
}