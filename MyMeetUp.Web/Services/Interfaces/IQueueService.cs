using System.Threading.Tasks;

namespace MyMeetUp.Web.Services.Interfaces
{
    public interface IQueueService
    {
        public Task<bool> SendMessage(string message);
    }
}
