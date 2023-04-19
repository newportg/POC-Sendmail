using Azure;
using Azure.Communication.Email;
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
            _logger = loggerFactory.CreateLogger("Sendmail");
        }

        [Function("negotiate")]
        [OpenApiOperation(operationId: "negotiate")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "text/plain", bodyType: typeof(SignalRConnectionInfo), Description = "The OK response")]
        [OpenApiResponseWithoutBody(statusCode: HttpStatusCode.InternalServerError, Description = "Configuration / Database issue")]
        public SignalRConnectionInfo Negotiate(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post")] HttpRequestData req,
            [SignalRConnectionInfoInput(HubName = "HubValue", ConnectionStringSetting = "SignalRCS")] SignalRConnectionInfo connectionInfo)
        {
            _logger.LogInformation($"SignalR Connection URL = '{connectionInfo.Url}'");
            return connectionInfo;
        }

        [Function("BroadcastToAll")]
        [OpenApiOperation(operationId: "BroadcastToAll")]
        [OpenApiRequestBody(contentType: "text/plain", bodyType: typeof(string), Description = "message", Example = typeof(string))]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "text/plain", bodyType: typeof(SignalRMessageAction), Description = "The OK response")]
        [OpenApiResponseWithoutBody(statusCode: HttpStatusCode.InternalServerError, Description = "Configuration / Database issue")]
        [SignalROutput(HubName = "HubValue", ConnectionStringSetting = "SignalRCS")]
        public SignalRMessageAction BroadcastToAll([HttpTrigger(AuthorizationLevel.Anonymous, "post")] HttpRequestData req)
        {
            using var bodyReader = new StreamReader(req.Body);

            _logger.LogInformation($"SignalR BroadcastToAlL");

            return new SignalRMessageAction("newEvent")
            {
                // broadcast to all the connected clients without specifying any connection, user or group.
                Arguments = new[] { bodyReader.ReadToEnd() },
            };

        }

        [Function("SendMail")]
        [OpenApiOperation(operationId: "SendMail")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "text/plain", bodyType: typeof(HttpResponseData), Description = "The OK response")]
        [OpenApiResponseWithoutBody(statusCode: HttpStatusCode.InternalServerError, Description = "Configuration / Database issue")]
        public HttpResponseData Run([HttpTrigger(AuthorizationLevel.Anonymous, "get")] HttpRequestData req)
        {
            _logger.LogInformation("C# HTTP trigger function processed a request.");

            var response = req.CreateResponse(HttpStatusCode.OK);
            response.Headers.Add("Content-Type", "text/plain; charset=utf-8");

            // Find your Communication Services resource in the Azure portal
            var connectionString = Environment.GetEnvironmentVariable("CommunicationServicesCS");
            EmailClient emailClient = new EmailClient(connectionString);

            // ID Token
            //string endpoint = "https://cs-poc-sendmail-vse-ne.communication.azure.com";
            //TokenCredential tokenCredential = new DefaultAzureCredential();
            //tokenCredential = new DefaultAzureCredential();
            //EmailClient emailClient = new EmailClient(new Uri(endpoint), tokenCredential);

            try
            {
                var emailSendOperation = emailClient.Send(
                    wait: WaitUntil.Completed,
                    senderAddress: Environment.GetEnvironmentVariable("FromEmailAddress"), // The email address of the domain registered with the Communication Services resource
                    recipientAddress: "gary.newport@zoomalong.co.uk",

                    subject: "This is the subject",
                    htmlContent: "<html><body>This is the html body</body></html>");
                Console.WriteLine($"Email Sent. Status = {emailSendOperation.Value.Status}");

                /// Get the OperationId so that it can be used for tracking the message for troubleshooting
                string operationId = emailSendOperation.Id;
                Console.WriteLine($"Email operation id = {operationId}");
                response.WriteString($"Email operation id = {operationId}");
            }
            catch (RequestFailedException ex)
            {
                /// OperationID is contained in the exception message and can be used for troubleshooting purposes
                Console.WriteLine($"Email send operation failed with error code: {ex.ErrorCode}, message: {ex.Message}");
                response.WriteString($"Email send operation failed with error code: {ex.ErrorCode}, message: {ex.Message}");
            }

            return response;
        }

        //[Function("EventHubTriggerCSharp")]
        //[SignalROutput(HubName = "HubValue", ConnectionStringSetting = "SignalRCS")]
        //public SignalRMessageAction evhtrigger([EventHubTrigger("evh-poc-sendmail-vse-ne", Connection = "EventHubCS")] string[] input,
        //     FunctionContext context)
        //{
        //    _logger.LogInformation($"First Event Hubs triggered message: {input[0]}");

        //    return new SignalRMessageAction("newEvent")
        //    {
        //        // broadcast to all the connected clients without specifying any connection, user or group.
        //        Arguments = new[] { input[0] },
        //    };
        //}

    }
}

