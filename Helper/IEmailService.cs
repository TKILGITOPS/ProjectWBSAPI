namespace ProjectWBSAPI.Helper
{
    public interface IEmailService
    {
        Task SendEmailAsync(string to,string cc, string subject, string body);
    }
}
