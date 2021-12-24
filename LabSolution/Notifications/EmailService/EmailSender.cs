using MailKit.Net.Smtp;
using MimeKit;
using System.Threading.Tasks;

namespace LabSolution.Notifications.EmailService
{
    public interface IEmailSender
    {
        Task SendEmailAsync(Message message, byte[] attachment = null, string attachmentName = null);
    }

    public class EmailSender : IEmailSender
    {
        private readonly EmailConfiguration _emailConfig;
        public EmailSender(EmailConfiguration emailConfig)
        {
            _emailConfig = emailConfig;
        }

        public async Task SendEmailAsync(Message message, byte[] attachment = null, string attachmentName = null)
        {
            var mailMessage = CreateEmailMessage(message, attachment, attachmentName);
            await SendAsync(mailMessage);
        }

        private MimeMessage CreateEmailMessage(Message message, byte[] attachmentBytes = null, string attachmentName = null)
        {
            var emailMessage = new MimeMessage();
            emailMessage.From.Add(new MailboxAddress(_emailConfig.FromName, _emailConfig.FromAddress));
            emailMessage.To.AddRange(message.To);
            emailMessage.Subject = message.Subject;

            var builder = new BodyBuilder { HtmlBody = message.Content };

            if(attachmentBytes is not null && attachmentName is not null)
                builder.Attachments.Add(attachmentName, attachmentBytes, ContentType.Parse(System.Net.Mime.MediaTypeNames.Application.Pdf));

            emailMessage.Body = builder.ToMessageBody();
            return emailMessage;
        }

        private async Task SendAsync(MimeMessage mailMessage)
        {
            using (var client = new SmtpClient())
            {
                try
                {
                    await client.ConnectAsync(_emailConfig.SmtpServer, _emailConfig.Port, _emailConfig.UseSsl);
                    await client.AuthenticateAsync(_emailConfig.UserName, _emailConfig.Password);
                    await client.SendAsync(mailMessage);
                }
                finally
                {
                    await client.DisconnectAsync(true);
                    client.Dispose();
                }
            }
        }
    }
}
