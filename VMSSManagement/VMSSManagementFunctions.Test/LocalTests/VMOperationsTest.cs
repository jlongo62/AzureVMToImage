using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading.Tasks;
using System.Net;
using System.Net.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using VMSSManagementFunctions;
using VMSSManagementFunctions.Test;
using Microsoft.Azure.Management.Fluent;
using System.Collections.Generic;
using Microsoft.Azure.Management.Compute.Fluent;
using Microsoft.Azure.Management.AppService.Fluent.Models;
using Microsoft.Azure.Management.ResourceManager.Fluent.Core;

//https://github.com/Azure-Samples/compute-dotnet-manage-virtual-machine-scale-sets/blob/master/Program.cs
namespace Local.Test
{

    [TestClass]
    public class VMOperationsTest: BaseLocalTest
    {


        string sourceResourceGroupName = "VMSSManagementTarget";
        string sourceVMName = "vmSource";
        string targetResourceGroupName = "VMSSManagementTemp";
        string targetVMName = System.Guid.NewGuid().ToString();
        string location = "eastus";

        [TestMethod]
        public async Task CloneVM()
        {
            IAzure azure = Login();
            ILogger logger = new TestLogger(TestContext);

            var operations = new VMSSOperations.VMOperations(azure,logger);


            var vm = await operations.CloneVM(sourceResourceGroupName, sourceVMName, targetResourceGroupName, targetVMName, targetResourceGroupName);

            Assert.IsTrue(vm != null);

        }

        [TestMethod]
        public async Task GetUri()
        {
            IAzure azure = Login();
            ILogger logger = new TestLogger(TestContext);

            string vmssResourceGroupName = "VMSSManagement";
            string location = "eastus";

            var operations = new VMSSOperations.VMOperations(azure, logger);

            var uri = await operations.GetBlobUri(vmssResourceGroupName, location);

            Assert.IsFalse(string.IsNullOrEmpty(uri));

            TestContext.WriteLine(uri);

        }

    }
}