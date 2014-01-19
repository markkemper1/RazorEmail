using System;
using RazorEngine.Templating;
using RazorEngine.Text;

namespace RazorEmail.Templating
{
    public class TemplateBase : RazorEngine.Templating.TemplateBase
    {
        /// <summary>
        ///     Please use 'Layout' instead, will be removed in future version
        /// </summary>
        [Obsolete("Please use 'Layout' instead, will be removed in future version", false)]
        public virtual string _Layout
        {
            get { return base.Layout; }
            set { base.Layout = value; }
        }

        public IEncodedString Partial(string name, object model = null)
        {
            return this.Raw(TemplateService.Resolve(name, model).Run(new ExecuteContext(ViewBag)));
        }
    }
}