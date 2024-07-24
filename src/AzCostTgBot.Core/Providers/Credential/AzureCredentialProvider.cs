using Azure.Core;
using Azure.Identity;

namespace AzCostTgBot.Core.Providers.Credential;

public class AzureCredentialProvider : IAzureCredentialProvider
{
    private readonly TokenCredential _tokenCredential = new DefaultAzureCredential();

    public async Task<AccessToken> GetToken(TokenRequestContext context, CancellationToken cancellation)
    {
        return await _tokenCredential.GetTokenAsync(context, cancellationToken: cancellation);
    }
}
