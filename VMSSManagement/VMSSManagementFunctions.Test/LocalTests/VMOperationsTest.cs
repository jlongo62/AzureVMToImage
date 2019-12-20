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

//https://github.com/Azure-Samples/compute-dotnet-manage-virtual-machine-scale-sets/blob/master/Program.cs
namespace Local.Test
{

    [TestClass]
    public class VMOperationsTest: BaseLocalTest
    {

        [TestMethod]
        public async Task GetVM()
        {
            IAzure azure = Login();
            ILogger logger = new TestLogger(TestContext);

            string sourceResourceGroupName = "VMSSManagementTarget";
            string sourceVMName = "vmSource";
            string targetResourceGroupName = "VMSSManagementTemp";

            var operations = new VMSSOperations.VMOperations(azure,logger);
            var snapShots = await operations.CreateSnapshotsAsync(sourceResourceGroupName, sourceVMName, targetResourceGroupName);

            Assert.IsTrue(snapShots.Count > 0);

        }

    }
}