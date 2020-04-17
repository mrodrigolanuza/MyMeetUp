using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using SendGrid;
using SendGrid.Helpers.Mail;
using System;

namespace MyMeetUp.FunctionApp.QueueEventReceiver
{
    public static class QueueMessageReceiver
    {
        [FunctionName("QueueMessageReceiver")]
        public static void Run([QueueTrigger("mymeetupeventsqueue", Connection = "AzureWebJobsStorage")]string myQueueItem, ILogger log) {
            log.LogInformation($"C# Queue trigger function processed: {myQueueItem} >>> Sending SendGridMail to marcosrlanuza@hotmail.com");
            SendNewsletterToUsers(myQueueItem);
        }

        private static async void SendNewsletterToUsers(string message) {
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
