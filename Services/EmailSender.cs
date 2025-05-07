using IT15_Project.Services;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.Extensions.Options;
using SendGrid;
using SendGrid.Helpers.Mail;
using System.Threading.Tasks;

namespace IT15_Project.Services
{
    public class EmailSender : IEmailSender
    {
        private readonly string _sendGridKey;

        public EmailSender(IOptions<AuthMessageSenderOptions> options)
        {
            _sendGridKey = options.Value.SendGridKey;
        }

        public async Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            var client = new SendGridClient(_sendGridKey);
            var msg = new SendGridMessage
            {
                From = new EmailAddress("e.deono.537918@umindanao.edu.ph", "SundoGO"),
                Subject = subject,
                HtmlContent = htmlMessage,
                PlainTextContent = htmlMessage

            };
            msg.AddTo(new EmailAddress(email));

            await client.SendEmailAsync(msg);
        }
    }
}
