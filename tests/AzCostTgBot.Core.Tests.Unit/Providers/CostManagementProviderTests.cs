using System.Net;
using AzCostTgBot.Core.Providers;
using AzCostTgBot.Core.Providers.CostManagement;
using AzCostTgBot.Core.Providers.Credential;
using Azure.Core;
using Microsoft.Extensions.Options;
using Moq;
using Moq.Protected;
using Xunit.Sdk;

namespace AzCostTgBot.Core.Tests.Unit.Providers;

public class CostManagementProviderTests
{
    private readonly Mock<HttpMessageHandler> _mockHandler;
    private readonly CostManagementProvider _costManagementProvider;
    private readonly Mock<IAzureCredentialProvider> _mockCredentialProvider;

    public CostManagementProviderTests()
    {
        _mockHandler = new Mock<HttpMessageHandler>(MockBehavior.Strict);
        _mockCredentialProvider = new Mock<IAzureCredentialProvider>();

        Mock<IOptions<AzureConfiguration>> mockOptions = new();
        mockOptions.Setup(o => o.Value).Returns(new AzureConfiguration { SubscriptionId = "test-subscription-id" });

        _costManagementProvider = new CostManagementProvider(new HttpClient(_mockHandler.Object), _mockCredentialProvider.Object, mockOptions.Object);
    }

    [Fact]
    public async Task GetTotalForecast_ValidResponse_ReturnsTotalForecast()
    {
        // Arrange
        var response = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent("""
                                        {
                                            "properties": {
                                                "nextLink": null,
                                                "columns": [
                                                    {
                                                        "name": "Cost",
                                                        "type": "Number"
                                                    },
                                                    {
                                                        "name": "CostStatus",
                                                        "type": "String"
                                                    },
                                                    {
                                                        "name": "Currency",
                                                        "type": "String"
                                                    }
                                                ],
                                                "rows": [
                                                    [
                                                        2.5685266690257307,
                                                        "Actual",
                                                        "USD"
                                                    ],
                                                    [
                                                        4.112466853196475,
                                                        "Forecast",
                                                        "USD"
                                                    ]
                                                ]
                                            },
                                            "id": "subscriptions/test-subscription-id/providers/Microsoft.CostManagement/query/123",
                                            "name": "123",
                                            "type": "Microsoft.CostManagement/query",
                                            "location": null,
                                            "eTag": null
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
        var result = await _costManagementProvider.GetTotalForecast(new DateOnly(2024, 7, 12), new DateOnly(2024, 8, 11));

        // Assert
        Assert.Equal("USD", result.Currency);
        Assert.Equal(2.5685266690257307m, result.Actual);
        Assert.Equal(4.112466853196475m, result.Forecast);
    }

    [Fact]
    public async Task GetRgLastBillingPeriodCost_ValidResponse_ReturnsResourceCosts()
    {
        // Arrange
        var response = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent("""
                                        {
                                            "id": "subscriptions/test-subscription-id/providers/Microsoft.CostManagement/query/123",
                                            "name": "123",
                                            "type": "Microsoft.CostManagement/query",
                                            "location": null,
                                            "sku": null,
                                            "eTag": null,
                                            "properties": {
                                                "nextLink": null,
                                                "columns": [
                                                    {
                                                        "name": "Cost",
                                                        "type": "Number"
                                                    },
                                                    {
                                                        "name": "ResourceGroup",
                                                        "type": "String"
                                                    },
                                                    {
                                                        "name": "Currency",
                                                        "type": "String"
                                                    }
                                                ],
                                                "rows": [
                                                    [
                                                        0.0024860048,
                                                        "test-1",
                                                        "USD"
                                                    ],
                                                    [
                                                        0.036797080246541,
                                                        "test-2",
                                                        "USD"
                                                    ],
                                                    [
                                                        6.38129861675489,
                                                        "test-3",
                                                        "USD"
                                                    ],
                                                    [
                                                        4,
                                                        "test-4",
                                                        "USD"
                                                    ]
                                                ]
                                            }
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
        var results = await _costManagementProvider.GetRgLastBillingPeriodCost();

        // Assert
        Assert.Equal(4, results.Count);
        foreach (var result in results)
        {
            var expectedCost = result.Name switch
            {
                "test-1" => 0.0024860048m,
                "test-2" => 0.036797080246541m,
                "test-3" => 6.38129861675489m,
                "test-4" => 4m,
                _ => throw FailException.ForFailure("Unknown resource group name"),
            };
            Assert.Equal(expectedCost, result.Cost);
        }
    }
}
