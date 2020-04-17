using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;

namespace MyMeetUp.FunctionApp.QueueEventReceiver
{
    public static class QueueMessageReceiver
    {
        [FunctionName("QueueMessageReceiver")]
        public static void Run([QueueTrigger("mymeetupeventsqueue", Connection = "AzureWebJobsStorage")]string myQueueItem, ILogger log) {
            log.LogInformation($"C# Queue trigger function processed: {myQueueItem}");
        }
    }
}
