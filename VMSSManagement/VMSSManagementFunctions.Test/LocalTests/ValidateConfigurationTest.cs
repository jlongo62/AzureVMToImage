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
//https://docs.microsoft.com/en-us/azure/azure-functions/functions-test-a-function

namespace Local.Test
{
    [TestClass]
    public class ValidateConfigurationTest
    {
        public TestContext TestContext { get; set; }

        [TestMethod]
        public void Authenticate()
        {
            IAzure azure = Login();

            var subscriptions = azure.Subscriptions.List();
            var cnt = new List<ISubscription>(subscriptions).Count;

            Assert.IsTrue(cnt >= 1);
        }

        private IAzure Login()
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

        [TestMethod]
        public void ValidateScopes()
        {
            IAzure azure = Login();

            var subscriptions = azure.Subscriptions.List();
            var cnt = new List<ISubscription>(subscriptions).Count;

            var resourceGroups = azure.ResourceGroups.List();
            var cnt2 = new List<ISubscription>(subscriptions).Count;
            Assert.IsTrue(cnt2 >= 1);
        }
        [TestMethod]
        public void ValidateSettings()
        {
            var configBuilder = new ConfigurationBuilder()
                .SetBasePath(TestContext.DeploymentDirectory)
                .AddJsonFile("settings.json", optional: true, reloadOnChange: true)
                .AddEnvironmentVariables();

            var config = configBuilder.Build();

            string clientId = config["clientId"];
            string clientSecret = config["clientSecret"];
            string tenantId = config["tenantId"];
            string subscriptionId = config["subscriptionId"];

            Assert.IsTrue(clientId == "--SECRET--");
            Assert.IsTrue(clientSecret == "--SECRET--");
            Assert.IsTrue(tenantId == "--SECRET--");
            Assert.IsTrue(subscriptionId == "--SECRET--");

        }


    }
}
