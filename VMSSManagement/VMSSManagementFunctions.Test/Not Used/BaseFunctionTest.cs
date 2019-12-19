
using System;
using System.Net.Http;
using Microsoft.Azure.WebJobs.Host;
using System.Text;
using System.Web.Http;
using Newtonsoft.Json;
using Microsoft.Extensions.Logging;

namespace VMSSManagementFunctions.Test
{
    public abstract class BaseFunctionTest
    {
        public abstract class FunctionTest
        {
            public (HttpRequestMessage, ILogger) Arrange(object content)
            {
                HttpMethod httpMethod = HttpMethod.Post;
                Uri requestUri = new Uri("http://tempuri.org");
                HttpRequestMessage req = new HttpRequestMessage(httpMethod, requestUri);
                var tempPath = Environment.GetEnvironmentVariable("temp");
                //HttpRouteCollection routeCollection = new HttpRouteCollection(tempPath);
                //req.SetConfiguration(new HttpConfiguration(routeCollection));
                req.Content = new StringContent(
                    JsonConvert.SerializeObject(content),
                    Encoding.UTF8, "application/json");
                var log = new VerboseDiagnosticsTraceWriter();
                return (req, log);
            }
        }
    }
}
