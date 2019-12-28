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
        private string key;
        private DateTime dateTime;
        const string dateTimeFormat = "yyyy_MM_dd_HH_mm_ss";

        public VMOperations(IAzure azure, ILogger logger, string key)
        {
            this.azure = azure;
            this.logger = logger;
            this.key = key;
            this.dateTime = DateTime.Now.ToUniversalTime();
        }


        public async Task<IVirtualMachine> CloneVM(
                string sourceResourceGroupName, string sourceVMName,
                string targetResourceGroupName, string targetVMName)
        {
            logger.LogInformation($"CloneVM Begin...");
            logger.LogInformation($"\tGet Source VM --> ResourceGroupName: {sourceResourceGroupName}; VM Name: {sourceVMName}...");

            var vm = azure.VirtualMachines.GetByResourceGroup(sourceResourceGroupName, sourceVMName);

            logger.LogInformation($"CloneVM Complete...");

            var result = await CloneVM(vm, targetResourceGroupName, targetVMName);

            return result;
        }

        public async Task<IVirtualMachine> CloneVM(
        IVirtualMachine vm,
        string targetResourceGroupName, string targetVMName)
        {
            logger.LogInformation($"CloneVM Begin...");
            logger.LogInformation($"\t Source VM --> ResourceGroupName: {vm.ResourceGroupName }; VM Name: {vm.Name}...");

            var tags = GetTags(vm);

            logger.LogInformation($"\tCreate Disks --> ResourceGroupName: {targetResourceGroupName}; Snapshot Name: {targetVMName}...");
            var disks = await CreateDisksAsync(targetResourceGroupName, targetVMName, vm);

            var tempNICName = $"{targetVMName}_nic_temp";
            logger.LogInformation($"\tGet NIC Definitions: {tempNICName}...");
            var nicDefinitions = await CreateNICs(targetResourceGroupName, vm.Name, targetResourceGroupName, tempNICName, vm, tags);

            logger.LogInformation($"\tCreate VM: {targetVMName}...");
            var tempVM = await azure.VirtualMachines.Define(targetVMName)
                .WithRegion(vm.Region)
                .WithExistingResourceGroup(targetResourceGroupName)
                .WithNewPrimaryNetworkInterface(nicDefinitions[0])
                .WithSpecializedOSDisk(disks[0], disks[0].OSType.Value)
                .WithBootDiagnostics()
                .WithSize(vm.Size)
                .WithTags(tags)
                .CreateAsync();

            logger.LogInformation($"\tAdd data disks...");
            for (int i = 1; i < disks.Count; i++)
            {
                logger.LogInformation($"\t\tAdd data disk: {disks[i].Name}...");
                var tempVM2 = azure.VirtualMachines.GetByResourceGroup(targetResourceGroupName, targetVMName);
                var tempVM3 = await tempVM2.Update()
                                    .WithExistingDataDisk(disks[i])
                                    .ApplyAsync();
            }

            if (disks.Count > 1)
            {
                var tempVM4 = azure.VirtualMachines.GetByResourceGroup(targetResourceGroupName, targetVMName);
                tempVM4.Restart();
                logger.LogInformation($"\tRestart VM...");
            }

            logger.LogInformation($"CloneVM Complete...");

            var result = azure.VirtualMachines.GetByResourceGroup(targetResourceGroupName, targetVMName);
            return result;
        }

        public async Task<List<IDisk>> CreateDisksAsync(string tempResourceGroupName, string targetVMName, IVirtualMachine vm)
        {
            IDisk disk = null;

            var results = new List<IDisk>();
            var tags = GetTags(vm);

            var diskName = $"{targetVMName}_OSDisk";
            disk = await CreateDiskAsync(tempResourceGroupName, targetVMName, vm, vm.StorageProfile.OsDisk.ManagedDisk.Id, tags, diskName);
            results.Add(disk);

            var cnt = 1;
            foreach (var item in vm.StorageProfile.DataDisks)
            {
                var dataDiskName = $"{targetVMName}_DataDisk_{cnt}";
                disk = await CreateDiskAsync(tempResourceGroupName, targetVMName, vm, item.ManagedDisk.Id, tags, dataDiskName);
                results.Add(disk);
                cnt++;
            }

            return results;
        }

        private async Task<IDisk> CreateDiskAsync(string tempResourceGroupName, string targetVMName, IVirtualMachine vm, string id, Dictionary<string, string> tags,string diskName)
        {
            var source = azure.Disks.GetById(id);

            var disk = await azure.Disks.Define(diskName)
                .WithRegion(vm.Region)
                .WithExistingResourceGroup(tempResourceGroupName)
                .WithWindowsFromDisk(source.Id)
                .WithSizeInGB(source.SizeInGB)
                .WithHyperVGeneration(source.HyperVGeneration)
                .WithSku(source.Sku)
                .WithTags(tags)
                .CreateAsync();

            return disk;
        }

        //public async Task<IVirtualMachine> CloneVM(
        //        string sourceResourceGroupName, string sourceVMName, 
        //        string targetResourceGroupName, string targetVMName, string tempResourceGroupName)
        //{
        //    logger.LogInformation($"CloneVM Begin...");
        //    logger.LogInformation($"\tGet Source VM: ResourceGroupName: {sourceResourceGroupName}; VM Name: {sourceVMName}...");

        //    var vm = azure.VirtualMachines.GetByResourceGroup(sourceResourceGroupName, sourceVMName);
        //    var tags = GetTags(vm);

        //    var tempImageName = $"{targetVMName}_image_temp";

        //    logger.LogInformation($"\tCreate temporary image: {tempImageName}...");

        //    var tempImage = await azure.VirtualMachineCustomImages.Define(tempImageName)
        //        .WithRegion(vm.Region)
        //        .WithExistingResourceGroup(tempResourceGroupName)
        //        .FromVirtualMachine(vm)
        //        .WithTags(tags)
        //        .CreateAsync();

        //    var tempNICName = $"{targetVMName}_nic_temp";
        //    logger.LogInformation($"\tGet NIC Definitions: {tempNICName}...");
        //    var nicDefinitions = await CreateNICs(sourceResourceGroupName, sourceVMName, targetResourceGroupName, tempNICName, vm, tags);

        //    logger.LogInformation($"\tCreate VM: {targetVMName}...");
        //    var newVM = await azure.VirtualMachines.Define(targetVMName)
        //        .WithRegion(vm.Region)
        //        .WithExistingResourceGroup(tempResourceGroupName)
        //        .WithNewPrimaryNetworkInterface(nicDefinitions[0])
        //        .WithWindowsCustomImage(tempImage.Id)
        //        .WithAdminUsername("")
        //        .WithAdminPassword("")
        //        .WithTags(tags)
        //        .CreateAsync();

        //    logger.LogInformation($"CloneVM Complete...");

        //    return newVM;
        //}

        public async Task<IVirtualMachineCustomImage> CreateImage(
                string sourceResourceGroupName, string sourceVMName, 
                string tempResourceGroupName,
                string targetResourceGroupName, string targetImageNamePrefix, 
                string vmssResourceGroupName, string vmssLocation)
        {

            var tempVMName = $"{targetImageNamePrefix}_vm_temp";

            logger.LogInformation($"CreateImage Begin...");
            logger.LogInformation($"\tClone VM: Source ResourceGroupName: {sourceResourceGroupName}; Source VM Name: {sourceVMName}...");
            logger.LogInformation($"\tClone VM: Temp ResourceGroupName: {tempResourceGroupName}; Temp VM Name: {tempVMName}...");

            logger.LogInformation($"\tGet Source VM --> ResourceGroupName: {sourceResourceGroupName}; VM Name: {sourceVMName}...");

            var vm = azure.VirtualMachines.GetByResourceGroup(sourceResourceGroupName, sourceVMName);
            var tags = GetTags(vm);

            var newVM = CloneVM(vm, tempResourceGroupName, tempVMName).Result;

            logger.LogInformation($"\tGet Blob Uri...");
            var blobUri = await GetBlobUri(vmssResourceGroupName, vmssLocation);

            logger.LogInformation($"\tApply Sysprep Extension...");
            //https://docs.microsoft.com/en-us/azure/virtual-machines/extensions/custom-script-windows

            var cmd = "powershell -ExecutionPolicy Unrestricted -File SysPrep.ps1";

            var extension = newVM.Update()
                        .DefineNewExtension("runsysprep")
                        .WithPublisher("Microsoft.Compute")
                        .WithType("CustomScriptExtension")
                        .WithVersion("1.9")
                        .WithMinorVersionAutoUpgrade()
                        .WithPublicSetting("fileUris", new string[] { blobUri })
                        .WithPublicSetting("commandToExecute", cmd)
                        .Attach()
                        .Apply();

            var targetImageName = $"{targetImageNamePrefix}_{DateTime.UtcNow.ToString(dateTimeFormat)}";

            logger.LogInformation($"\tDeallocate temp VM...");
            newVM.Deallocate();

            logger.LogInformation($"\tGeneralize temp VM...");
            newVM.Generalize();

            logger.LogInformation($"\tCreate Image --> ResourceGroupName: {targetResourceGroupName}; Image Name: {targetImageName}...");
            var image = await azure.VirtualMachineCustomImages.Define(targetImageName)
                        .WithRegion(newVM.Region)
                        .WithExistingResourceGroup(targetResourceGroupName)
                        .FromVirtualMachine(newVM)
                        .WithTags(tags)
                        .CreateAsync();

            logger.LogInformation($"CreateImage Complete.");

            return image;
        }
        public async Task<string> GetBlobUri(string resourceGroupName, string location)
        {


            const string storeAccountPrefix = "vmssmgmtstorage";
            const string containerName = "scripts";
            const string blobName = "sysprep.ps1";

            var resourceGroup = azure.ResourceGroups.GetByName(resourceGroupName);

            var b = Encoding.ASCII.GetBytes(resourceGroup.Id);
            var crc32 = new Crc32();
            var suffix = crc32.Get(b).ToString();

            var storageAccountName = $"{storeAccountPrefix}{suffix}".ToLower().Substring(0, 23);


            logger.LogInformation($"Get StorageAccount from Location: {location}; ResourceGroupName: {resourceGroupName}; StorageAccount Name: {storageAccountName}...");

            var storageAcount = await GetStorageAccount(resourceGroupName, location, blobName, containerName, storageAccountName);

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

        public async Task<List<ISnapshot>> CreateSnapshotsAsync(string targetResourceGroupName, string targetSnapShotName, IVirtualMachine vm)
        {

            var results = new List<ISnapshot>();

            var cnt = 1;
            string targetResourceName;

            var tags = GetTags(vm);

            ISnapshot shapshot;

            targetResourceName = $"{targetSnapShotName}_OSDisk";
            logger.LogInformation($"{targetResourceName}: ");

            shapshot = await azure.Snapshots.Define(targetSnapShotName)
                                .WithRegion(vm.Region)
                                .WithExistingResourceGroup(targetResourceGroupName)
                                .WithWindowsFromDisk(vm.StorageProfile.OsDisk.ManagedDisk.Id)
                                .WithTags(tags)
                                .WithIncremental(false)
                                .CreateAsync();

            logger.LogInformation(shapshot.Id);
            results.Add(shapshot);

            foreach (var disk in vm.DataDisks)
            {
                targetResourceName = $"{targetSnapShotName}_DataDisk_{cnt.ToString()}";
                logger.LogInformation($"{targetResourceName}: ");

                shapshot = await azure.Snapshots.Define(targetResourceName)
                                                    .WithRegion(vm.Region)
                                                    .WithExistingResourceGroup(targetResourceGroupName)
                                                    .WithWindowsFromDisk(disk.Value.Inner.ManagedDisk.Id)
                                                    .WithTags(tags)
                                                    .WithIncremental(false)
                                                    .CreateAsync();
                logger.LogInformation(shapshot.Id);

                cnt++;

                results.Add(shapshot);
            }

            return results;

        }

        private Dictionary<string, string> GetTags(IVirtualMachine vm)
        {
            var result = new Dictionary<string, string>(vm.Tags);
            result.Add("VMSSMgmntKey", this.key);
            result.Add("VMSSMgmntDate", this.dateTime.ToString(dateTimeFormat));

            return result;
        }
    }
}
