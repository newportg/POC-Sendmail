//using System.Collections.Generic;
//using System.Net;
//using Microsoft.Azure.Functions.Worker;
//using Microsoft.Azure.Functions.Worker.Http;
//using Microsoft.Extensions.Logging;

//namespace Sendmail
//{
//    public class Sendmail
//    {
//        private readonly ILogger _logger;

//        public Sendmail(ILoggerFactory loggerFactory)
//        {
//            _logger = loggerFactory.CreateLogger("negotiate");
//        }

//        [Function("negotiate2")]
//        public HttpResponseData Negotiate2(
//            [HttpTrigger(AuthorizationLevel.Anonymous)] HttpRequestData req,
//            [SignalRConnectionInfoInput(HubName = "signalrhub")] MyConnectionInfo connectionInfo)
//        {
//            _logger.LogInformation($"SignalR Connection URL = '{connectionInfo.Url}'");

//            var response = req.CreateResponse(HttpStatusCode.OK);
//            response.Headers.Add("Content-Type", "text/plain; charset=utf-8");
//            response.WriteString($"Connection URL = '{connectionInfo.Url}'");
            
//            return response;
//        }
//    }

//    public class MyConnectionInfo
//    {
//        public string? Url { get; set; }

//        public string? AccessToken { get; set; }
//    }
//}