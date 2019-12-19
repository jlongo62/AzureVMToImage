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
		$resourceGroupName = "YOUR AUTOMATION RESOURCE GROUP",

		[string] [Parameter(Mandatory=$false)]
	    $automationAccountName = "automation" 

)


Set-StrictMode -Version Latest 

#Includes
."$PSScriptRoot\local_AzureRMAuthenticate.ps1"

#Authenticate Azure
[Microsoft.Azure.Commands.Profile.Models.PSAzureContext] $authenticate = $null
AzureRMAuthenticate ([ref]$authenticate ) -TenantId $tenantId -SubscriptionId $subscriptionId  
Write-Output $authenticate.Name

$path = "$PSScriptRoot\..\"

$files = Get-ChildItem -Path $path -Filter *.ps1


function ImportAndPublishRunbook
(
	[Parameter(Mandatory=$true)] 
	$file
)
{

	Write-Output "Publishing $file"

	Import-AzureRmAutomationRunbook -Path $file.FullName `
									-ResourceGroup $resourceGroupName `
									-AutomationAccountName $automationAccountName `
									-Type Powershell `
									-Force

	$name = $file.Name.Split(".")[0]
	Publish-AzureRmAutomationRunbook -ResourceGroup $resourceGroupName `
									 -AutomationAccountName $automationAccountName `
									 -Name $name
}


foreach($file in $files)
{

	ImportAndPublishRunbook $file

}

Write-Output "Publishing Complete."
