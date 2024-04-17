using '../main.bicep'

param env = 'prd'
param appConfiguration = loadYamlContent('../configs/prd.yml')
