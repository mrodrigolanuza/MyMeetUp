using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using SendGrid;
using SendGrid.Helpers.Mail;
using System;
using MyMeetUp.FunctionApp.QueueEventReceiver.Data;

namespace MyMeetUp.FunctionApp.QueueEventReceiver
{
    public class QueueMessageReceiver
    {
        private string[] eventInformation;
        private string eventType;

        [FunctionName("QueueMessageReceiver")]
        public void Run([QueueTrigger("mymeetupeventsqueue", Connection = "AzureWebJobsStorage")]string myQueueItem, ILogger log) {
            try {
                AnalyzeEventReceived(myQueueItem);
                PerformAssociatedEventActions();
                LogEventAsProcessedOK(myQueueItem, log);
            } catch (Exception ex) {
                LogEventAsProcessedWithException(myQueueItem, log, ex);
                SendNewsletterToUsers($"Event >> {eventType} not recognized!!");
            }

        }

        private void AnalyzeEventReceived(string myQueueItem) {
            if (myQueueItem.Contains(":")) {
                eventInformation = myQueueItem.Split(new char[] { ':' });
                eventType = eventInformation[0];
            }
        }
        private void PerformAssociatedEventActions() {
            switch (eventType) {
                case EventQueueMessages.GROUP_CREATED:
                    SendNewsletterToUsers($"Event >> {eventType} recived!!");
                    break;
                case EventQueueMessages.EVENT_CREATED:
                    SendNewsletterToUsers($"Event >> {eventType} recived!!");
                    break;
                case EventQueueMessages.NEW_GROUP_MEMBER:
                    SendNewsletterToUsers($"Event >> {eventType} recived!!");
                    break;
                default:
                    throw new Exception("Queue message not recognized.");
            }
        }

        private void LogEventAsProcessedOK(string myQueueItem, ILogger log) {
            log.LogInformation($"C# Queue trigger function processed: {myQueueItem} >>> Sending SendGridMail to marcosrlanuza@hotmail.com");
        }

        private void LogEventAsProcessedWithException(string myQueueItem, ILogger log, Exception ex) {
            log.LogError($"EXCEPTION >> QueueMessageReceiver >> Queue message received: '{myQueueItem}' >>> Exception Message: {ex.Message}");
        }

        private async void SendNewsletterToUsers(string message) {
            string apiKey = Environment.GetEnvironmentVariable("MyMeetupSendgridApiKey");
            var client = new SendGridClient(apiKey);
            var msg = new SendGridMessage()
            {
                From = new EmailAddress("newsletter@fpplusplustechcommunity.com", "FP++ TechCommunity Team"),
                Subject = message,
                HtmlContent = "Enviado Sendgrid Mail mediante envento en Queue"
            };
            msg.AddTo(new EmailAddress("marcosrlanuza@hotmail.com"));
            var response = await client.SendEmailAsync(msg);
        }
    }
}
