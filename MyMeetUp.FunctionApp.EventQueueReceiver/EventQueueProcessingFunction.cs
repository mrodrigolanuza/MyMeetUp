using Microsoft.Azure.WebJobs;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MyMeetUp.FunctionApp.EventQueueReceiver.Data;
using MyMeetUp.FunctionApp.EventQueueReceiver.Utils;
using MyMeetUp.Web.Models;
using SendGrid;
using SendGrid.Helpers.Mail;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MyMeetUp.FunctionApp.EventQueueReceiver
{
    public class EventQueueProcessingFunction
    {
        private readonly ApplicationDbContext _context;
        private List<string> eventArgs;
        private string eventType;

        private enum GROUP_CREATED_EVENT_ARGS : int { 
            GROUPID = 0,
            GROUP_DETAIL_URI = 1,
            POS_INI_GROUP_CATEGORIES_LIST = 2
        }

        public EventQueueProcessingFunction(ApplicationDbContext context) {
            _context = context;
        }

        [FunctionName("EventQueueProcessingFunction")]
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
            if (myQueueItem.Contains(";")) {
                List<string> eventInformation = myQueueItem.Split(new char[] { ';' }).ToList();
                eventType = eventInformation.First();
                if (eventArgs is null) 
                    eventArgs = new List<string>();
                for (int index = 1; index < eventInformation.Count; index++) {
                    eventArgs.Add(eventInformation[index]);
                }
            }
        }

        private void PerformAssociatedEventActions() {
            switch (eventType) {
                case EventQueueMessages.GROUP_CREATED:
                    PerformGroupCreationAssociatedActions();
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

        private async void PerformGroupCreationAssociatedActions() {
            int newGroupId = int.Parse(eventArgs[(int)GROUP_CREATED_EVENT_ARGS.GROUPID]);
            string groupDetailsUri = eventArgs[(int)GROUP_CREATED_EVENT_ARGS.GROUP_DETAIL_URI];
            List<int> groupCategoriesIds = eventArgs.GetRange((int)GROUP_CREATED_EVENT_ARGS.POS_INI_GROUP_CATEGORIES_LIST,
                                                              eventArgs.Count -(int)GROUP_CREATED_EVENT_ARGS.POS_INI_GROUP_CATEGORIES_LIST)
                                                    .Select(int.Parse).ToList();

            //New Group Info
            Group newGroup = _context.Groups.Where(g => g.Id == newGroupId).FirstOrDefault();

            //List of groups with same categories
            List<int> similarGroups =  _context.Group_GroupCategories
                                            .Where(ggc => groupCategoriesIds.Contains(ggc.GroupCategoryId) && ggc.GroupId!= newGroupId)
                                            .Select(ggc=>ggc.GroupId)
                                            .Distinct()
                                            .ToList();

            //Users in this list of groups
            List<ApplicationUser> similarGroupsUsers = _context.GroupMembers
                                            .Include(gm => gm.ApplicationUser)
                                            .Where(gm => similarGroups.Contains(gm.GroupId))
                                            .Select(gm => gm.ApplicationUser)
                                            .Distinct()
                                            .ToList();

            //Send personalized email to every single user with the new group info
            string apiKey = Environment.GetEnvironmentVariable("MyMeetupSendgridApiKey");
            var client = new SendGridClient(apiKey);
            StringBuilder NotificationEmailContent = new StringBuilder();
            foreach (ApplicationUser user in similarGroupsUsers) {
                NotificationEmailContent.Append($"Hola {user.Name}!");
                NotificationEmailContent.Append("</br>");
                NotificationEmailContent.Append("</br>");
                NotificationEmailContent.Append("<p>Se acaba e crear un nuevo grupo afín a tus intereses, que creemos que te podría interesar.</br>");
                NotificationEmailContent.Append($"Su nombre es <b>{newGroup.Name}</b> y su sede está en <b>{newGroup.City}</b>, <b>{newGroup.Country}</b>.</p>");
                NotificationEmailContent.Append("<p>Sobre este grupo y sus intereses, podemos contarte lo siguiente:</p>");
                NotificationEmailContent.Append($"<p><i>{newGroup.AboutUs}</i></p>");
                NotificationEmailContent.Append($"<p>Si quieres unirte al grupo, pulsa el siguiente enlace: <a href=\"{groupDetailsUri}\" > Quiero saber más de: {newGroup.Name}</a></p>");
                NotificationEmailContent.Append("<p>Un saludo,</br>");
                NotificationEmailContent.Append("Equipo MyMeetUp</p>");

                var msg = new SendGridMessage()
                {
                    From = new EmailAddress("newsletter@mymeetup.com", "MyMeetUp Team"),
                    Subject = "MyMeetUp: Nuevo Grupo interesante!",
                    HtmlContent = NotificationEmailContent.ToString()
                };
                msg.AddTo(new EmailAddress("marcosrlanuza@hotmail.com"));
                var response = await client.SendEmailAsync(msg);
                NotificationEmailContent.Clear();
            }
        }

        private void LogEventAsProcessedOK(string myQueueItem, ILogger log) {
            log.LogInformation($"C# Queue trigger function processed: {myQueueItem} >>> Sending SendGridMail to marcosrlanuza@hotmail.com");
        }

        private void LogEventAsProcessedWithException(string myQueueItem, ILogger log, Exception ex) {
            log.LogError($"EXCEPTION >> EventQueueProcessingFunction >> Queue message received: '{myQueueItem}' >>> Exception Message: {ex.Message}");
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
