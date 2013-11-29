namespace RazorEmail
{
    public interface IEmailResolver
    {
        Email Resolve(string templateName);
    }
}