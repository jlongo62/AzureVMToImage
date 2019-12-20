using Microsoft.VisualStudio.TestTools.UnitTesting;
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
//https://docs.microsoft.com/en-us/azure/azure-functions/functions-test-a-function

namespace Local.Test
{
    public class BaseLocalTest
    {
        public TestContext TestContext { get; set; }

        public IAzure Login()
        {
            //App registrations> VMSSManagement
            var configBuilder = new ConfigurationBuilder()
                .SetBasePath(TestContext.DeploymentDirectory)
                .AddJsonFile("settings.json", optional: true, reloadOnChange: true)
                .AddJsonFile("local.settings.json", optional: true, reloadOnChange: true)
                .AddEnvironmentVariables();

            var config = configBuilder.Build();

            string clientId = config["clientId"];
            string clientSecret = config["clientSecret"];
            string tenantId = config["tenantId"];
            string subscriptionId = config["subscriptionId"];
            AzureEnvironment azureEnvironment = AzureEnvironment.AzureGlobalCloud;

            var azure = VMSSOperations.Authentication.Authenticate(clientId, clientSecret, tenantId, subscriptionId, azureEnvironment);
            return azure;
        }
    }
}
