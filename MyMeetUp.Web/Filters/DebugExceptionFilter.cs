using Microsoft.ApplicationInsights;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace MyMeetUp.Web.Filters
{
    public class DebugExceptionFilter : IExceptionFilter
    {
        private readonly ILogger _logger;
        TelemetryClient _telemetryClient;
        public DebugExceptionFilter(TelemetryClient telemetryClient, ILogger<DebugExceptionFilter> logger) {
            _logger = logger;
            _telemetryClient = telemetryClient;
        }
        public void OnException(ExceptionContext context) {
           _logger.LogError($"Exception thrown in a Controller: {context.Exception.Message}");
            _telemetryClient.TrackException(context.Exception);
        }
    }
}
