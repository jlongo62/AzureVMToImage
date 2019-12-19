#Using credential:
#Using Az in a runbook
#https://ps1code.com/2017/06/28/stop-start-azure-vm-schedule/
#Using AutomationConnection
#https://stackoverflow.com/questions/50531989/azure-runbook-get-a-file-from-azure-file-system-storage

Import-Module Azure
Import-Module AzureRM.Profile

Set-StrictMode -Version Latest 

Write-Output "AzureRMAuthenticate.ps1 loaded."

function RunbookAzureRMAuthenticate
(
   
	[string] 
	[Parameter(Mandatory=$false)] 
	$connectionName = "AzureRunAsConnection",


	[ref]$result

)

{

    Write-Output "servicePrincipalConnection:"
    $servicePrincipalConnection =  Get-AutomationConnection -Name $connectionName 


    Write-Output $servicePrincipalConnection

    Write-Output "Logging in to Azure..."

    $connectionResult =  Connect-AzureRMAccount `
                                -Tenant $servicePrincipalConnection.TenantId `
                                -Subscription $servicePrincipalConnection.SubscriptionId `
                                -ApplicationId $servicePrincipalConnection.ApplicationId `
                                -CertificateThumbprint  $servicePrincipalConnection.CertificateThumbprint `
                                -ServicePrincipal

    $context = Set-AzureRMContext  -Tenant $servicePrincipalConnection.TenantId  -SubscriptionId $servicePrincipalConnection.SubscriptionId  

    if($context.Account -eq $null)
    {
        Write-Output "NOT LOGGED IN!" 
    }
    else
    {
        Write-Output "Logged in." 
    }

    Write-Output "connectionResult:"
    Write-Output $connectionResult

    $result.Value = $servicePrincipalConnection

}

