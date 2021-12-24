using MimeKit;
using System.Collections.Generic;
using System.Linq;

namespace LabSolution.Notifications.EmailService
{
    public class Message
    {
        public List<MailboxAddress> To { get; set; }
        public string Subject { get; set; }
        public string Content { get; set; }

        public Message(IEnumerable<(string Name, string Address)> to, string subject, string content)
        {
            To = new List<MailboxAddress>();
            To.AddRange(to.Select(x => new MailboxAddress(x.Name, x.Address)));
            Subject = subject;
            Content = content;
        }
    }
}
