using System.Net;
using System.Net.Mail;

namespace ProjectWBSAPI.Helper
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _config;

        public EmailService(IConfiguration config)
        {
            _config = config;
        }

        public async Task SendEmailAsync(string to,string cc, string subject, string body)
        {
            try
            {
                var smtpClient = new SmtpClient(_config["Email:SmtpHost"])
                {
                    Port = int.Parse(_config["Email:Port"]!),
                    //Credentials = new NetworkCredential(
                    //    _config["Email:Username"],
                    //    _config["Email:Password"]
                    //),
                    EnableSsl = false
                };

                var mailMessage = new MailMessage
                {
                    From = new MailAddress(_config["Email:From"]!),
                    Subject = subject,
                    Body = body,
                    IsBodyHtml = true
                };

                mailMessage.To.Add(to);
                mailMessage.CC.Add(cc);

                await smtpClient.SendMailAsync(mailMessage);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}
