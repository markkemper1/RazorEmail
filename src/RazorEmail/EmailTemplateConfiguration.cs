using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Xml;
using System.Xml.Linq;
using RazorEngine;
using RazorEngine.Compilation;
using RazorEngine.Compilation.Inspectors;
using RazorEngine.Configuration;
using RazorEngine.Templating;
using RazorEngine.Text;

namespace RazorMail
{
    public class EmailTemplateConfiguration : ITemplateServiceConfiguration
    {
        readonly TemplateServiceConfiguration innerConfig = new TemplateServiceConfiguration();

        public EmailTemplateConfiguration(string baseDir)
        {
            string configFile = Path.Combine(baseDir, "razor.config");

            RazorEngineConfigurationSection config = null;

            if(File.Exists(configFile))
            {
                using (var stream = File.OpenRead(configFile))
                {
                    var doc = XDocument.Load(stream);

                    var xElement = doc.Element("razorEngine");
                    if(xElement == null)
                        throw new ApplicationException("There should be a root element called razorEngine");
                    config = GetSection<RazorEngineConfigurationSection>(xElement.ToString());
                }
            }
            else
            {
                Trace.WriteLine(String.Format("Razor email configuration file not found at: {0}. using defaults", configFile));
            }

            innerConfig.Language = (config == null)
                                       ? Language.CSharp
                                       : config.DefaultLanguage;
        }

        public T GetSection<T>(string sectionXml) where T: new()
        {
            T section = new T();
            using (var stringReader = new StringReader(sectionXml))
            using (XmlReader reader = XmlReader.Create(stringReader, new XmlReaderSettings() { CloseInput = true }))
            {
                reader.Read();
                section.GetType().GetMethod("DeserializeElement", BindingFlags.NonPublic | BindingFlags.Instance).Invoke(section, new object[] { reader, true });
            }
            return section;
        }

        /// <summary>
        /// Gets or sets the activator.
        /// </summary>
        public IActivator Activator { get { return innerConfig.Activator; } }

        /// <summary>
        /// Gets or sets the base template type.
        /// </summary>
        public Type BaseTemplateType { get { return innerConfig.BaseTemplateType; } }

        /// <summary>
        /// Gets the set of code inspectors.
        /// </summary>
        IEnumerable<ICodeInspector> ITemplateServiceConfiguration.CodeInspectors
        {
            get { return CodeInspectors; }
        }

        /// <summary>
        /// Gets the set of code inspectors.
        /// </summary>
        public IList<ICodeInspector> CodeInspectors { get { return (IList<ICodeInspector>)innerConfig.CodeInspectors; } }

        /// <summary>
        /// Gets or sets the compiler service factory.
        /// </summary>
        public ICompilerServiceFactory CompilerServiceFactory { get { return innerConfig.CompilerServiceFactory; } }

        /// <summary>
        /// Gets whether the template service is operating in debug mode.
        /// </summary>
        public bool Debug { get { return innerConfig.Debug; } }

        /// <summary>
        /// Gets or sets the encoded string factory.
        /// </summary>
        public IEncodedStringFactory EncodedStringFactory { get { return innerConfig.EncodedStringFactory; } }

        /// <summary>
        /// Gets or sets the language.
        /// </summary>
        public Language Language { get { return innerConfig.Language; } }

        /// <summary>
        /// Gets or sets the collection of namespaces.
        /// </summary>
        public ISet<string> Namespaces { get { return innerConfig.Namespaces; } }

        /// <summary>
        /// Gets or sets the template resolver.
        /// </summary>
        public ITemplateResolver Resolver { get; set; } 
    }
}