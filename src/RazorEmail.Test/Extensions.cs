using System;

namespace RazorEmail.Test
{
    public static class Extensions
    {
        public static string CleanUpNewLines(this string text)
        {
            return text.Replace("\r\n", "\n").Replace("\r", "\n");
        }
    }
}
