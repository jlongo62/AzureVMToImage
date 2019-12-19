using Microsoft.VisualStudio.TestTools.UnitTesting;

//using FunctionApp1;
using System.Threading.Tasks;
using System.Net;
using System.Net.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Azure.Management.Fluent;
using Microsoft.Extensions.Configuration;
using Microsoft.Azure.Management.ResourceManager.Fluent;
using System.Collections.Generic;
using System;
using VMSSManagementFunctions.Test;
using VMSSManagementFunctions;
using Microsoft.Azure.WebJobs;
//https://docs.microsoft.com/en-us/azure/azure-functions/functions-test-a-function

namespace AzureFunctions.Test
{
    [TestClass]
    public class ValidateConfigurationTest
    {

        public TestContext TestContext { get; set; }

        [TestMethod]
        public async Task ValidateConfigurationAzureFunction()
        {

            ILogger logger = new TestLogger(TestContext);
            var request = TestFactory.CreateHttpRequest();
            var response = (OkObjectResult)await ValidateConfiguration.Run(request, logger, null);


            Assert.IsTrue(response is OkObjectResult);
        }
    }
}
