#Dependencies found in "Automation Account\Modules Gallery"
#Name					Version
#------------------		-------
#AzureRM.Automation		6.1.1
#AzureRM.Compute		5.9.1
#AzureRM.Network		6.11.1
#AzureRM.profile		5.8.3
#AzureRM.Resources		6.7.3
#AzureRM.Sql			4.12.1
#AzureRM.Storage		5.2.0

param
(

		[string]
		[Parameter(Mandatory=$false)] 
		$sourceResourceId = '/subscriptions/YOUR SUBSCRIPTION/resourceGroups/YOUR RESOURCE GROUP/providers/Microsoft.Compute/virtualMachines/YOUR TARGET VM',

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

#Includes (need to use two dots in Azure runbooks)
. .\AzureRMAuthenticate.ps1
. .\CloneVM.ps1
. .\ImageFromVM.ps1


#Authenticate using Service Principal
$servicePrincipalConnection = $null
RunbookAzureRMAuthenticate([ref] $servicePrincipalConnection) `
					-connectionName "AzureRunAsConnection" `
					-ErrorAction Stop
							

Write-Output "servicePrincipalConnection:"
Write-Output $servicePrincipalConnection

Write-Output "*** Begin CloneVM: "(Get-Date)

#Generate new Name of Temp VM
$destVMName =  $(New-Guid).Guid

#set the context
Write-Output ("***Selecting subscription: " + $servicePrincipalConnection.SubscriptionId)
Set-AzureRmContext -SubscriptionId $servicePrincipalConnection.SubscriptionId  

#set the source VM	
$resource = Get-AzureRmResource -ResourceId $sourceResourceId
Write-Output ("***Source VM: " + $resource.Name )
$vmSource = Get-AzureRmVM -Name $resource.Name -ResourceGroupName $resource.ResourceGroupName

Write-Output "*** Begin CloneVM: "(Get-Date)

[Microsoft.Azure.Commands.Compute.Models.PSVirtualMachine] $newVM = $null
CloneVM ([ref] $newVM) `
	-vmSource $vmSource `
	-destLocation $imagesLocation  `
	-destResourceGroupName $imagesResourceGroup `
	-destVMName $destVMName  

Write-Output "***Created temp VM: " $newVM.ResourceGroupName ":" $newVM.Name

Write-Output "*** ImageFromVM: "(Get-Date)

[Microsoft.Azure.Commands.Compute.Automation.Models.PSImage]$image = $null
ImageFromVM  ([ref]$image ) `
				-vmSource $newVM `
                -imagesLocation $imagesLocation `
                -imagesResourceGroup  $imagesResourceGroup `
                -imagePrefix $imagePrefix `
                -version $version 


Write-Output "***Created Image: " $image.ResourceGroupName ":" $image.Name
Write-Output "*** End: "(Get-Date)
Write-Output "----------------------------"
Write-Output "----------------------------"
Write-Output "----------------------------"