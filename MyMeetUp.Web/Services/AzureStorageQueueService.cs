using MyMeetUp.Web.Services.Interfaces;
using System;
using Microsoft.Azure.Storage;          // Namespace for CloudStorageAccount
using Microsoft.Azure.Storage.Queue;    // Namespace for Queue storage types
using Microsoft.Extensions.Options;
using MyMeetUp.Web.Configuration;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace MyMeetUp.Web.Services
{
    public class AzureStorageQueueService : IQueueService
    {
        private readonly ILogger<AzureStorageQueueService> _logger;
        private readonly IOptions<EventQueueSettings> _eventQueueSettings;
        private CloudStorageAccount storageAccount;
        private CloudQueueClient queueClient;
        private CloudQueue queue;

        public AzureStorageQueueService() { }
        public AzureStorageQueueService(ILogger<AzureStorageQueueService> logger, IOptions<EventQueueSettings> eventQueueSettings) {
            _logger = logger;
            _eventQueueSettings = eventQueueSettings;
        }

        public async Task<bool> SendMessage(string message) {
            // Create the queue if it doesn't already exist
            if (await InitializeConnectionWithAzureStorageQueueAsync()) {
                CloudQueueMessage cloudQueueMessage = new CloudQueueMessage(message);
                await queue.AddMessageAsync(cloudQueueMessage);
                _logger.LogInformation($"Queued message: {message}");
                return true;    
            }
            return false;
        }

        private async Task<bool> InitializeConnectionWithAzureStorageQueueAsync() {
            try {
                //Storage Account
                string storageAccountConnectionString = $"DefaultEndpointsProtocol=https;AccountName={_eventQueueSettings.Value.StorageAccount};AccountKey={_eventQueueSettings.Value.StorageKey}";
                storageAccount = CloudStorageAccount.Parse(storageAccountConnectionString);
                //Queue Client
                queueClient = storageAccount.CreateCloudQueueClient();
                //Queue Reference
                queue = queueClient.GetQueueReference(_eventQueueSettings.Value.QueueName);
                await queue.CreateIfNotExistsAsync();
                return true;
            } catch (Exception ex) {
                _logger.LogError($"EXCEPCION: {ex.Message}");
                return false;
            }
        }
    }
}
