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
		$resourceId = '/subscriptions/YOUR SUBSCRIPTION ID/resourceGroups/YOUR RESOURCE GROUP TO RECIEVE IMAGE/providers/Microsoft.Compute/virtualMachines/YOUR TARGET VM',

		[string] [Parameter(Mandatory=$false)]
	    $imagesLocation = "eastus", 

		[string] [Parameter(Mandatory=$false)]
		$imagesResourceGroup = "YOUR RESOURCE GROUP TO RECIEVE IMAGE",

		[string] [Parameter(Mandatory=$false)]
		$imagePrefix = "YOUR IMAGE PREFIX",

		[string] [Parameter(Mandatory=$false)]
		$version = "1.0.0"


)

Set-StrictMode -Version Latest 

#Includes
."$PSScriptRoot\local_AzureRMAuthenticate.ps1"
."$PSScriptRoot\..\ImageFromVM.ps1"


#Authenticate Azure
[Microsoft.Azure.Commands.Profile.Models.PSAzureContext] $authenticate = $null
AzureRMAuthenticate ([ref]$authenticate ) -TenantId $tenantId -SubscriptionId $subscriptionId  
Write-Output $authenticate.Name

$destVMName =  $(New-Guid).Guid

Write-Output ">>>Selecting subscription: $subscriptionId" 
Set-AzureRmContext -SubscriptionId $subscriptionId  
	
$resource = Get-AzureRmResource -ResourceId $resourceId
Write-Output (">>>Source VM: " + $resource.Name )
$vmSource = Get-AzureRmVM -Name $resource.Name -ResourceGroupName $resource.ResourceGroupName


#perform work
Write-Output  "ImageFromVM..."
[Microsoft.Azure.Commands.Compute.Automation.Models.PSImage]$image = $null
ImageFromVM  ([ref]$image ) `
					-vmSource $vmSource `
					-imagesLocation $imagesLocation `
					-imagesResourceGroup  $imagesResourceGroup `
					-imagePrefix $imagePrefix `
					-version $version


Write-Output $image.Name

Write-Output $image.Name
