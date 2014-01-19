using System;

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
    }
}
