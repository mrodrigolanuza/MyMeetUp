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
        private ILogger _log;
        private string SendGridMailAccount;
        private string MockMailAccountTo;
        private const string COORDINATOR = "COORDINATOR";
        private const string MEMBER = "MEMBER";

        private enum GROUP_CREATED_EVENT_ARGS : int { 
            GROUPID = 0,
            GROUP_DETAIL_URI = 1,
            POS_INI_GROUP_CATEGORIES_LIST = 2
        }

        private enum EVENT_CREATED_ARGS : int { 
            EVENTID = 0,
            EVENT_DETAIL_URI = 1
        }

        private enum NEW_GROUP_MEMBER : int {
            GROUPID = 0,
            USERID = 1
        }

        public EventQueueProcessingFunction(ApplicationDbContext context) {
            _context = context;
            SendGridMailAccount = Environment.GetEnvironmentVariable("SendGridMailAccount");
            MockMailAccountTo = Environment.GetEnvironmentVariable("MockMailAccountTo"); 
        }

        [FunctionName("EventQueueProcessingFunction")]
        public void Run([QueueTrigger("mymeetupeventsqueue", Connection = "AzureWebJobsStorage")]string myQueueItem, ILogger log) {
            _log = log;
            try {
                AnalyzeEventReceived(myQueueItem);
                PerformAssociatedEventActions();
                LogEventAsProcessedOK(myQueueItem);
            } catch (Exception ex) {
                LogEventAsProcessedWithException(myQueueItem, ex);
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
                    PerformEventCreationAssociatedActions();
                    break;
                case EventQueueMessages.NEW_GROUP_MEMBER:
                    PerformNewGroupMemberAssociatedActions();
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
            SendGridClient client = GetSendGridClient();
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
                    From = new EmailAddress(SendGridMailAccount, "MyMeetUp Team"),
                    Subject = "MyMeetUp: Nuevo Grupo interesante!",
                    HtmlContent = NotificationEmailContent.ToString()
                };
                msg.AddTo(new EmailAddress(MockMailAccountTo));
                var response = await client.SendEmailAsync(msg);
                _log.LogWarning($"SendGrid response: {response.StatusCode}. Email sent to {user.Name} {user.Surname} related to new group created: {newGroup.Name}");
                NotificationEmailContent.Clear();
            }
        }

        private async void PerformEventCreationAssociatedActions() {
            int newEventId = int.Parse(eventArgs[(int)EVENT_CREATED_ARGS.EVENTID]);
            string eventDetailsUri = eventArgs[(int)EVENT_CREATED_ARGS.EVENT_DETAIL_URI];

            //Event info
            Event newEvent = _context.Events
                                  .Where(e => e.Id == newEventId)
                                  .FirstOrDefault();

            //Group associated
            Group group = _context.Events
                                  .Include(e => e.Group)
                                  .Where(e => e.Id == newEventId)
                                  .Select(e => e.Group)
                                  .FirstOrDefault();
            
            //List of group members
            List<ApplicationUser> groupMembers = _context.GroupMembers
                                            .Include(gm => gm.ApplicationUser)
                                            .Include(gm => gm.GroupMemberProfile)
                                            .Where(gm => gm.GroupId == group.Id && gm.GroupMemberProfile.Name == MEMBER)
                                            .Select(gm => gm.ApplicationUser)
                                            .ToList();

            //Send personalized email to every single user with the new group info
            SendGridClient client = GetSendGridClient();
            StringBuilder NotificationEmailContent = new StringBuilder();
            foreach (ApplicationUser user in groupMembers) {
                NotificationEmailContent.Append($"Hola {user.Name}!");
                NotificationEmailContent.Append("</br>");
                NotificationEmailContent.Append("</br>");
                NotificationEmailContent.Append("<p>Desde el equipo de MyMeetUp queremos comunicarte que estás de enhorabuena!</br>");
                NotificationEmailContent.Append($"Acaba de crearse recientemente un nuevo evento organizado por el grupo <b>{group.Name}</b>.</p>");
                NotificationEmailContent.Append("<p>Esto son los datos del evento:");
                NotificationEmailContent.Append("<ul>");
                NotificationEmailContent.Append($"<li>Título: <b><i>{newEvent.Title}</i></b></li>");
                NotificationEmailContent.Append($"<li>Dónde: <i>{newEvent.Address} - {newEvent.City} - {newEvent.Country}</i></li>");
                NotificationEmailContent.Append($"<li>Cuándo: <i>{newEvent.FechaHora}</i></li>");
                NotificationEmailContent.Append("</ul></p>");
                NotificationEmailContent.Append($"<p><u>Descripción</u><br><i>{newEvent.Description}</i></li></p>");
                NotificationEmailContent.Append($"<p>Si quieres acceder al evento directamente, puedes utilizar el siguiente enlace: <a href=\"{eventDetailsUri}\" > Quiero saber más sobre este evento!</a></p>");
                NotificationEmailContent.Append("<p>Un saludo,</br>");
                NotificationEmailContent.Append("Equipo MyMeetUp</p>");

                var msg = new SendGridMessage()
                {
                    From = new EmailAddress(SendGridMailAccount, "MyMeetUp Team"),
                    Subject = "MyMeetUp: Atención Nuevo Evento!!",
                    HtmlContent = NotificationEmailContent.ToString()
                };
                msg.AddTo(new EmailAddress(MockMailAccountTo));
                var response = await client.SendEmailAsync(msg);
                _log.LogWarning($"SendGrid response: {response.StatusCode}. Email sent to {user.Name} {user.Surname} related to new event created: {newEvent.Title}");
                NotificationEmailContent.Clear();
            }
        }

        private async void PerformNewGroupMemberAssociatedActions() {
            int groupId = int.Parse(eventArgs[(int)NEW_GROUP_MEMBER.GROUPID]);
            string newUserId = eventArgs[(int)NEW_GROUP_MEMBER.USERID];

            //Get group info
            Group group = _context.Groups.Where(g => g.Id == groupId).FirstOrDefault();
            
            //Get group coordinators
            List<ApplicationUser> groupCoordinators = _context.GroupMembers
                                                            .Include(gm => gm.GroupMemberProfile)
                                                            .Include(gm => gm.ApplicationUser)
                                                            .Where(gm => gm.GroupId == groupId && gm.GroupMemberProfile.Name == COORDINATOR)
                                                            .Select(gm => gm.ApplicationUser)
                                                            .ToList();

            //Get new member info
            ApplicationUser newMemberInfo = _context.GroupMembers
                                                    .Include(gm => gm.ApplicationUser)
                                                    .Where(gm => gm.ApplicationUserId == newUserId)
                                                    .Select(gm => gm.ApplicationUser)
                                                    .FirstOrDefault();
            //Send email to group coordinators
            SendGridClient client = GetSendGridClient();
            StringBuilder NotificationEmailContent = new StringBuilder();
            foreach (ApplicationUser coordinator in groupCoordinators) {
                NotificationEmailContent.Append($"Hola {coordinator.Name},");
                NotificationEmailContent.Append("</br>");
                NotificationEmailContent.Append("</br>");
                NotificationEmailContent.Append($"<p>Desde el equipo de MyMeetUp queremos informarle como coordinador de grupo <b>{group.Name}</b>,</br>");
                NotificationEmailContent.Append($"que acaba de darse de alta un nuevo miembro. Enhorabuena!.</p>");
                NotificationEmailContent.Append("<p><ul>");
                NotificationEmailContent.Append($"<li><b><i>{newMemberInfo.Name} {newMemberInfo.Surname}</i></b></li>");
                NotificationEmailContent.Append($"<li><i>{newMemberInfo.Email}</i></li>");
                NotificationEmailContent.Append($"<li><i>{newMemberInfo.City} - {newMemberInfo.Country}</i></li>");
                NotificationEmailContent.Append("</ul></p>");
                NotificationEmailContent.Append("<p>Un saludo,</br>");
                NotificationEmailContent.Append("Equipo MyMeetUp</p>");

                var msg = new SendGridMessage()
                {
                    From = new EmailAddress(SendGridMailAccount, "MyMeetUp Team"),
                    Subject = "MyMeetUp: Nuevo Miembro en tu Grupo!!",
                    HtmlContent = NotificationEmailContent.ToString()
                };
                msg.AddTo(new EmailAddress(MockMailAccountTo));
                var response = await client.SendEmailAsync(msg);
                _log.LogWarning($"SendGrid response: {response.StatusCode}. Email sent to {coordinator.Name} {coordinator.Surname} related to there is a new member in the group: {newMemberInfo.Name} {newMemberInfo.Surname}");
                NotificationEmailContent.Clear();
            }
        }


        private SendGridClient GetSendGridClient() {
            string apiKey = Environment.GetEnvironmentVariable("MyMeetupSendgridApiKey");
            var client = new SendGridClient(apiKey);
            return client;
        }

        private void LogEventAsProcessedOK(string myQueueItem) {
            _log.LogWarning($"C# Queue trigger function processed: {myQueueItem} >>> Sending SendGridMail to marcosrlanuza@hotmail.com");
        }

        private void LogEventAsProcessedWithException(string myQueueItem, Exception ex) {
            _log.LogError($"EXCEPTION >> EventQueueProcessingFunction >> Queue message received: '{myQueueItem}' >>> Exception Message: {ex.Message}");
        }
    }
}
