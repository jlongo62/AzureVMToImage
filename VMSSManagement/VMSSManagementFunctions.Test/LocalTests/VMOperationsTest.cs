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



        [TestMethod]
        public async Task CloneVM()
        {

            string sourceResourceGroupName = "VMSSManagementSource";
            string sourceVMName = "vmSource";

            string targetResourceGroupName = "VMSSManagementTarget";
            string targetVMName = System.Guid.NewGuid().ToString();


            IAzure azure = Login();
            ILogger logger = new TestLogger(TestContext);

            var operations = new VMSSOperations.VMOperations(azure,logger, targetVMName);


            var vm = await operations.CloneVM(
                    sourceResourceGroupName, sourceVMName, 
                    targetResourceGroupName, targetVMName);

            Assert.IsTrue(vm != null);

        }

        [TestMethod]
        public async Task CreateImage()
        {

            string sourceResourceGroupName = "VMSSManagementSource";
            string sourceVMName = "vmSource";

            string tempResourceGroupName = "VMSSManagementTemp";

            string targetResourceGroupName = "VMSSManagementDest";
            string targetImageName = System.Guid.NewGuid().ToString();

            string vmssResourceGroupName = "VMSSManagement";
            string vmssLocation = "eastus";


            IAzure azure = Login();
            ILogger logger = new TestLogger(TestContext);

            var operations = new VMSSOperations.VMOperations(azure, logger, targetImageName);


            var vm = await operations.CreateImage(
                                    sourceResourceGroupName, sourceVMName, 
                                    tempResourceGroupName,
                                    targetResourceGroupName, targetImageName, 
                                    vmssResourceGroupName, vmssLocation);

            Assert.IsTrue(vm != null);

        }

        [TestMethod]
        public async Task GetUri()
        {
            IAzure azure = Login();
            ILogger logger = new TestLogger(TestContext);

            string vmssResourceGroupName = "VMSSManagement";
            string vmssLocation = "eastus";

            var operations = new VMSSOperations.VMOperations(azure, logger, "GetUri");

            var uri = await operations.GetBlobUri(vmssResourceGroupName, vmssLocation);

            Assert.IsFalse(string.IsNullOrEmpty(uri));

            TestContext.WriteLine(uri);

        }

        [TestMethod]
        public async Task ApplyExtension()
        {

            string tempResourceGroupName = "VMSSManagementTemp";

            string vmssResourceGroupName = "VMSSManagement";
            string vmssLocation = "eastus";


            IAzure azure = Login();
            ILogger logger = new TestLogger(TestContext);

            var operations = new VMSSOperations.VMOperations(azure, logger, "ApplyExtension");
            var blobUri = await operations.GetBlobUri(vmssResourceGroupName, vmssLocation);

            var newVM = azure.VirtualMachines.GetByResourceGroup(tempResourceGroupName, "10f21bc7-c1f1-4f73-b809-4c0924551e35_vm_temp");

            //        {
            //            "$schema": "https://schema.management.azure.com/schemas/2015-01-01/deploymentParameters.json#",
            //"contentVersion": "1.0.0.0",
            //"parameters": {
            //                "vmName": {
            //                    "value": "10f21bc7-c1f1-4f73-b809-4c0924551e35_vm_temp"
            //                },
            //    "location": {
            //                    "value": "eastus"
            //    },
            //    "fileUris": {
            //                    "value": "https://iaasv2tempstoreeastus.blob.core.windows.net/vmextensionstemporary-0003bffda89860b1-20191228174107441/SysPrep.ps1?sv=2017-04-17&sr=c&sig=gynUHvuVE8%2FbTWCOh9%2FSEwIr4o10MdfZBJujLjMg6HQ%3D&se=2019-12-29T17%3A41%3A07Z&sp=rw"
            //    },
            //    "arguments": { "value": null }
            //            }}
            //            {
            //                "$schema": "http://schema.management.azure.com/schemas/2015-01-01/deploymentTemplate.json#",
            //    "contentVersion": "1.0.0.0",
            //    "parameters": {
            //        "vmName": {  "type": "String"},
            //        "location": {"type": "String"},
            //        "fileUris": {"type": "String"},
            //        "arguments": {"defaultValue": " ",
            //            "type": "SecureString"}
            //                },
            //    "variables": {
            //        "UriFileNamePieces": "[split(parameters('fileUris'), '/')]",
            //        "firstFileNameString": "[variables('UriFileNamePieces')[sub(length(variables('UriFileNamePieces')), 1)]]",
            //        "firstFileNameBreakString": "[split(variables('firstFileNameString'), '?')]",
            //        "firstFileName": "[variables('firstFileNameBreakString')[0]]"
            //    },
            //    "resources": [
            //        {
            //            "type": "Microsoft.Compute/virtualMachines/extensions",
            //            "apiVersion": "2015-06-15",
            //            "name": "[concat(parameters('vmName'),'/CustomScriptExtension')]",
            //            "location": "[parameters('location')]",
            //            "properties": {
            //                "publisher": "Microsoft.Compute",
            //                "type": "CustomScriptExtension",
            //                "typeHandlerVersion": "1.9",
            //                "autoUpgradeMinorVersion": true,
            //                "settings": {
            //                    "fileUris": "[split(parameters('fileUris'), ' ')]"
            //                },
            //                "protectedSettings": {
            //                    "commandToExecute": "[concat ('powershell -ExecutionPolicy Unrestricted -File ', variables('firstFileName'), ' ', parameters('arguments'))]"
            //                }
            //            }
            //        }
            //    ]
            //}
            var cmd = "powershell -ExecutionPolicy Unrestricted -File SysPrep.ps1";
            var extension = await newVM.Update()
                        .DefineNewExtension("runsysprep")
                        .WithPublisher("Microsoft.Compute")
                        .WithType("CustomScriptExtension")
                        .WithVersion("1.9")
                        .WithMinorVersionAutoUpgrade()                        
                        .WithPublicSetting("fileUris", new string[] { blobUri } )
                        .WithPublicSetting("commandToExecute", cmd)
                        .Attach()
                        .ApplyAsync();

        }

    }
}