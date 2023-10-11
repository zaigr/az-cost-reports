# Azure Cost Monitoring Telegram Bot
---
Azure Function timer app periodically calling Azure API to get forecasted monthly cost and send it to Telegram bot.

## Deployment

Deploy function infra using Azure Bicep:
```sh
az deployment group create \
  --resource-group rg-costreport-{env}-westeu-01 \
  --template-file main.bicep \
  --parameters params/main.dev.bicepparam
```

TODOs:
- [ ] Setup `nuke` build and add `setup-dev-secrets` command
- [ ] add `bicep` IaC
- [ ] generate tests
- [ ] GitHub pipeline
- [ ] Feature toggle to support multiple chats
- [ ] Stylecop for missing lint rules
 - [ ] unused usings
 - [ ] unnecessary break/whitespace