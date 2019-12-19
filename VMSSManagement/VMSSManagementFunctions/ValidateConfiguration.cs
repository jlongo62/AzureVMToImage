using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Microsoft.Azure.Management.ResourceManager.Fluent;
using System.Collections.Generic;
using Microsoft.Extensions.Configuration;

namespace VMSSManagementFunctions
{
    public static class ValidateConfiguration
    {
        [FunctionName("ValidateConfiguration")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)] HttpRequest req,
            ILogger log, ExecutionContext context)
        {

           var result = new List<string>();

           try
            {

                log.LogInformation("Test.Authenticate");

                if (context == null)
                {
                    log.LogInformation("!!!Context is null. Faking Context...");
                    context = new ExecutionContext() { FunctionAppDirectory = AppContext.BaseDirectory };
                }

                var config = new ConfigurationBuilder()
                    .SetBasePath(context.FunctionAppDirectory)
                    .AddJsonFile("settings.json", optional: true, reloadOnChange: true)
                    .AddJsonFile("local.settings.json", optional: true, reloadOnChange: true)
                    .AddEnvironmentVariables()
                    .Build();

                //App registrations> VMSSManagement
                string azureEnvironmentString =config["AzureEnvironment"];
                string tenantId = config["TenantId"];
                string subscriptionId =config["SubscriptionId"];
                string clientId =config["ClientId"];
                string clientSecret =config["ClientSecret"];

                log.LogInformation($"\tazureEnvironment={azureEnvironmentString}");
                log.LogInformation($"\ttenantId={tenantId}");
                log.LogInformation($"\ttenantId={tenantId}");
                log.LogInformation($"\tsubscriptionId={subscriptionId}");
                log.LogInformation($"\tclientId={clientId}");

                AzureEnvironment azureEnvironment = AzureEnvironment.FromName(azureEnvironmentString);

                log.LogInformation($"Authentication Test...");
                result.Add($"Authentication Test...");

                var azure = VMSSOperations.Authentication.Authenticate(
                        clientId, clientSecret, tenantId, subscriptionId, azureEnvironment);
                log.LogInformation($"Authentication Passed.");
                result.Add($"Authentication Passed.");


                return (ActionResult)new OkObjectResult(result);
            }
            catch (Exception ex)
            {
                result.Add(ex.ToString());
                return  new BadRequestObjectResult(result);

            }


        }
    }
}


//var credentials = SdkContext.AzureCredentialsFactory.FromServicePrincipal(ClientId, ClientSecret, AzureTenantId, AzureEnvironment.AzureGlobalCloud);

//var azure = Microsoft.Azure.Management.Fluent.Azure
//    .Configure()
//    .Authenticate(credentials)
//    .WithSubscription(subscriptionid);

//var windowsVM = azure.VirtualMachines.Define("myWindowsVM")
//    .WithRegion(Region.USWest)
//    .WithNewResourceGroup(rgName)
//    .WithNewPrimaryNetwork("10.0.0.0/28")
//    .WithPrimaryPrivateIPAddressDynamic()
//    .WithNewPrimaryPublicIPAddress("mywindowsvmdns")
//    .WithPopularWindowsImage(KnownWindowsVirtualMachineImage.WindowsServer2012R2Datacenter)
//    .WithAdminUsername("tirekicker")
//    .WithAdminPassword(password)
//    .WithSize(VirtualMachineSizeTypes.StandardD3V2)
//    .Create();