using Microsoft.Azure.Management.Fluent;
using Microsoft.Azure.Management.ResourceManager.Fluent;
using System;
using System.Collections.Generic;
using System.Text;
//https://stackoverflow.com/questions/48248904/how-do-you-authenticate-using-azure-management-fluent-api
namespace VMSSOperations
{
    public class Authentication
    {
        public static IAzure Authenticate(string clientId, string clientSecret, string tenantId, string subscriptionId, AzureEnvironment azureEnvironment)
        {
            var credentials = SdkContext
                .AzureCredentialsFactory.FromServicePrincipal(clientId, clientSecret, tenantId, azureEnvironment);

            var azure = Microsoft.Azure.Management.Fluent.Azure
                .Configure()
                .Authenticate(credentials)
                .WithSubscription(subscriptionId);

            return azure;
        }

    }
}
