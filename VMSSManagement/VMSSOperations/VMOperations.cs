using Microsoft.Azure.Management.Compute.Fluent;
using Microsoft.Azure.Management.Fluent;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Azure.Management.Network.Fluent;
using Microsoft.Azure.Management.Network.Fluent.NetworkInterface.Definition;
using Microsoft.Azure.Management.Storage.Fluent;
using Microsoft.WindowsAzure.Storage.Auth;
using Microsoft.WindowsAzure.Storage.Blob;
using System.Reflection;
using Microsoft.Azure.Management.ResourceManager.Fluent.Core;

namespace VMSSOperations
{
    public class VMOperations
    {
        private IAzure azure;
        private ILogger logger;

        public VMOperations(IAzure azure, ILogger logger)
        {
            this.azure = azure;
            this.logger = logger;
        }

        public async Task<IVirtualMachine> CloneVM(string sourceResourceGroupName, string sourceVMName, string targetResourceGroupName, string targetVMName, string tempResourceGroupName)
        {

            var vm = azure.VirtualMachines.GetByResourceGroup(sourceResourceGroupName, sourceVMName);
            var tags = GetTags(vm);

            var tempImageName = $"{targetVMName}_image_temp";

            var tempImage = await azure.VirtualMachineCustomImages.Define(targetVMName)
                .WithRegion(vm.Region)
                .WithExistingResourceGroup(tempResourceGroupName)
                .FromVirtualMachine(vm)
                .WithTags(tags)
                .CreateAsync();

            var nicDefinitions = await CreateNICs(sourceResourceGroupName, sourceVMName, targetResourceGroupName, targetVMName, vm, tags);

            var newVM = await azure.VirtualMachines.Define(targetVMName)
                .WithRegion(vm.Region)
                .WithExistingResourceGroup(targetResourceGroupName)
                .WithNewPrimaryNetworkInterface(nicDefinitions[0])
                .WithWindowsCustomImage(tempImage.Id)
                .WithAdminUsername("")
                .WithAdminPassword("")
                .WithTags(tags)
                .CreateAsync();


            return newVM;
        }
        public async Task<string> GetBlobUri(string tempResourceGroupName, string location)
        {


            const string storeAccountPrefix = "vmssmgmtstorage";
            const string containerName = "scripts";
            const string blobName = "sysprep.ps1";

            var resourceGroup = azure.ResourceGroups.GetByName(tempResourceGroupName);

            var b = Encoding.ASCII.GetBytes(resourceGroup.Id);
            var crc32 = new Crc32();
            var suffix = crc32.Get(b).ToString();

            var storageAccountName = $"{storeAccountPrefix}{suffix}".ToLower().Substring(0, 23);


            logger.LogInformation($"Get StorageAccount from Location: {location}; ResourceGroupName: {tempResourceGroupName}; StorageAccount Name: {storageAccountName}...");

            var storageAcount = await GetStorageAccount(tempResourceGroupName, location, blobName, containerName, storageAccountName);

            logger.LogInformation($"Get Blob Uri from Container: {containerName}; Blob: {blobName}...");

            var cloundStorageCredentials = new StorageCredentials(storageAccountName, storageAcount.GetKeys()[0].Value);
            var cloudStorageAccount = new Microsoft.WindowsAzure.Storage.CloudStorageAccount(cloundStorageCredentials, true);

            var blobClient = cloudStorageAccount.CreateCloudBlobClient();
            var container = blobClient.GetContainerReference(containerName);

            var blob = container.GetBlockBlobReference(blobName);

            var policy = new SharedAccessBlobPolicy()
            {
                Permissions = SharedAccessBlobPermissions.Read,
                SharedAccessStartTime = DateTime.Now.ToUniversalTime(),
                SharedAccessExpiryTime = DateTime.Now.AddMinutes(30).ToUniversalTime()
            };

            var sasToken = blob.GetSharedAccessSignature(policy);
            var uri = blob.Uri + sasToken;

            logger.LogInformation($"Uri: {uri}");

            return uri;

        }

        private async Task<IStorageAccount> GetStorageAccount(string resourceGroupName, string location, string blobName, string containerName, string storageAccountName)
        {
            logger.LogInformation($"\tGetStorageAccount Begin...");

            IStorageAccount storageAccount = null;

            storageAccount = azure.StorageAccounts.GetByResourceGroup(resourceGroupName, storageAccountName);

            if (storageAccount == null)
            {

                logger.LogInformation($"Storage Account not found...");
                logger.LogInformation($"\tCreating Storage Account...");

                storageAccount = await azure.StorageAccounts.Define(storageAccountName)
                    .WithRegion(location)
                    .WithExistingResourceGroup(resourceGroupName)
                    .WithAccessFromAllNetworks()
                    .WithBlobStorageAccountKind()
                    .WithOnlyHttpsTraffic()
                    .CreateAsync();

                var cloundStorageCredentials = new StorageCredentials(storageAccountName, storageAccount.GetKeys()[0].Value);
                var cloudStorageAccount = new Microsoft.WindowsAzure.Storage.CloudStorageAccount(cloundStorageCredentials, true);

                var blobClient = cloudStorageAccount.CreateCloudBlobClient();
                var container = blobClient.GetContainerReference(containerName);

                logger.LogInformation($"\tCreating container...");

                var result = await container.CreateIfNotExistsAsync();

                var permission = await container.GetPermissionsAsync();
                permission.PublicAccess = BlobContainerPublicAccessType.Off;
                await container.SetPermissionsAsync(permission);

                logger.LogInformation($"\tCreating blob...");
                var blob = container.GetBlockBlobReference(blobName);

                var assembly = Assembly.GetExecutingAssembly();
                var resourceName = "VMSSOperations.SysPrep.ps1";

                var content = assembly.GetManifestResourceStream(resourceName);

                logger.LogInformation($"\tUploading blob...");
                await blob.UploadFromStreamAsync(content);

            }

            logger.LogInformation($"\tGetStorageAccount Complete.");

            return storageAccount;

        }



        //public async Task<List<IVirtualMachine>> CreateTempImage(string sourceResourceGroupName, string sourceVMName, string targetResourceGroupName, string targetVMName, IVirtualMachine vm, Dictionary<string, string> tags)
        //{


        //	//var nics = await CreateNICs(sourceResourceGroupName, sourceVMName, targetResourceGroupName, targetVMName, vm, tags);

        //	//var vmDefinition = azure.VirtualMachines.Define(targetVMName)
        //	//						.WithRegion(vm.Region)
        //	//						.WithExistingResourceGroup(targetResourceGroupName)
        //	//						.WithNewPrimaryNetworkInterface(nics[0])
        //	//						.WithSpecializedOSDisk



        //	return null;

        //}

        public async Task<List<IWithCreate>> CreateNICs(string sourceResourceGroupName, string sourceVMName, string targetResourceGroupName, string targetVMName, IVirtualMachine vm, Dictionary<string, string> tags)
        {
            var results = new List<IWithCreate>();

            var cnt = 1;
            string targetResourceName;


            foreach (var nicId in vm.NetworkInterfaceIds)
            {
                targetResourceName = $"{targetVMName}_NIC_{cnt.ToString()}";

                var nic = azure.NetworkInterfaces.GetById(nicId);
                var nsg = azure.NetworkSecurityGroups.GetById(nic.NetworkSecurityGroupId);

                var newNic = azure.NetworkInterfaces.Define(targetResourceName)
                                    .WithRegion(vm.Region)
                                    .WithExistingResourceGroup(targetResourceGroupName)
                                    .WithExistingPrimaryNetwork(nic.PrimaryIPConfiguration.GetNetwork())
                                    .WithSubnet(nic.PrimaryIPConfiguration.SubnetName)
                                    .WithPrimaryPrivateIPAddressDynamic()
                                    .WithExistingNetworkSecurityGroup(nsg)
                                    .WithTags(tags);

                results.Add(newNic);
            }

            return results;
        }

        //public async Task<List<ISnapshot>> CreateSnapshotsAsync(string sourceResourceGroupName, string sourceVMName, string targetResourceGroupName, string targetVMName, IVirtualMachine vm, Dictionary<string, string> tags)
        //{

        //	var results = new List<ISnapshot>();

        //	var cnt = 1;
        //	string targetResourceName;

        //	ISnapshot shapshot;



        //	targetResourceName = $"{targetVMName}_OSDisk";
        //	logger.LogInformation($"{targetResourceName}: ");

        //	shapshot = await azure.Snapshots.Define(targetVMName)
        //						.WithRegion(vm.Region)
        //						.WithExistingResourceGroup(targetResourceGroupName)
        //						.WithWindowsFromDisk(vm.StorageProfile.OsDisk.ManagedDisk.Id)
        //						.WithTags(tags)
        //						.WithIncremental(false)
        //						.CreateAsync();

        //	logger.LogInformation(shapshot.Id);
        //	results.Add(shapshot);

        //	foreach (var disk in vm.DataDisks)
        //	{
        //		targetResourceName = $"{targetVMName}_DataDisk_{cnt.ToString()}";
        //		logger.LogInformation($"{targetResourceName}: ");

        //		shapshot = await azure.Snapshots.Define(targetResourceName)
        //											.WithRegion(vm.Region)
        //											.WithExistingResourceGroup(targetResourceGroupName)
        //											.WithWindowsFromDisk(disk.Value.Inner.ManagedDisk.Id)
        //											.WithTags(tags)
        //											.WithIncremental(false)
        //											.CreateAsync();
        //		logger.LogInformation(shapshot.Id);

        //		cnt++;

        //		results.Add(shapshot);
        //	}

        //	return results;

        //}

        private Dictionary<string, string> GetTags(IVirtualMachine vm)
        {
            return new Dictionary<string, string>(vm.Tags);
        }
    }
}
