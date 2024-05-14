using '../main.bicep'

param env = 'dev'
param appConfiguration = loadYamlContent('../configs/dev.yml')
param createAppInsights = true
param resourceGroupName = 'rg-costreport-${env}-westeu-01'
