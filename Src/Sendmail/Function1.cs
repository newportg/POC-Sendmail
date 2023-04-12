using System.Net;
using System.Net.Http;
using System.Text;
using Azure;
using Azure.Communication.Email;
using Azure.Core;
using Azure.Identity;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Extensions.Logging;

namespace Sendmail
{
    public class Function1
    {
        private readonly ILogger _logger;

        public Function1(ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.CreateLogger<Function1>();
        }

        // Azure Function for handling negotation protocol for SignalR. It returns a connection info
        // that will be used by Client applications to connect to the SignalR service.
        // It is recommended to authenticate this Function in production environments.
        //[Function("negotiate")]
        //public static SignalRConnectionInfo GetSignalRInfo(
        //    [HttpTrigger(AuthorizationLevel.Anonymous, "post")] HttpRequest req,
        //    [SignalRConnectionInfoInput(HubName = "cloudEventSchemaHub")] SignalRConnectionInfo connectionInfo,
        //    ILogger log)
        //{
        //    log.LogInformation($"SignalR Connection URL = '{connectionInfo.Url}'");
        //    return connectionInfo;
        //}

        [Function("negotiate")]
        [OpenApiOperation(operationId: "negotiate")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(SignalRConnectionInfo), Description = "The OK response")]
        public SignalRConnectionInfo Negotiate([HttpTrigger(AuthorizationLevel.Anonymous, "post")] HttpRequestData req,
            [SignalRConnectionInfoInput(HubName = "serverless")] SignalRConnectionInfo connectionInfo)
        {
            _logger.LogInformation($"SignalR connection string = '{Environment.GetEnvironmentVariable("AzureSignalRConnectionString")}'");

            _logger.LogInformation($"SignalR Connection URL = '{connectionInfo.Url}'");

            //var response = req.CreateResponse(HttpStatusCode.OK);
            //response.Headers.Add("Content-Type", "application/json");
            //response.WriteString(connectionInfo);
            return connectionInfo;
        }


        //[Function("SendMail")]
        //public HttpResponseData Run([HttpTrigger(AuthorizationLevel.Anonymous, "get")] HttpRequestData req)
        //{
        //    _logger.LogInformation("C# HTTP trigger function processed a request.");

        //    var response = req.CreateResponse(HttpStatusCode.OK);
        //    response.Headers.Add("Content-Type", "text/plain; charset=utf-8");

        //    // Find your Communication Services resource in the Azure portal
        //    var connectionString = Environment.GetEnvironmentVariable("CommunicationServicesConnectionString");
        //    EmailClient emailClient = new EmailClient(connectionString);

        //    // ID Token
        //    //string endpoint = "https://cs-poc-sendmail-vse-ne.communication.azure.com";
        //    //TokenCredential tokenCredential = new DefaultAzureCredential();
        //    //tokenCredential = new DefaultAzureCredential();
        //    //EmailClient emailClient = new EmailClient(new Uri(endpoint), tokenCredential);

        //    try
        //    {
        //        var emailSendOperation = emailClient.Send(
        //            wait: WaitUntil.Completed,
        //            senderAddress: "donotreply@8fa448ea-8145-4330-b77f-29e274c2cc1a.azurecomm.net", // The email address of the domain registered with the Communication Services resource
        //            recipientAddress: "gary.newport@zoomalong.co.uk",
           
        //            subject: "This is the subject",
        //            htmlContent: "<html><body>This is the html body</body></html>");
        //        Console.WriteLine($"Email Sent. Status = {emailSendOperation.Value.Status}");

        //        /// Get the OperationId so that it can be used for tracking the message for troubleshooting
        //        string operationId = emailSendOperation.Id;
        //        Console.WriteLine($"Email operation id = {operationId}");
        //        response.WriteString($"Email operation id = {operationId}");
        //    }
        //    catch (RequestFailedException ex)
        //    {
        //        /// OperationID is contained in the exception message and can be used for troubleshooting purposes
        //        Console.WriteLine($"Email send operation failed with error code: {ex.ErrorCode}, message: {ex.Message}");
        //        response.WriteString($"Email send operation failed with error code: {ex.ErrorCode}, message: {ex.Message}");
        //    }

        //    return response;
        //}

        //[Function("EventHubTriggerCSharp")]
        //public static string evhtrigger([EventHubTrigger("%eventHubName%", Connection = "EventHubConnectionAppSetting")] string[] input,
        //     FunctionContext context)
        //{
        //    var logger = context.GetLogger("EventHubsFunction");

        //    logger.LogInformation($"First Event Hubs triggered message: {input[0]}");

        //    var message = $"Output message created at {DateTime.Now}";
        //    return message;
        //}

        //[EventHubTrigger("samples-workitems", Connection = "EventHubConnectionAppSetting")] EventData myEventHubMessage, DateTime enqueuedTimeUtc, Int64 sequenceNumber, string offset,
        //    ILogger log)
        //{
        //    log.LogInformation($"Event: {Encoding.UTF8.GetString(myEventHubMessage.Body)}");
        //    // Metadata accessed by binding to EventData
        //    log.LogInformation($"EnqueuedTimeUtc={myEventHubMessage.SystemProperties.EnqueuedTimeUtc}");
        //    log.LogInformation($"SequenceNumber={myEventHubMessage.SystemProperties.SequenceNumber}");
        //    log.LogInformation($"Offset={myEventHubMessage.SystemProperties.Offset}");
        //    // Metadata accessed by using binding expressions in method parameters
        //    log.LogInformation($"EnqueuedTimeUtc={enqueuedTimeUtc}");
        //    log.LogInformation($"SequenceNumber={sequenceNumber}");
        //    log.LogInformation($"Offset={offset}");
        //}
    }
}
