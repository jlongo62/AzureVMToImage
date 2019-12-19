using Microsoft.VisualStudio.TestTools.UnitTesting;

//using FunctionApp1;
using System.Threading.Tasks;
using System.Net;
using System.Net.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using VMSSManagementFunctions;
using VMSSManagementFunctions.Test;
//https://docs.microsoft.com/en-us/azure/azure-functions/functions-test-a-function
namespace Local.Test
{

    [TestClass]
    public class FunctionsTests
    {
        private readonly ILogger logger = TestFactory.CreateLogger();

        [TestMethod]
        public void TestMethod1()
        {
            Assert.IsTrue(true);
        }
        [TestMethod]
        public void TestMethod2()
        {
            Assert.IsTrue(true);
        }
        [TestMethod]
        public async Task Function1Test()
        {
            var request = TestFactory.CreateHttpRequest("name", "Bill");
            var response = (OkObjectResult)await Function1.Run(request, logger);
            Assert.IsTrue("Hello, Bill" == (string)response.Value);
        }

    }
}