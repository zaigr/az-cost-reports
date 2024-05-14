param functionAppPrincipalId string

targetScope = 'subscription'

@description('Built-in Billing Reader role. See https://learn.microsoft.com/en-us/azure/role-based-access-control/built-in-roles/management-and-governance#billing-reader')
resource billingReaderRole 'Microsoft.Authorization/roleDefinitions@2022-05-01-preview' existing = {
  scope: subscription()
  name: 'fa23ad8b-c56e-40d8-ac0c-ce449e1d2c64'
}

resource billingReaderAssignment 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
  name: guid(functionAppPrincipalId, billingReaderRole.id)
  properties: {
    principalId: functionAppPrincipalId
    principalType: 'ServicePrincipal'
    roleDefinitionId: billingReaderRole.id
  }
}
