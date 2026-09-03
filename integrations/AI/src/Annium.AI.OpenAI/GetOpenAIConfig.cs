using System;

namespace Annium.AI.OpenAI;

/// <summary>
/// Resolves the settings of an OpenAI client from the provider, deferring the lookup to resolution time so
/// that configuration loaded after registration (or rotated secrets) is still picked up.
/// </summary>
/// <param name="sp">The provider to resolve the settings from.</param>
/// <returns>The settings of the client.</returns>
// ReSharper disable once InconsistentNaming
public delegate OpenAIConfig GetOpenAIConfig(IServiceProvider sp);
