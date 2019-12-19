Import-Module AzureRM.Profile

Set-StrictMode -Version Latest 
Write-Output "local_AzureRmAuthenticate.ps1 loaded"

function AzureRmAuthenticate
(
		[string] 
		[Parameter(Mandatory=$false)] 
		$TenantId,


		[string] 
		[Parameter(Mandatory=$false)] 
		$SubscriptionIdl,
	
		[OutputType([Microsoft.Azure.Commands.Profile.Models.PSAzureContext])]
		[CmdletBinding()]
		[ref]$result
)
{

	try
	{
		#Try, if  Set-AzureRmContext Fails
		#az account clear
		#Disconnect-AzureRmAccount # -Username "YOUR AZURE USER NAME"

		#Disable-AzureRmContextAutosave
		
		#Connect-AzureRmAccount -Tenant $TenantId -Subscription $SubscriptionId

		#Enable-AzureRmContextAutosave 

		Write-Output "Logging in to Azure..." 

		$context = Set-AzureRMContext  -Tenant $TenantId  -SubscriptionId $SubscriptionId  

		if($context.Account -eq $null)
		{
			Write-Output "NOT LOGGED IN!" 
		}
		else
		{
			Write-Output "Logged in." 
		}

		$vms = Get-AzureRmVM
		Write-Output ("VMs: " + $vms.Count)

		
		$result.Value = $context
	}
	catch {
			Write-Error -Message $_.Exception
			throw $_.Exception
	}

}