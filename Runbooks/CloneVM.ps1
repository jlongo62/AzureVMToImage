#
# Copy_an_existing_Azure_VM.ps1
# https://docs.microsoft.com/en-us/azure/virtual-machines/windows/create-vm-specialized#option-3-copy-an-existing-azure-vm
Import-Module AzureRM.Compute

Set-StrictMode -Version Latest 

Write-Output "CloneVM.ps1 Loaded"

function GetTagsFromVM
(
	[Microsoft.Azure.Commands.Compute.Models.PSVirtualMachine]
	[Parameter(Mandatory=$true)] 
	$vm,

	[ref]
	[OutputType([hashtable])]
	[CmdletBinding()]
	$result
)
{

	Write-Output  ">>>Get VM Tags"

	$tags = @{}
	foreach($tag in $vm.Tags.GetEnumerator())
	{
		$tags.Add($tag.Key, $tag.Value)
	}

	$result.Value = $tags
}

function CreateNewNicsFromVM
(
	[Microsoft.Azure.Commands.Compute.Models.PSVirtualMachine]
	[Parameter(Mandatory=$true)] 
	$vm,
	
	[string] [Parameter(Mandatory=$true)]
	$destLocation,
		
	[string] [Parameter(Mandatory=$true)]
	$destResourceGroupName,

	[string] [Parameter(Mandatory=$true)]
	$destVMName,

	[ref]
	[OutputType([hashtable])]
	[CmdletBinding()]
	$result

)
{

	Write-Output  ">>>CreateNewNicsFromVM: $destVMName"

	$newNics = @{}
	$cnt = 1

	foreach($vmNic in $vm.NetworkProfile.NetworkInterfaces)
	{
		$vmNicResource = Get-AzureRmResource -ResourceId $vmNic.Id

		$vmNic = Get-AzureRmNetworkInterface  `
					-ResourceGroupName $vm.ResourceGroupName `
					-Name $vmNicResource.Name

		$newNic = New-AzureRmNetworkInterface -Name "$destVMName-$cnt" `
		   -ResourceGroupName $destResourceGroupName `
		   -Location $destLocation `
		   -SubnetId $vmNic.IpConfigurations[0].Subnet.Id `
		   -Force:$true  

		$newNics.Add($newNic.Id, $newNic)

		Write-Output( ">>>     " + $newNic.Name + " NIC Created.")

		$cnt++
	}

	$result.Value = $newNics
}


function CreateOSSnapshotFromVM
(

	[Microsoft.Azure.Commands.Compute.Models.PSVirtualMachine]
	[Parameter(Mandatory=$true)] 
	$vm,
	
	[string] [Parameter(Mandatory=$true)]
	$destLocation,
		
	[string] [Parameter(Mandatory=$true)]
	$destResourceGroupName,

	[string] [Parameter(Mandatory=$true)]
	$destVMName,

	[ref]
	[OutputType([Microsoft.Azure.Commands.Compute.Automation.Models.PSSnapshot])]
	[CmdletBinding()]
	$result

)
{

	$snapShotName = $destVMName + '_snapshot_os' 

	Write-Output  ">>>CreateOSSnapshotFromVM: $snapShotName..."

	$disk = Get-AzureRmDisk `
		-ResourceGroupName $vm.ResourceGroupName `
		-DiskName $vm.StorageProfile.OsDisk.Name

	$tags = @{}
	$tags.Add('disk sku', $disk.Sku.Name)


	$snapshotConfig =  New-AzureRmSnapshotConfig `
		-SourceUri $disk.Id  `
		-OsType Windows  `
		-CreateOption Copy  `
		-Location $destLocation `
		-Tag $tags

	$snapShot = New-AzureRmSnapshot `
		-Snapshot $snapshotConfig `
		-SnapshotName $snapShotName `
		-ResourceGroupName $destResourceGroupName 
		

	Write-Output  ">>>     $snapShotName Created."

	$result.Value = $snapShot

}


function CreateDataSnapshotsFromVM
(

	[Microsoft.Azure.Commands.Compute.Models.PSVirtualMachine]
	[Parameter(Mandatory=$true)] 
	$vm,
	
	[string] [Parameter(Mandatory=$true)]
	$destLocation,
		
	[string] [Parameter(Mandatory=$true)]
	$destResourceGroupName,

	[string] [Parameter(Mandatory=$true)]
	$destVMName,

	[ref]
	[OutputType([hashtable])]
	[CmdletBinding()]
	$result

)
{

	Write-Output  ">>>CreateDataSnapshotsFromVM..."

	$snapshots = @{}
	$cnt = 1

	foreach($disk in $vm.StorageProfile.DataDisks)
	{

		$snapShotName = $destVMName + "_snapshot_data_" + $cnt
	
		Write-Output  ">>>     Create the Snapshot(s): $snapShotName..."

		$resource = Get-AzureRmResource -ResourceId $disk.ManagedDisk.Id
		$diskItem = Get-AzureRMDisk -ResourceGroupName $resource.ResourceGroupName -DiskName $resource.Name

		$tags = @{}
		$tags.Add('disk sku', $diskItem.Sku.Name)
		$tags.Add('diskSizeGB', ":" + $diskItem.DiskSizeGB)
		$tags.Add('lun', ":" + $disk.Lun ) 

		$snapshotConfig =  New-AzureRmSnapshotConfig `
		  -SourceUri $disk.ManagedDisk.Id  `
		  -OsType Windows  `
		  -CreateOption Copy  `
		  -Location $destLocation `
		  -Tag $tags

		$snapShot = New-AzureRmSnapshot `
		   -Snapshot $snapshotConfig `
		   -SnapshotName $snapShotName `
		   -ResourceGroupName $destResourceGroupName  
   
		$snapshots.Add($snapShot.Id, $snapShot)


		Write-Output  ">>>     Snapshot Created:  $snapShotName"

		$cnt++
	}

	$result.Value = $snapshots
}


function CreateDiskFromSnapshot
(

	[Microsoft.Azure.Commands.Compute.Models.PSVirtualMachine]
	[Parameter(Mandatory=$true)] 
	$vm,
	
	[string] [Parameter(Mandatory=$true)]
	$destLocation,
		
	[string] [Parameter(Mandatory=$true)]
	$destResourceGroupName,

	[string] [Parameter(Mandatory=$true)]
	$destVMName,
		
	[Microsoft.Azure.Commands.Compute.Automation.Models.PSSnapshot]
	[Parameter(Mandatory=$true)] 
	$snapShot,
		
	[string] [Parameter(Mandatory=$true)]
	$diskSuffix,

	[ref]
	[OutputType([Microsoft.Azure.Commands.Compute.Automation.Models.PSDisk])]
	[CmdletBinding()]
	$result

)
{

	$newDiskName = $destVMName + "_" + $diskSuffix

	Write-Output  ">>>CreateDiskFromSnapshot: $newDiskName ..."

	$sku = $snapShot.Tags["disk sku"]

	$newDisk = New-AzureRmDisk `
					-ResourceGroupName $destResourceGroupName `
					-DiskName $newDiskName `
					-Disk `
						(New-AzureRmDiskConfig  `
							-Location $destLocation `
							-CreateOption Copy `
							-SourceResourceId $snapShot.Id `
							-SkuName $sku `
							-Tag $snapShot.Tags `
							) 

	Write-Output ">>>     $newDiskName Created."

	$result.Value = $newDisk

}

function CreateNewVM
(

	[Microsoft.Azure.Commands.Compute.Models.PSVirtualMachine]
	[Parameter(Mandatory=$true)] 
	$vm,
	
	[string] [Parameter(Mandatory=$true)]
	$destLocation,
		
	[string] [Parameter(Mandatory=$true)]
	$destResourceGroupName,

	[string] [Parameter(Mandatory=$true)]
	$destVMName,

	[Microsoft.Azure.Commands.Compute.Automation.Models.PSDisk] [Parameter(Mandatory=$true)]
	$oSDisk,

	[hashtable] [Parameter(Mandatory=$true)]
	$dataDisks,
		
	[hashtable]	[Parameter(Mandatory=$true)] 
	$nics,

	[hashtable]	[Parameter(Mandatory=$true)] 
	$tags,

	[ref]
	[OutputType([Microsoft.Azure.Commands.Compute.Models.PSVirtualMachine])]
	[CmdletBinding()]
	$result

)
{

	Write-Output  ">>>CreateNewVM: $destVMName..."

	$vmConfig = New-AzureRmVMConfig -VMName $destVMName -VMSize $vm.HardwareProfile.VmSize

	foreach($nic in $nics.Values)
	{
		$vmConfig = Add-AzureRmVMNetworkInterface -VM $vmConfig -Id $nic.Id
	}

	$vmConfig = Set-AzureRmVMOSDisk  `
					-VM $vmConfig  `
					-ManagedDiskId $oSDisk.Id  `
					-DiskSizeInGB $oSDisk.DiskSizeGB `
					-CreateOption Attach -Windows 

	$cnt = 1

	foreach($dataDisk in $dataDisks.Values)
	{
		[void]( `
		 Add-AzureRmVMDataDisk `
				-VM $vmConfig `
				-ManagedDiskId $dataDisk.Id `
				-Caching ReadWrite `
				-CreateOption Attach `
				-Lun $dataDisk.Tags["lun"].Replace(":","") )

		$cnt++
	}


	$newVM = New-AzureRmVM -ResourceGroupName $destResourceGroupName -Location $destLocation -VM $vmConfig -Tag $tags

	$newVM = Get-AzureRmVM -Name $destVMName -ResourceGroupName $destResourceGroupName
	
	Write-Output  (">>>     VM: " + $newVM.Name + " has been created.")
	Write-Output  ">>>     Warning: this VM is a exact copy of the source. Until the Server Name is changed, only one VM can be in operation."

	$result.Value = $newVM

}


function CloneVM
(

	[Microsoft.Azure.Commands.Compute.Models.PSVirtualMachine]
	[Parameter(Mandatory=$true)] $vmSource,

	[string] [Parameter(Mandatory=$true)] $destLocation, 

	[string] [Parameter(Mandatory=$true)] $destResourceGroupName,

	[string] [Parameter(Mandatory=$true)] $destVMName,

	[OutputType([Microsoft.Azure.Commands.Compute.Models.PSVirtualMachine])]
	[CmdletBinding()]
	[ref]$result

)

{
	[hashtable]$newtags = $null
	[hashtable]$newNics = $null
	[Microsoft.Azure.Commands.Compute.Automation.Models.PSSnapshot]$oSSnapshot = $null
	[hashtable]$dataSnapShots = $null
	[Microsoft.Azure.Commands.Compute.Automation.Models.PSDisk]$newOsDisk = $null
	[Microsoft.Azure.Commands.Compute.Automation.Models.PSDisk]$newDataDisk = $null
	[Microsoft.Azure.Commands.Compute.Models.PSVirtualMachine]$newVM = $null

	Write-Output ">>>CloneVM..."
	Write-Output ">>>VM: $destVMName will be created."
	Write-Output ">>>Warning: this VM will be an exact copy of the source. Until the Server Name is changed, only one VM can be in operation."


	Write-Output (">>>Source VM: " + $vmSource.Name )

	GetTagsFromVM([ref] $newtags) -vm $vmSource
	
	CreateNewNicsFromVM ([ref] $newNics) `
					-vm $vmSource `
					-destLocation $destLocation `
					-destResourceGroupName $destResourceGroupName `
					-destVMName $destVMName
	
	 CreateOSSnapshotFromVM([ref] $oSSnapshot) `
					-vm $vmSource `
					-destLocation $destLocation `
					-destResourceGroupName $destResourceGroupName `
					-destVMName $destVMName

	 CreateDataSnapshotsFromVM([ref] $dataSnapShots) `
					-vm $vmSource `
					-destLocation $destLocation `
					-destResourceGroupName $destResourceGroupName `
					-destVMName $destVMName

	 CreateDiskFromSnapshot([ref] $newOsDisk) `
					-vm $vmSource `
					-destLocation $destLocation `
					-destResourceGroupName $destResourceGroupName `
					-destVMName $destVMName `
					-snapShot $oSSnapshot `
					-diskSuffix "os"

	$newDataDisks = @{}

	$cnt = 1

	foreach ($dataSnapShot in $dataSnapShots.Values)
	{

		CreateDiskFromSnapshot([ref] $newDataDisk) `
				-vm $vmSource `
				-destLocation $destLocation `
				-destResourceGroupName $destResourceGroupName `
				-destVMName $destVMName `
				-snapShot $dataSnapShot `
				-diskSuffix "data_$cnt" 

		$newDataDisks.Add($newDataDisk.Id, $newDataDisk)

		$cnt++
	}

	CreateNewVM([ref] $newVM) `
					-vm $vmSource `
					-destLocation $destLocation `
					-destResourceGroupName $destResourceGroupName `
					-destVMName $destVMName `
					-oSDisk $newOsDisk `
					-dataDisks $newDataDisks `
					-nics $newNics `
					-tags $newtags

	
	Write-Output ">>>Cleanup (removing snapshots)..."


	Get-AzureRmSnapshot -ResourceGroupName $destResourceGroupName | `
		Remove-AzureRmSnapshot -Force 
	
	Write-Output ">>>     All jobs completed."
	Write-Output ">>>     CloneVM complete."

	$result.Value = $newVM


}

