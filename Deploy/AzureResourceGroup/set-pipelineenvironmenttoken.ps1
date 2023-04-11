param (
    $ResourceGroupName,
    $SiteName
)

$RestMethodParameters = @{
    SubscriptionId = ((get-azcontext).subscription.id)
    ResourceGroupName = $ResourceGroupName
    ResourceProviderName = 'Microsoft.Web'
    ResourceType = 'staticSites'
    Name = "${SiteName}/listsecrets"
    ApiVersion = '2019-08-01'
    Method = 'POST'
}

write-host "RM" ($RestMethodParameters | Format-Table | Out-String)

$ApiKey = Invoke-AzRestMethod @RestMethodParameters | 
    Select-Object -ExpandProperty Content | 
    ConvertFrom-Json | 
    Select-Object -ExpandProperty properties | 
    Select-Object -ExpandProperty ApiKey

Write-Host "##vso[task.setvariable variable=token;issecret=true]$ApiKey"



