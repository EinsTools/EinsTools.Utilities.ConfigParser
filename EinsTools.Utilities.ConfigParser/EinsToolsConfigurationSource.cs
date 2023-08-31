using Microsoft.Extensions.Configuration;

namespace EinsTools.Utilities.ConfigParser;

/// <summary>
/// The configuration source for EinsTools.
/// </summary>
public class EinsToolsConfigurationSource : IConfigurationSource
{
    private readonly IConfiguration _configuration;

    /// <summary>
    /// Constructs an object of <see cref="EinsToolsConfigurationSource"/>. You should not use this constructor directly.
    /// Use the AddEinsToolsConfiguration extension method instead.
    /// </summary>
    /// <param name="configuration">The basic configuration</param>
    public EinsToolsConfigurationSource(IConfiguration configuration) =>
        _configuration = configuration;

    /// <inheritdoc cref="IConfigurationSource.Build"/>
    public IConfigurationProvider Build(IConfigurationBuilder builder) =>
        new EinsToolsConfiguration(_configuration);
}

/// <summary>
/// Extension methods for <see cref="IConfigurationBuilder"/>.
/// </summary>
public static class ConfigurationBuilderExtension
{
    /// <summary>
    /// Adds a new EinsTools configuration source to the configuration builder.
    /// </summary>
    public static IConfigurationBuilder AddEinsToolsConfiguration(this IConfigurationBuilder builder,
        IConfiguration configuration) =>
        builder.Add(new EinsToolsConfigurationSource(configuration));
}