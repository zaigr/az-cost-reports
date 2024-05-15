using '../main.bicep'

param env = 'prd'
param appConfiguration = loadYamlContent('../configs/prd.yml')
param resourceGroupName = 'rg-costreport-${env}-westeu-01'
