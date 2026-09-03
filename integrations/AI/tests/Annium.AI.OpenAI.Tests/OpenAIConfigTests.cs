using Annium.Testing;
using Xunit;

namespace Annium.AI.OpenAI.Tests;

/// <summary>
/// Tests that the API key never leaves the config through its textual representation.
/// </summary>
public class OpenAIConfigTests
{
    /// <summary>
    /// ToString redacts the key: a record prints every property by default, so logging or reporting the
    /// config in an exception message would otherwise leak the credential.
    /// </summary>
    [Fact]
    public void ToString_RedactsKey()
    {
        // arrange
        var config = new OpenAIConfig("sk-secret-value", "gpt-5", null);

        // act
        var text = config.ToString();

        // assert
        text.Contains("sk-secret-value").IsFalse("API key must not be rendered");
        text.Contains("gpt-5").IsTrue("model is not a secret and stays visible");
        // the contract is redaction, not omission: dropping the field entirely would satisfy the two
        // assertions above while losing the signal that a key is configured at all
        text.Contains($"{nameof(OpenAIConfig.Key)} = ***").IsTrue("the key must be shown as redacted");
    }

    /// <summary>
    /// String interpolation goes through the same redacted representation.
    /// </summary>
    [Fact]
    public void Interpolation_RedactsKey()
    {
        // arrange
        var config = new OpenAIConfig("sk-secret-value", "gpt-5", null);

        // act
        var text = $"resolved {config}";

        // assert
        text.Contains("sk-secret-value").IsFalse("API key must not be rendered");
    }
}
