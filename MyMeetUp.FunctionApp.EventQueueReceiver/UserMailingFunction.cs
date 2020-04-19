using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MyMeetUp.FunctionApp.EventQueueReceiver.Data;
using MyMeetUp.Web.Models;
using SendGrid;
using SendGrid.Helpers.Mail;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyMeetUp.FunctionApp.EventQueueReceiver
{
    public class UserMailingFunction
    {
        private const string Route = "usermailing";
        private readonly ApplicationDbContext _context;
        private string emailAddressFrom;
        private string emailAddressTo; 
        private string apiKey;

        public UserMailingFunction(ApplicationDbContext context, UserManager<ApplicationUser> userManager) {
            _context = context;
        }

        /// <summary>
        /// Mailing Function. Perform a massive notification to users that have upcoming new events within the next days.
        /// </summary>
        [FunctionName("UserMailingFunction")]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = Route)] HttpRequest req,
            ILogger log) {

            try {
                GetEnvironmentVariables();
                SendEventRemindersForThisWeek(log);
                return new OkResult();
            } catch {
                return new StatusCodeResult(400);
            }
        }
        private void GetEnvironmentVariables() {
            apiKey = Environment.GetEnvironmentVariable("MyMeetupSendgridApiKey");
            emailAddressFrom = Environment.GetEnvironmentVariable("SendGridMailAccount");
            emailAddressTo = Environment.GetEnvironmentVariable("MockMailAccountTo");
            
        }

        private  void SendEventRemindersForThisWeek(ILogger log) {
            //Eventos de la semana
            var dateIni = DateTime.Now;
            var dateEnd = DateTime.Now + new TimeSpan(7, 0, 0, 0);
            List<Event> eventsThisWeek =  _context.Events
                                                    .Where(e => e.FechaHora >= dateIni && e.FechaHora <= dateEnd).
                                                    ToList();
            
            log.LogWarning($"{eventsThisWeek.Count} Events for this week");
            if (eventsThisWeek == null || eventsThisWeek.Count == 0)
                return;

            //Miembros de cada evento
            foreach (Event upcomingEvent in eventsThisWeek) {
                List<ApplicationUser> membersSayWillAttendToEvent = _context.EventAttendances
                                                                        .Include(ea => ea.ApplicationUser)
                                                                        .Include(ea => ea.EventAttendanceState)
                                                                        .Where(ea => ea.EventId == upcomingEvent.Id && ea.EventAttendanceState.State == "I WILL ATTEND")
                                                                        .Select(ea => ea.ApplicationUser)
                                                                        .ToList();

                log.LogWarning($"{membersSayWillAttendToEvent.Count} user members will attend event {upcomingEvent.Title}");
                if (membersSayWillAttendToEvent == null || membersSayWillAttendToEvent.Count == 0) 
                    continue;
                    
                
                Group groupInCharge = _context.Events
                                                .Include(e => e.Group)
                                                .Where(e => e.Id == upcomingEvent.Id)
                                                .Select(e => e.Group)
                                                .FirstOrDefault();

                foreach (ApplicationUser possibleAttendee in membersSayWillAttendToEvent) {
                   SendNewsletterToUser(possibleAttendee, upcomingEvent, groupInCharge, log).Wait();
                }
            }
            log.LogWarning("Mailing process finished.");
        }

        private async Task SendNewsletterToUser(ApplicationUser user, Event upcomingEvent, Group group, ILogger log) {
            var client = new SendGridClient(apiKey);
            StringBuilder NotificationEmailContent = new StringBuilder();
            NotificationEmailContent.Append($"Hola {user.Name}!");
            NotificationEmailContent.Append("</br>");
            NotificationEmailContent.Append("</br>");
            NotificationEmailContent.Append($"<p>Nos ponemos en contacto contigo para recordarte que el próximo día <b>{upcomingEvent.FechaHora}</b> tienes un evento.</p>");
            NotificationEmailContent.Append($"<p>Recuerda que el evento lo organiza el grupo <b>{group.Name}</b> y tiene por título <b>{upcomingEvent.Title}</b>.</p>");
            NotificationEmailContent.Append($"<p>Tendrá lugar en <b>{upcomingEvent.Address}, {upcomingEvent.City} ({upcomingEvent.Country})</b>.</p>");
            NotificationEmailContent.Append("<p>Te esperamos!  </br>");
            NotificationEmailContent.Append("Equipo MyMeetUp</p>");



            var msg = new SendGridMessage()
            {
                From = new EmailAddress(emailAddressFrom, "MyMeetUp Team"),
                Subject = "MyMeetUp: Recordatorio, tienes un evento próximo!!",
                HtmlContent = NotificationEmailContent.ToString()
            };
            msg.AddTo(new EmailAddress(emailAddressTo));
            log.LogWarning($"Ready for sending the SendGrid email: From:{msg.From.Email} To:{emailAddressTo} Subject:{msg.Subject}");
            var response = await client.SendEmailAsync(msg);
            log.LogWarning($"SendGrid response: {response.StatusCode}. Email sent to {user.Name} {user.Surname} related to upcoming event: {upcomingEvent.Title}");
        }

    }
}
