using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using System.Net;

namespace Sendmail
{
    public class Sendmail
    {
        private readonly ILogger _logger;

        public Sendmail(ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.CreateLogger("negotiate");
        }

        [Function("negotiate")]
        [OpenApiOperation(operationId: "negotiate")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "text/plain", bodyType: typeof(SignalRConnectionInfo), Description = "The OK response")]
        public SignalRConnectionInfo Negotiate(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post")] HttpRequestData req,
            [SignalRConnectionInfoInput(HubName = "HubValue", ConnectionStringSetting = "AzureSignalRConnectionString")] SignalRConnectionInfo connectionInfo)
        {
            _logger.LogInformation($"SignalR Connection URL = '{connectionInfo.Url}'");

            var response = req.CreateResponse(HttpStatusCode.OK);
            response.Headers.Add("Content-Type", "text/plain; charset=utf-8");
            response.WriteString($"Connection URL = '{connectionInfo.Url}'");

            return connectionInfo;
        }

        [Function("BroadcastToAll")]
        [OpenApiOperation(operationId: "BroadcastToAll")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "text/plain", bodyType: typeof(SignalRMessageAction), Description = "The OK response")]
        [OpenApiRequestBody(contentType: "text/plain", bodyType: typeof(string), Description = "message", Example = typeof(string))]
        [SignalROutput(HubName = "HubValue", ConnectionStringSetting = "AzureSignalRConnectionString")]
        public static SignalRMessageAction BroadcastToAll([HttpTrigger(AuthorizationLevel.Anonymous, "post")] HttpRequestData req)
        {
            using var bodyReader = new StreamReader(req.Body);
            return new SignalRMessageAction("newMessage")
            {
                // broadcast to all the connected clients without specifying any connection, user or group.
                Arguments = new[] { bodyReader.ReadToEnd() },
            };
        }
    }

    public class MyConnectionInfo
    {
        public string? Url { get; set; }

        public string? AccessToken { get; set; }
    }
}
