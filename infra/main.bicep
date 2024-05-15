targetScope = 'subscription'

param env string
param resourceGroupName string
param appConfiguration object

param appInsightsConnString string = ''
param createAppInsights bool = false

@secure()
param telegramApiKey string = ''

module appInsights 'modules/appInsights.bicep' = if (createAppInsights) {
  name: 'appInsights'
  scope: resourceGroup(resourceGroupName)
  params: {
    env: env
  }
}

module webApps 'modules/webApps.bicep' = {
  name: 'webApps'
  scope: resourceGroup(resourceGroupName)
  params: {
    env: env
    appConfiguration: appConfiguration
    appInsightsConnString: createAppInsights ? appInsights.outputs.connectionString : appInsightsConnString
    telegramApiKey: telegramApiKey
  }
}

module roleAssignments 'modules/roleAssignments.bicep' = {
  name: 'roleAssignments'
  params: {
    functionAppPrincipalId: webApps.outputs.functionAppPrincipalId
  }
}
