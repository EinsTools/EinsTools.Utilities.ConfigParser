using Microsoft.Extensions.Configuration;

namespace EinsTools.Utilities.ConfigParser;

public class EinsToolsConfigurationSource : IConfigurationSource
{
    private readonly IConfiguration _configuration;

    public EinsToolsConfigurationSource(IConfiguration configuration) =>
        _configuration = configuration;

    public IConfigurationProvider Build(IConfigurationBuilder builder) =>
        new EinsToolsConfiguration(_configuration);
}

public static class ConfigurationBuilderExtension
{
    public static IConfigurationBuilder AddEinsToolsConfiguration(this IConfigurationBuilder builder,
        IConfiguration configuration) =>
        builder.Add(new EinsToolsConfigurationSource(configuration));
}