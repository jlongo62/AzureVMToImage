#
# local_Create_Image_From_VM.ps1
#
#Source VM
param
(
		[string] 
		[Parameter(Mandatory=$false)] 
		$tenantId= "YOUR TENANT ID",

		[string] 
		[Parameter(Mandatory=$false)] 
		$subscriptionId="YOUR SUBSCRIPTION ID",

		[string]
		[Parameter(Mandatory=$false)] 
		$sourceResourceId = '/subscriptions/YOUR SUBSCRIPTION ID/resourceGroups/YOUR RESOURCE GROUP/providers/Microsoft.Compute/virtualMachines/YOUR TARGET VM',

		[string] [Parameter(Mandatory=$false)]
	    $imagesLocation = "eastus", 

		[string] [Parameter(Mandatory=$false)]
		$imagesResourceGroup = "YOUR RESOURCE GROUP TO RECIEVE IMAGE",

		[string] [Parameter(Mandatory=$false)]
		$imagePrefix = "YOUR IMAGE PREFIX",

		[string] [Parameter(Mandatory=$false)]
		$version = "1.0.0"

)
$ErrorActionPreference = "Stop"

Set-StrictMode -Version Latest 

#Includes
."$PSScriptRoot\local_AzureRMAuthenticate.ps1"
."$PSScriptRoot\..\CloneVM.ps1"
."$PSScriptRoot\..\ImageFromVM.ps1"


#Authenticate Azure
[Microsoft.Azure.Commands.Profile.Models.PSAzureContext] $authenticate = $null
AzureRMAuthenticate ([ref]$authenticate ) `
					-TenantId $tenantId `
					-SubscriptionId $subscriptionId   `
					-ErrorAction Stop

Write-Output $authenticate.Name

Write-Host "*** Begin CloneVM: "(Get-Date)

#Generate new Name of Temp VM
$destVMName =  $(New-Guid).Guid

#set the context
Write-Output ">>>Selecting subscription: $subscriptionId" 
Set-AzureRmContext -SubscriptionId $subscriptionId  

#set the source VM	
$resource = Get-AzureRmResource -ResourceId $sourceResourceId
Write-Output (">>>Source VM: " + $resource.Name )
$vmSource = Get-AzureRmVM -Name $resource.Name -ResourceGroupName $resource.ResourceGroupName

Write-Host "*** Begin CloneVM: "(Get-Date)

[Microsoft.Azure.Commands.Compute.Models.PSVirtualMachine] $newVM = $null
CloneVM ([ref] $newVM) `
				-vmSource $vmSource `
				-destLocation $imagesLocation  `
				-destResourceGroupName $imagesResourceGroup `
				-destVMName $destVMName

Write-Host "***Created temp VM: " $newVM.ResourceGroupName ":" $newVM.Name

Write-Host "*** ImageFromVM: "(Get-Date)

[Microsoft.Azure.Commands.Compute.Automation.Models.PSImage]$image = $null
ImageFromVM  ([ref]$image ) `
				-vmSource $newVM `
				-imagesLocation $imagesLocation `
				-imagesResourceGroup  $imagesResourceGroup `
				-imagePrefix $imagePrefix `
				-version $version 


Write-Host "***Created Image: " $image.ResourceGroupName ":" $image.Name
Write-Host "*** End: "(Get-Date)