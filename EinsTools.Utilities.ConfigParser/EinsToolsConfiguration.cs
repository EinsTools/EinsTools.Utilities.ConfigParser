using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using System.Text.Json;
using EinsTools.Utilities.StringHelper;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Primitives;

namespace EinsTools.Utilities.ConfigParser;

public class EinsToolsConfiguration : IConfigurationProvider
{
    private readonly IConfiguration _config;

    public EinsToolsConfiguration(IConfiguration config)
    {
        _config = config;
    }
    
    public bool TryGet(string key, out string? value)
    {
        value = TranslateString(key, _config[key]);
        return value != null;
    }

    
    [return: NotNullIfNotNull("str")]
    private string? TranslateString(string key, string? str)
    {
        if (str == null)
        {
            return null;
        }

        var values = StringSplitter.Split(str);
        var sb = new StringBuilder();
        foreach (var value in values)
        {
            switch (value)
            {
                case StringLiteral literal:
                    sb.Append(literal.Value);
                    break;
                case ScriptLiteral script:
                    ProcessScript(key, script, sb);
                    break;
            }
        }
        return sb.ToString();
    }

    private void ProcessScript(string key, ScriptLiteral script, StringBuilder sb)
    {

        var items = script.Value.Split("|");
        var prefix = script.Prefix.ToLowerInvariant();
        if (items.Length > 2)
            throw new JsonException("Invalid script literal");
        var val = items[0];
        var ensureEndingSlash = val.EndsWith('/');
        if (ensureEndingSlash)
            val = val[0..^1];
            
        var defaultValue = items.Length == 2 ? items[1] : "";
        switch (prefix)
        {
            case "env":
            {
                var e = Environment.GetEnvironmentVariable(val.Trim());
                Append(sb, e ?? defaultValue, ensureEndingSlash);
                break;
            }
            case "file":
            {
                var path = RetrieveValue(key, val.Trim(), defaultValue);
                if (!File.Exists(path))
                {
                    throw new JsonException($"File {path} does not exist");
                }
                Append(sb, File.ReadAllText(path), ensureEndingSlash);
                break;
            }
            case "ref":
                Append(sb, RetrieveValue(key, val.Trim(), defaultValue), ensureEndingSlash);
                break;
            default:
                throw new JsonException($"Unknown prefix {prefix}");
        }
    }

    private string RetrieveValue(string key, string val, string defaultValue)
    {
        var prefixElements = key.Split(':')[..^1]
            .ToImmutableList();

        while (true)
        {
            var prefix = string.Join(":", prefixElements);
            var name = prefix.Length > 0 ? $"{prefix}:{val}" : val;
            if (_config[name] is { } v)
            {
                return TranslateString(name, v);
            }
            if (prefixElements.Count == 0)
            {
                return defaultValue;
            }
            prefixElements = prefixElements.RemoveAt(prefixElements.Count - 1);
        }
    }
    
    private void Append(StringBuilder sb, string s, bool ensureEndingSlash)
    {
        if (ensureEndingSlash && !(s.EndsWith(Path.DirectorySeparatorChar)
                                   || s.EndsWith(Path.AltDirectorySeparatorChar)))
        {
            sb.Append(s);
            sb.Append(Path.DirectorySeparatorChar);
        }
        else
        {
            if (s.EndsWith(Path.AltDirectorySeparatorChar))
            {
                //Replace the last character with Path.DirectorySeparatorChar
                sb.Append(s[0..^1]);
                sb.Append(Path.DirectorySeparatorChar);
            }
            else
            {
                sb.Append(s);
            }
        }
    }
    
    public void Set(string key, string? value)
    {
        _config[key] = value;
    }

    public IChangeToken GetReloadToken()
    {
        return null!;
    }

    public void Load()
    {
    }

    public IEnumerable<string> GetChildKeys(IEnumerable<string> earlierKeys, string? parentPath)
    {
        var section = earlierKeys
            .Aggregate(_config, (current, key) => current.GetSection(key));
        if (parentPath != null)
        {
            section = section.GetSection(parentPath);
        }
        return section.GetChildren().Select(c => c.Key);
    }
    
}