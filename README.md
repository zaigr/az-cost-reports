# Azure Cost Monitoring Telegram Bot

![Build](https://github.com/zaigr/az-cost-reports/actions/workflows/build.yml/badge.svg?branch=main)

An Azure Function timer app periodically calls the Azure API to get the forecasted monthly cost and sends it to a Telegram bot.

### Forecasted cost report

Report executes daily; It sends accumulated actual and final forcasted cost for current billing period:
```
Cost for 24/5/16:
  Actual: 2.57 USD
  Forecast: 6.68 USD
```
> Edit `ForecastReport:Cron` to change the schedule

### Resource groups cost for last billing month

Configure report to execute next day after billing period end by setting `LastBillingPeriodRgReport:Cron`; Report is represented by a pie chart like:
![rg-cost-pie.png](docs/rg-cost-pie.png)

## Run locally

To run the function locally add `SubscriptionId`, `Telegram:ChatId` and `Telegram:Token` to configuration either by adding to `local.settings.json` or using `dotnet-secrets`
(see [how to get Telegram Bot Chat ID and Token](#how-to-get-telegram-bot-chat-id-and-token))
```json
{
  "IsEncrypted": false,
  "Values": {
    ...
    "AzureSubscriptionId": "...",
    ...
    "Telegram:ChatId": "...",
    "Telegram:Token": "..."
  }
}
```

### How to get Telegram Bot Chat ID and Token

1. Obtain your API key from [BotFather](https://core.telegram.org/bots/tutorial)
2. Open this URL in a browser https://api.telegram.org/bot{bot_token}/getUpdates
 - Token is prefixed by `bot`
 - URL will look like `https://api.telegram.org/bot12xxxxxx89:ABCoxxxxxx-xxxxxxdf4h/getUpdates`
3. The response would look like
```json
{
  "ok": true,
  "result": [
    {
      ...
      "message": {
        "message_id": 1234,
        "from": {...},
        "chat": {
          "id": 21xxxxx38,
          ...
        },
        "date": 1703062972,
        "text": "/start"
      }
    }
  ]
}
```
4. Chat ID value is in `result.0.message.chat.id`: `34xxxxx15`

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
