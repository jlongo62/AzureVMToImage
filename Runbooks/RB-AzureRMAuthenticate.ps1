
Set-StrictMode -Version Latest 

#Includes (need to use two dots in Azure runbooks)
. .\AzureRMAuthenticate.ps1

Write-Output "Invoke RunbookAzureRMAuthenticate..."

$servicePrincipalConnection = $null
RunbookAzureRMAuthenticate([ref] $servicePrincipalConnection) -connectionName "AzureRunAsConnection"

Write-Output "servicePrincipalConnection:"
Write-Output $servicePrincipalConnection

