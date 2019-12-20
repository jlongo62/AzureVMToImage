using Microsoft.Azure.Management.Compute.Fluent;
using Microsoft.Azure.Management.Fluent;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

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

        public async Task<List<ISnapshot>> CreateSnapshotsAsync(string sourceResourceGroupName, string sourceVMName, string targetResourceGroupName)
        {
            var vm = azure.VirtualMachines.GetByResourceGroup(sourceResourceGroupName, sourceVMName);

            Dictionary<string, string> tags = GetTags(vm);

            var targetVMName = System.Guid.NewGuid().ToString();

            ISnapshot shapshot;

            var cnt = 1;
            string targetDiskName;

            var snapshots = new List<ISnapshot>();

            foreach (var disk in vm.DataDisks)
            {
                targetDiskName = $"{targetVMName}_DataDisk_{cnt.ToString()}";
                logger.LogInformation($"{targetDiskName}: ");

                shapshot = await azure.Snapshots.Define(targetDiskName)
                                    .WithRegion(vm.Region)
                                    .WithExistingResourceGroup(targetResourceGroupName)
                                    .WithWindowsFromDisk(disk.Value.Inner.ManagedDisk.Id)
                                    .WithTags(tags)
                                    .WithIncremental(false)
                                    .CreateAsync();

                logger.LogInformation(shapshot.Id);

                cnt++;

                snapshots.Add(shapshot);
            }

            targetDiskName = $"{targetVMName}_OSDisk";
            logger.LogInformation($"{targetDiskName}: ");

            shapshot = await azure.Snapshots.Define(targetVMName)
                                .WithRegion(vm.Region)
                                .WithExistingResourceGroup(targetResourceGroupName)
                                .WithWindowsFromDisk(vm.StorageProfile.OsDisk.ManagedDisk.Id)
                                .WithTags(tags)
                                .WithIncremental(false)
                                .CreateAsync();

            logger.LogInformation(shapshot.Id);
            snapshots.Add(shapshot);


            return snapshots;

        }

        private Dictionary<string, string> GetTags(IVirtualMachine vm)
        {
            return new Dictionary<string, string>(vm.Tags);
        }
    }
}
