Import-Module AzureRM.Compute

Set-StrictMode -Version Latest 

Write-Output "ImageFromVM.ps1 Loaded"

# http://xpertkb.com/compute-hash-string-powershell/


function Get-StringHash([String] $String, $HashName = "MD5") 
{ 
	$StringBuilder = New-Object System.Text.StringBuilder 
	[System.Security.Cryptography.HashAlgorithm]::Create($HashName).ComputeHash([System.Text.Encoding]::UTF8.GetBytes($String))|%{ 
	[Void]$StringBuilder.Append($_.ToString("x2")) 
	} 
	$StringBuilder.ToString() 
}

function GetTagsFromVM
(
	[Microsoft.Azure.Commands.Compute.Models.PSVirtualMachine]
	[Parameter(Mandatory=$true)] 
	$vm,

	[OutputType([hashtable])]
	[CmdletBinding()]
	[ref]$result
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

function GetBlobUri
(
	
	[string] [Parameter(Mandatory=$true)]
	$location,
		
	[string] [Parameter(Mandatory=$true)]
	$resourceGroupName,

	[OutputType([string])]
	[CmdletBinding()]
	[ref]$result

)
{

	Write-Output  ">>>GetBlobUri..."

	$fileName =  "sysprep.ps1"
	$containerName = "scripts"


	$resource = Get-AzureRmResourceGroup -Location $location -Name $resourceGroupName 

	$storageAccountName =  Get-StringHash -String $resource.ResourceId

	$storageAccountName = $storageAccountName.ToLower().Substring(0,23)

	Write-Output  ">>>     Looking for storageAccount: $storageAccountName ..."

	$storageAccountExist = Get-AzureRmStorageAccount -ResourceGroupName $resourceGroupName -Name $storageAccountName -ErrorAction SilentlyContinue

	if ($storageAccountExist  -eq $null)
	{  

		Write-Output  ">>>     StorageAccount $storageAccountName not found. Creating..."

		Write-Output ">>>     Creating Storage Account $storageAccountName"
		$storageAccount = New-AzureRmStorageAccount -ResourceGroupName $resourceGroupName `
								  -Location $location `
								  -StorageAccountName $storageAccountName `
								  -Type Standard_LRS

		$container = New-AzureRmStorageContainer -ResourceGroupName $resourceGroupName `
												 -StorageAccountName $storageAccountName `
												 -Name $containerName `
												 -PublicAccess None 

		Write-Output  ">>>     Create 'sysprep.ps1' File: " + $fileName
		$content = Set-Content -Path $fileName `
							   -Value "Start-Process -FilePath C:\Windows\System32\Sysprep\Sysprep.exe -ArgumentList '/generalize /oobe /quiet /quit'  -Wait" 

		Write-Output  ">>>     Create Blob: $fileName"
		$storageAccountKey  = (Get-AzureRmStorageAccountKey -ResourceGroupName $resourceGroupName -Name $storageAccountName).Value[0]
		$storagecontext = New-AzureStorageContext -StorageAccountName $storageAccountName -StorageAccountKey $storageAccountKey  
		$blob = Set-AzureStorageBlobContent -Context $storagecontext `
											-Container $containerName `
											-Blob $fileName `
											-File $fileName
	}

	Write-Output  ">>>     Fetching Blob SAS Uri: $storageAccountName, $containerName, $fileName"
	$storageAccountKey  = (Get-AzureRmStorageAccountKey -ResourceGroupName $resourceGroupName -Name $storageAccountName).Value[0]
	$storagecontext = New-AzureStorageContext -StorageAccountName $storageAccountName -StorageAccountKey $storageAccountKey  

	#Blob SAS Url
	# https://c7b87d22144b3343738b71e.blob.core.windows.net/scripts/sysprep.ps1?sp=r&st=2019-12-09T18:22:30Z&se=2019-12-10T02:22:30Z&spr=https&sv=2019-02-02&sr=b&sig=MbIYVE5p%2FF%2FMFBVl0WMC2QgwatRL78R%2Bh%2Bbw%2FODDNJg%3D
	$startTime = Get-Date
	$endTime = $startTime.AddHours(2.0)
	$uri = New-AzureStorageBlobSASToken -Context $storagecontext `
										  -Container $containerName `
										  -Blob $fileName `
										  -Permission r `
										  -FullUri:$true `
										  -StartTime $startTime -ExpiryTime $endTime
	Write-Output  ">>>     Blob SAS Uri: $uri"

	$result.Value = $uri
}

function RunSysPrep
(
	[Microsoft.Azure.Commands.Compute.Models.PSVirtualMachine]
	[Parameter(Mandatory=$true)] 
	$vm,
	
	[string] [Parameter(Mandatory=$true)]
	$location,
		
	[string] [Parameter(Mandatory=$true)]
	$resourceGroupName,

	[string] [Parameter(Mandatory=$true)]
	$imagePrefix,

	[string] [Parameter(Mandatory=$true)]
	$blobUri,

	[OutputType([Microsoft.Azure.Commands.Compute.Models.PSVirtualMachineExtension])]
	[CmdletBinding()]
	[ref]$result

)	
{

	##################################################################################################################
	# Extension Notes:
	# use blob storage, file storage does not support Access Level
	# must set blobs read permissions or provide SAS key
	# https://docs.microsoft.com/en-us/azure/storage/blobs/storage-manage-access-to-resources
	# https://autofx.blob.core.windows.net/scripts/SysPrep.ps1
	# Script Extension Logs:
	#	C:\WindowsAzure\Logs\Plugins\Microsoft.Compute.CustomScriptExtension\1.9.3
	# Scripts are Downloded to: 
	#	C:\Packages\Plugins\Microsoft.Compute.CustomScriptExtension\1.9.3\Downloads
	$FileName ='SysPrep.ps1'
	$ExtensionName = 'runsysprep'

	#Can take a LONG time 12+ minutes
	#extension must not keep process open or it hangs: /quit
	#extension should not shut down vm (removed shutdown)
	#
	#Start-Process -FilePath C:\Windows\System32\Sysprep\Sysprep.exe 
	#			   -ArgumentList '/generalize /oobe /quiet /quit'  
	#			   -Wait 

	Write-Output  (">>>Set-AzureRmVMCustomScriptExtension : " + $ExtensionName)
	Write-Output  ">>>    This can take a LONG time, up to 12+ minutes"
	Write-Output  ">>>    VM will run Sysprep.exe /generalize /oobe /quiet /quit"
	Write-Output  ">>>    Upon completion VM will shutdown in preparation for imaging."

	$response = Set-AzureRmVMCustomScriptExtension  `
					-ResourceGroupName  $vm.ResourceGroupName `
					-VMName $vm.Name `
					-Location $vm.Location `
					-Name $ExtensionName  `
					-FileUri $blobUri  `
					-Run $FileName 

	$extStatus = Get-AzureRmVMDiagnosticsExtension `
					-ResourceGroupName $resourceGroupName  `
					-VMName  $vm.Name `
					-Name $ExtensionName `
					-Status

	Write-Output (">>>     " + $extStatus.ProvisioningState + " complete.")

	$result.Value = $extStatus
}

function MakeImageFromVM
(

	[Microsoft.Azure.Commands.Compute.Models.PSVirtualMachine]
	[Parameter(Mandatory=$true)]  $vm,

	[string] [Parameter(Mandatory=$true)] $imagesLocation,

	[string] [Parameter(Mandatory=$true)] $imagesResourceGroup,

	[string] [Parameter(Mandatory=$true)] $imagePrefix,

	[string] [Parameter(Mandatory=$true)] $version,

	[hashtable]	[Parameter(Mandatory=$true)] $tags,

	[OutputType([Microsoft.Azure.Commands.Compute.Automation.Models.PSImage])]
	[CmdletBinding()]
	[ref]$result

)
{

	Write-Output ">>>MakeImageFromVM"
	Write-Output ">>>     Deallocate target VM..."
	
	$stopVM = Stop-AzureRMVM -ResourceGroupName $vm.ResourceGroupName -Name $vm.Name -Force

	$setVM = Set-AzureRMVM -ResourceGroupName $vm.ResourceGroupName -Name $vm.Name -Generalized

	#The 'Image' name must begin with a letter or number, end with a letter, number or underscore, 
	#and may contain only letters, numbers, underscores, periods, or hyphens.
	$Date = Get-Date -Format 'yyyy_MM_dd_HH_mm_ss'
	$imageName = $imagePrefix + '_' + $version + '_' + $Date
	
	Write-Output  ">>>     Creating image: $imageName"
	$imageConfig = New-AzureRMImageConfig   -Location $imagesLocation  `
											-SourceVirtualMachineId $vm.Id 

	$image = New-AzureRMImage -ResourceGroupName $imagesResourceGroup -Image $imageConfig -ImageName $imageName

	$tags.Add("Created", (Get-Date -Format "dddd MM/dd/yyyy HH:mm K") )
	$setTags = Set-AzureRMResource -ResourceGroupName $imagesResourceGroup `
						-Name $imageName `
						-ResourceType "Microsoft.Compute/images" `
						-Tag $tags `
						-Force
						

	Write-Output ">>>     New VM Image Complete: $imageName"

	$result.Value = $image
}	

function ImageFromVM
(
	[Microsoft.Azure.Commands.Compute.Models.PSVirtualMachine]
	[Parameter(Mandatory=$true)] $vmSource,

	[string] [Parameter(Mandatory=$true)] $imagesLocation,

	[string] [Parameter(Mandatory=$true)] $imagesResourceGroup,

	[string] [Parameter(Mandatory=$true)] $imagePrefix,

	[string] [Parameter(Mandatory=$true)] $version,

	[OutputType([Microsoft.Azure.Commands.Compute.Automation.Models.PSImage])]
	[CmdletBinding()]
	[ref]$result

)
{
	#region
	Write-Output ">>>ImageFromVM..."

	Write-Output ">>>     Warning: Creating a machine image will delete the source VM"
	Write-Output ">>>     Warning: Data disks will be mapped before DVD drives. This may impact drive letter mappings. "
	Write-Output ">>>              Ensure DVD drive of target VM uses the last letter. ie E:(Disk 1), F:(Disk 2), G:(DVD)"

	Write-Output (">>>     Source VM: " + $vmSource.Name)
	#endregion

	[hashtable]$tags = $null
	[string]$blobUri = $null
	[Microsoft.Azure.Commands.Compute.Models.PSVirtualMachineExtension] $status = $null
	[Microsoft.Azure.Commands.Compute.Automation.Models.PSImage] $image = $null

	GetTagsFromVM([ref] $tags) -vm $vmSource
	
	GetBlobUri([ref] $blobUri) `
				-location $imagesLocation `
				-resourceGroupName $imagesResourceGroup 

	RunSysPrep([ref] $status) `
				-vm $vmSource `
				-location $imagesLocation `
				-resourceGroupName $imagesResourceGroup `
				-imagePrefix $imagePrefix `
				-blobUri $blobUri
						 

	MakeImageFromVM([ref] $image) `
				-vm $vmSource `
				-imagesLocation $imagesLocation `
				-imagesResourceGroup $imagesResourceGroup `
				-imagePrefix $imagePrefix `
				-version $version `
				-tags $tags 

	Write-Output (">>>     Removing VM: " + $vmSource.ResourceGroupName + ":" + $vmSource.Name)
	$removeVM = Remove-AzureRmVM -ResourceGroupName  $vmSource.ResourceGroupName -Name  $vmSource.Name -Force 

	$resource = Get-AzureRmResource -ResourceId $vmSource.StorageProfile.OsDisk.ManagedDisk.Id
	$removeDisk = Remove-AzureRmDisk -ResourceGroupName  $resource.ResourceGroupName -Name  $resource.Name -Force 
	
	foreach($disk in  $vmSource.StorageProfile.DataDisks)
	{
		$resource = Get-AzureRmResource -ResourceId $disk.ManagedDisk.Id
		$removeDisk = Remove-AzureRmDisk -ResourceGroupName  $resource.ResourceGroupName -Name  $resource.Name -Force 
	}

	foreach($nic in  $vmSource.NetworkProfile.NetworkInterfaces)
	{
		$resource = Get-AzureRmResource -ResourceId $nic.Id
		$removeNIC = Remove-AzureRmNetworkInterface -ResourceGroupName  $resource.ResourceGroupName -Name  $resource.Name -Force 
	}

	Write-Output ( ">>>ImageFromVM: " + $image.Name + " Complete.")

	$result.Value = $image

}

