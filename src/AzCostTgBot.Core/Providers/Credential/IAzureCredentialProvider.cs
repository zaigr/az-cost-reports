using Azure.Core;

namespace AzCostTgBot.Core.Providers.Credential;

public interface IAzureCredentialProvider
{
    Task<AccessToken> GetToken(TokenRequestContext context, CancellationToken cancellation);
}
