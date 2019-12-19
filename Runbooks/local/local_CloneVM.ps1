#
# local_CloneVM.ps1
#
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
		$resourceId = '/subscriptions/YOUR SUBSCRIPTION ID/resourceGroups/YOUR RESOURCE GROUP/providers/Microsoft.Compute/virtualMachines/YOUR TARGET VM',

		[string] [Parameter(Mandatory=$false)]
	    $destLocation = "eastus", 

		[string] [Parameter(Mandatory=$false)]
	    $destResourceGroupName = "YOUR RESOURCE GROUP TO RECIEVE IMAGE"

)


Set-StrictMode -Version Latest 

#Includes
."$PSScriptRoot\local_AzureRMAuthenticate.ps1"
."$PSScriptRoot\..\CloneVM.ps1"


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
[Microsoft.Azure.Commands.Compute.Models.PSVirtualMachine] $newVM = $null
CloneVM ([ref] $newVM) `
	-vmSource $vmSource `
	-destLocation $destLocation  `
	-destResourceGroupName $destResourceGroupName `
	-destVMName $destVMName 
	
Write-Output $newVM.Name
