using System.Net.Http;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using SmartCourt.Providers.ChatModel;
using Xunit;
using Xunit.Abstractions;

namespace SmartCourt.Tests.Providers.ChatModel;

/// <summary>
/// Smoke test that makes a REAL HTTP call to the ITI Student Bedrock Gateway.
/// Requires a valid API key in DeepSeekChatModelOptions.
/// Run with: dotnet test --filter "Category=Smoke"
/// Skip in CI with: dotnet test --filter "Category!=Smoke"
/// </summary>
[Trait("Category", "Smoke")]
public sealed class BedrockGatewaySmokeTest
{
    private readonly ITestOutputHelper _output;

    public BedrockGatewaySmokeTest(ITestOutputHelper output)
    {
        _output = output;
    }

    [Fact]
    public async Task GenerateAsync_WithSimplePrompt_ReturnsNonEmptyResponse()
    {
        // Arrange
        var options = Options.Create(new DeepSeekChatModelOptions
        {
            BaseUrl   = "http://apiaccess.iti.net.eg/api/v1/",
            Model     = "deepseek.r1-v1:0",
            ApiKey    = "sbg_8WeGulirobgK5wPnVJz0S7XGETioi8mu",
            MaxTokens = 500   // R1 has a reasoning chain — needs headroom
        });

        using var httpClient = new HttpClient();
        var provider = new DeepSeekChatModelProvider(
            httpClient,
            options,
            NullLogger<DeepSeekChatModelProvider>.Instance);

        // Act
        var result = await provider.GenerateAsync(
            systemPrompt: "You are a concise assistant. Reply in one sentence.",
            userPrompt:   "What is 2 + 2?");

        // Assert
        _output.WriteLine($"[Bedrock Gateway Response]: {result.Content}");

        Assert.NotNull(result);
        Assert.NotEmpty(result.Content);
    }

    [Fact]
    public async Task GenerateAsync_WithLegalPrompt_ReturnsNonEmptyResponse()
    {
        // Arrange
        var options = Options.Create(new DeepSeekChatModelOptions
        {
            BaseUrl   = "http://apiaccess.iti.net.eg/api/v1/",
            Model     = "deepseek.r1-v1:0",
            ApiKey    = "sbg_8WeGulirobgK5wPnVJz0S7XGETioi8mu",
            MaxTokens = 500   // R1 has a reasoning chain — needs headroom
        });

        using var httpClient = new HttpClient();
        var provider = new DeepSeekChatModelProvider(
            httpClient,
            options,
            NullLogger<DeepSeekChatModelProvider>.Instance);

        // Act
        var result = await provider.GenerateAsync(
            systemPrompt: "You are a legal assistant. Respond in one brief sentence.",
            userPrompt:   "What is a contract?");

        // Assert
        _output.WriteLine($"[Bedrock Gateway Legal Response]: {result.Content}");

        Assert.NotNull(result);
        Assert.NotEmpty(result.Content);
    }
}
