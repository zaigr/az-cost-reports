# Azure Cost Monitoring Telegram Bot

Azure Function timer app periodically calling Azure API to get forecasted monthly cost and send it to Telegram bot.

## Deployment

Deploy function infra using Azure Bicep:
```sh
az deployment sub create \
  --location <location-code> \
  --template-file main.bicep \
  --parameters params/main.{dev|prd}.bicepparam \
  --parameters resourceGroupName={rg_name} telegramApiKey={api_key} appInsightsConnString=<optional>
```

Deployment should happen on subscription scope in order to create `Billing.Reader` role assignment;


# Appendix

TODOs:
- [ ] Setup `nuke` build and add `setup-dev-secrets` command
- [x] add `bicep` IaC
- [ ] generate tests
- [ ] GitHub pipeline
- [ ] Feature toggle to support multiple chats
- [ ] Stylecop for missing lint rules
 - [ ] unused usings
 - [ ] unnecessary break/whitespace