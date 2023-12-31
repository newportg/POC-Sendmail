using Azure;
using Azure.Communication.Email;
using Azure.Identity;
using Azure.Messaging.EventGrid;
using Azure.Security.KeyVault.Secrets;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using System.Net;

namespace Sendmail
{
    public class MyEventType
    {
        public string? Id { get; set; }

        public string? Topic { get; set; }

        public string? Subject { get; set; }

        public string? EventType { get; set; }

        public DateTime? EventTime { get; set; }

        public IDictionary<string, object>? Data { get; set; }
    }

    public class Sendmail
    {
        private readonly ILogger _logger;

        public Sendmail(ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.CreateLogger("Sendmail");
        }

        [Function("negotiate")]
        [OpenApiOperation(operationId: "negotiate")]
        [OpenApiResponseWithoutBody(statusCode: HttpStatusCode.InternalServerError, Description = "Configuration issue")]
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
        [OpenApiResponseWithoutBody(statusCode: HttpStatusCode.InternalServerError, Description = "Configuration issue")]
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

        [Function("MailEventGridSubscription")]
        [SignalROutput(HubName = "HubValue", ConnectionStringSetting = "SignalRCS")]
        public SignalRMessageAction MailEventGridSubscription([EventGridTrigger] MyEventType input)
        {
            _logger.LogInformation(input.Data.ToString());
            var data = Newtonsoft.Json.JsonConvert.SerializeObject(input);
            _logger.LogInformation(data);

            return new SignalRMessageAction("newGridEvent")
            {
                // broadcast to all the connected clients without specifying any connection, user or group.
                Arguments = new[] { data },
            };
        }

        [Function("MailEventHubSubscription")]
        [SignalROutput(HubName = "HubValue", ConnectionStringSetting = "SignalRCS")]
        public SignalRMessageAction MailEventHubSubscription([EventHubTrigger("%EventHubName%", Connection = "ServiceBusConnection")] string[] input)
        {
            _logger.LogInformation($"First Event Hubs triggered message: {input[0]}");

            return new SignalRMessageAction("newHubEvent")
            {
                // broadcast to all the connected clients without specifying any connection, user or group.
                Arguments = new[] { input[0] },
            };
        }

        [Function("SendMail")]
        [OpenApiOperation(operationId: "SendMail")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "text/plain", bodyType: typeof(string), Description = "The OK response")]
        [OpenApiResponseWithoutBody(statusCode: HttpStatusCode.InternalServerError, Description = "Configuration / Database issue")]

        public HttpResponseData Run([HttpTrigger(AuthorizationLevel.Anonymous, "get")] HttpRequestData req)
        {
            _logger.LogInformation("C# HTTP trigger function processed a request.");

            var cred = new DefaultAzureCredential(new DefaultAzureCredentialOptions { ManagedIdentityClientId = Environment.GetEnvironmentVariable("ServiceBusConnection__clientId") });
            var client = new SecretClient(new Uri("https://kv-poc-sendmail-vse-ne.vault.azure.net/"), cred);
            var val = client.GetSecret("ServiceBusConnectionTenantId");     
            _logger.LogInformation($"Secret client : {val.Value.Value}");



            var response = req.CreateResponse(HttpStatusCode.OK);
            response.Headers.Add("Content-Type", "text/plain; charset=utf-8");

            // Find your Communication Services resource in the Azure portal
            //var connectionString = Environment.GetEnvironmentVariable("CommunicationServicesCS");
            //EmailClient emailClient = new EmailClient(connectionString);

            // ID Token
            //string endpoint = "https://cs-poc-sendmail-vse-ne.communication.azure.com";
            //TokenCredential tokenCredential = new DefaultAzureCredential();
            //tokenCredential = new DefaultAzureCredential();
            //EmailClient emailClient = new EmailClient(new Uri(endpoint), tokenCredential);


            //var credential = new DefaultAzureCredential(new DefaultAzureCredentialOptions { ManagedIdentityClientId = Environment.GetEnvironmentVariable("ServiceBusConnection__clientId")  });
            //string? endpoint = Environment.GetEnvironmentVariable("CommunicationServicesEP");
//            string? endpoint = Environment.GetEnvironmentVariable("CommunicationServicesEP");
            //if (endpoint == null)
            //{
            //    response.WriteString($"ACS endpoint is null");
            //    return response;
            //}
            var emailClient = new EmailClient(new Uri("https://cs-poc-sendmail-vse-ne.communication.azure.com/"), cred);
            _logger.LogInformation($"Email Client ");


            try
            {
                var emailSendOperation = emailClient.Send(
                    wait: WaitUntil.Completed,
                    senderAddress: Environment.GetEnvironmentVariable("FromEmailAddress"), // The email address of the domain registered with the Communication Services resource
                    recipientAddress: "gary.newport@zoomalong.co.uk",

                    subject: "This is the subject",
                    htmlContent: "<html><body>This is the html body <a href=\"http://maps.google.com\">Google Maps</a> <a href=\"https://www.w3schools.com\">Visit W3Schools.com!</a></body></html>");
                Console.WriteLine($"Email Sent. Status = {emailSendOperation.Value.Status}");

                /// Get the OperationId so that it can be used for tracking the message for troubleshooting
                string operationId = emailSendOperation.Id;
                Console.WriteLine($"Email operation id = {operationId}");
                response.WriteString($"{operationId}");
            }
            catch (RequestFailedException ex)
            {
                /// OperationID is contained in the exception message and can be used for troubleshooting purposes
                Console.WriteLine($"Email send operation failed with error code: {ex.ErrorCode}, message: {ex.Message}");
                response.WriteString($"Email send operation failed with error code: {ex.ErrorCode}, message: {ex.Message}");
            }

            return response;
        }
    }
}

