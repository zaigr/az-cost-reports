using '../main.bicep'

param env = 'dev'
param appConfiguration = loadYamlContent('../configs/dev.yml')
