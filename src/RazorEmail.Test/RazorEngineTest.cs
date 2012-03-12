using System;
using NUnit.Framework;

namespace RazorEmail.Test
{
    [TestFixture]
    public class RazorEngineTest
    {
        [Test]
        public void template_with_layout_should_render_layout_then_content()
        {
            var engine = new RazorEngine(@"..\..\Templates");

            var result = engine.RenderTempateToString("test_master_content", (object) null);

            Assert.AreEqual(@"<!DOCTYPE html>
<html>
<head><title></title></head>
<body>
<p>The is the content page</p>
</body>
</html>".Replace("\r\n", "\n"), result.Replace("\r\n", "\n"));

            engine.Dispose();   
        }
    }
}
