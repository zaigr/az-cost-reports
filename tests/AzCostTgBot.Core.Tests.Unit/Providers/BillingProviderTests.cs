using System.Net;
using AzCostTgBot.Core.Providers;
using AzCostTgBot.Core.Providers.Billing;
using AzCostTgBot.Core.Providers.Credential;
using Azure.Core;
using Microsoft.Extensions.Options;
using Moq;
using Moq.Protected;

namespace AzCostTgBot.Core.Tests.Unit.Providers;

public class BillingProviderTests
{
    private readonly Mock<HttpMessageHandler> _mockHandler;
    private readonly BillingProvider _billingProvider;
    private readonly Mock<IAzureCredentialProvider> _mockCredentialProvider;

    public BillingProviderTests()
    {
        _mockHandler = new Mock<HttpMessageHandler>(MockBehavior.Strict);
        _mockCredentialProvider = new Mock<IAzureCredentialProvider>();
        Mock<IOptions<AzureConfiguration>> mockOptions = new();
        mockOptions.Setup(o => o.Value).Returns(new AzureConfiguration { SubscriptionId = "test-subscription-id" });
        _billingProvider = new BillingProvider(new HttpClient(_mockHandler.Object), _mockCredentialProvider.Object, mockOptions.Object);
    }

    [Fact]
    public async Task GetBillingPeriod_ValidResponse_ReturnsBillingPeriod()
    {
        // Arrange
        var response = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent("""
                                        {
                                            "id": "/subscriptions/test-subscription-id/providers/Microsoft.Billing/billingPeriods/202405-1",
                                            "name": "202405-1",
                                            "properties": {
                                                "billingPeriodEndDate": "2024-04-11",
                                                "billingPeriodStartDate": "2024-03-12"
                                            },
                                            "type": "Microsoft.Billing/BillingPeriods"
                                        }
                                        """),
        };

        _mockHandler.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(response);

        _mockCredentialProvider.Setup(m => m.GetToken(It.IsAny<TokenRequestContext>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new AccessToken("mock-token", DateTimeOffset.Now.AddHours(1)));

        // Act
        var result = await _billingProvider.GetBillingPeriod(2022, 5);

        // Assert
        Assert.Equal("202405-1", result!.Name);
        Assert.Equal(new DateOnly(2024, 3, 12), result.Start);
        Assert.Equal(new DateOnly(2024, 4, 11), result.End);
    }

    [Fact]
    public async Task GetBillingPeriod_InvalidResponse_ThrowsException()
    {
        // Arrange
        var response = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent("""
                                        {
                                            "id": "/subscriptions/test-subscription-id/providers/Microsoft.Billing/billingPeriods/202201-1",
                                            "name": "202201-1",
                                            "properties": {},
                                            "type": "Microsoft.Billing/BillingPeriods"
                                        }
                                        """),
        };

        _mockHandler.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(response);

        _mockCredentialProvider.Setup(m => m.GetToken(It.IsAny<TokenRequestContext>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new AccessToken("mock-token", DateTimeOffset.Now.AddHours(1)));

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => _billingProvider.GetBillingPeriod(2022, 13));

        Assert.Equal("Invalid billing period response.", exception.Message);
    }
}
