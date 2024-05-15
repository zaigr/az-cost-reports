param env string
param appConfiguration object
param appInsightsConnString string

@allowed([
  'Standard_LRS'
  'Standard_GRS'
  'Standard_RAGRS'
])
param storageAccountType string = 'Standard_LRS'

@secure()
param telegramApiKey string = ''

param location string = resourceGroup().location

resource storageAccount 'Microsoft.Storage/storageAccounts@2023-01-01' = {
  name: 'stafunc${env}${location}01'
  location: location
  kind: 'StorageV2'
  sku: {
    name: storageAccountType
  }
}

resource hostingPlan 'Microsoft.Web/serverfarms@2022-09-01' = {
  name: 'asp-costreport-${env}-${location}-01'
  location: location
  sku: {
    name: 'Y1'
    tier: 'Dynamic'
  }
  properties: {}
}

var appSettingsDefinition = map(items(appConfiguration), pair => {
  name: pair.key
  value: pair.value
})

resource functionApp 'Microsoft.Web/sites@2022-09-01' = {
  name: 'func-costreport-${env}-${location}-01'
  location: location
  kind: 'functionapp'
  identity: {
    type: 'SystemAssigned'
  }
  properties: {
    serverFarmId: hostingPlan.id
    siteConfig: {
      appSettings: concat([
        {
          name: 'Telegram:Token'
          value: telegramApiKey
        }
        {
          name: 'AzureWebJobsStorage'
          value: 'DefaultEndpointsProtocol=https;AccountName=${storageAccount.name};AccountKey=${storageAccount.listKeys().keys[0].value}'
        }
        {
          name: 'APPLICATIONINSIGHTS_CONNECTION_STRING'
          value: appInsightsConnString
        }
        {
          name: 'FUNCTIONS_EXTENSION_VERSION'
          value: '~4'
        }
        {
          name: 'FUNCTIONS_WORKER_RUNTIME'
          value: 'dotnet'
        }
      ], appSettingsDefinition)
    }
  }
}

output functionAppPrincipalId string = functionApp.identity.principalId
