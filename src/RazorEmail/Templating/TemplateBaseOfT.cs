using System;
using RazorEngine.Templating;
using RazorEngine.Text;

namespace RazorEmail.Templating
{
    public class TemplateBase<T> : RazorEngine.Templating.TemplateBase<T>
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

        public IEncodedString Partial(string name)
        {
            return this.Raw(this.TemplateService.Resolve(name, null).Run(new ExecuteContext(ViewBag)));
        }

        public IEncodedString Partial(string name, T model)
        {
            return this.Raw(this.TemplateService.Resolve(name, model).Run(new ExecuteContext(ViewBag)));
        }
    }
}
