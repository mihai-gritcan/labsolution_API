using LabSolution.Dtos;
using LabSolution.HttpModels;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LabSolution.Notifications.EmailService;
using System;
using System.IO;
using System.Text;

namespace LabSolution.Notifications
{
    public interface INotificationManager
    {
        public Task NotifyOrderCompleted(ProcessedOrderForPdf orderForPdf, LabConfigAddresses labConfigs, byte[] pdfBytes);
        public Task NotifyOrdersCreated(IEnumerable<CreatedOrdersResponse> createdOrders, LabConfigAddresses labConfigs);
    }

    public class NotificationManager : INotificationManager
    {
        private const string DATE_TIME_FORMAT = "dd-MM-yyyy HH:mm";

        private readonly IEmailSender _emailSender;

        public NotificationManager(IEmailSender emailSender)
        {
            _emailSender = emailSender;
        }

        public async Task NotifyOrderCompleted(ProcessedOrderForPdf orderForPdf, LabConfigAddresses labConfigs, byte[] pdfBytes)
        {
            var subject = $"COVID-19 [Rezultat serviciu din data/ora: {orderForPdf.OrderDate.ToString(DATE_TIME_FORMAT)}]";

            var fullName = $"{orderForPdf.Customer.LastName} {orderForPdf.Customer.FirstName}";

            var attachmentName = $"{fullName.Replace(" ", "")}_{orderForPdf.TestType}_{orderForPdf.OrderDate:dd-MM-yyyy}.pdf";
            var messageText = await MessageProvider.PrepareMessage(EmailTemplateLoader.NotificationType.OrderCompleted, fullName, orderForPdf.OrderDate, labConfigs);

            var message = new Message(new List<(string Name, string Address)> { (fullName, orderForPdf.Customer.Email) }, subject, messageText);

            await _emailSender.SendEmailAsync(message, pdfBytes, attachmentName);
        }

        public async Task NotifyOrdersCreated(IEnumerable<CreatedOrdersResponse> createdOrders, LabConfigAddresses labConfigs)
        {
            foreach (var item in createdOrders.Where(x => !string.IsNullOrWhiteSpace(x.Customer.Email)))
            {
                var subject = $"COVID-19 [Programare serviciu la data/ora: {item.Scheduled.ToString(DATE_TIME_FORMAT)}]";

                var fullName = $"{item.Customer.LastName} {item.Customer.FirstName}";

                var messageText = await MessageProvider.PrepareMessage(EmailTemplateLoader.NotificationType.OrderCreated, fullName, item.Scheduled, labConfigs);

                var message = new Message(new List<(string Name, string Address)> { (fullName, item.Customer.Email) }, subject, messageText);
                await _emailSender.SendEmailAsync(message);
            }
        }


        static class MessageProvider
        {
            private const string _customerNameKey = "#CUSTOMER_NAME_KEY";
            private const string _scheduledDateKey = "#SCHEDULED_DATE_KEY";

            private const string _labNameKey = "#LAB_NAME_KEY";
            private const string _labAddressKey = "#LAB_ADDRESS_KEY";
            private const string _labPhoneKey = "#LAB_PHONE_KEY";
            private const string _labSiteKey = "#LAB_SITE_KEY";


            internal static async Task<string> PrepareMessage(EmailTemplateLoader.NotificationType notificationType, string customerName, DateTime scheduledDate, LabConfigAddresses labConfigs)
            {
                var htmlTemplate = await EmailTemplateLoader.GetEmailTemplateHtml(notificationType);

                return htmlTemplate.Replace(_customerNameKey, customerName)
                    .Replace(_scheduledDateKey, $"{scheduledDate.ToString(DATE_TIME_FORMAT)}")
                    .Replace(_labNameKey, labConfigs.LabName)
                    .Replace(_labAddressKey, labConfigs.LabAddress)
                    .Replace(_labPhoneKey, labConfigs.PhoneNumber)
                    .Replace(_labSiteKey, labConfigs.WebSiteAddress);
            }
        }

        static class EmailTemplateLoader
        {
            internal enum NotificationType
            {
                OrderCreated,
                OrderCompleted
            }

            internal static async Task<string> GetEmailTemplateHtml(NotificationType notificationType)
            {
                var templateName = string.Empty;
                switch (notificationType)
                {
                    case NotificationType.OrderCreated:
                        templateName = "OrderCreated";
                        break;
                    case NotificationType.OrderCompleted:
                        templateName = "OrderCompleted";
                        break;
                }

                string path = Path.Combine(Directory.GetCurrentDirectory(), "assets", "EmailTemplates", $"{templateName}.html");
                using var streamReader = new StreamReader(path, Encoding.UTF8);
                return await streamReader.ReadToEndAsync();
            }
        }
    }

}
