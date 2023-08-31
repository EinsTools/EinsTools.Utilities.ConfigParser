using System.Collections.Immutable;
using System.Text;
using System.Text.Json;

namespace EinsTools.Utilities.ConfigParser;

internal static class StringSplitter
{
    public static ImmutableList<StringValue> Split(string input)
    {
        var result = ImmutableList<StringValue>.Empty;
        var builder = new StringBuilder();
        var inScript = false;
        while (input.Length > 0)
        {
            if (input.StartsWith("$("))
            {
                if (builder.Length > 0)
                {
                    result = result.Add(new StringLiteral(builder.ToString()));
                    builder.Clear();
                }
                inScript = true;
                input = input[2..];
            }
            else if (input.StartsWith("))"))
            {
                if (inScript)
                {
                    // This is an escape sequence
                    builder.Append(')');
                    input = input[2..];
                }
                else
                {
                    // just add the two characters
                    builder.Append("))");
                    input = input[2..];
                }
            }
            else if (input.StartsWith("(("))
            {
                if (inScript)
                {
                    // This is an escape sequence
                    builder.Append('(');
                    input = input[2..];
                }
                else
                {
                    // just add the two characters
                    builder.Append("((");
                    input = input[2..];
                }
            }
            else if (input.StartsWith("("))
            {
                if (inScript)
                {
                    // unterminated script
                    throw new JsonException("Unterminated script");
                }
                else
                {
                    // just add the character
                    builder.Append('{');
                    input = input[1..];
                }
            }
            else if (input.StartsWith(")"))
            {
                if (inScript)
                {
                    // end of script
                    var scriptTag = builder.ToString();
                    // split at the first colon
                    var r = ScriptLiteral.FromString(scriptTag);
                    result = result.Add(r);
                    builder.Clear();
                    inScript = false;
                    input = input[1..];
                }
                else
                {
                    // just add the character
                    builder.Append(')');
                    input = input[1..];
                }
            }
            else
            {
                builder.Append(input[0]);
                input = input[1..];
            }
        }
        
        if (inScript)
        {
            throw new JsonException("Unterminated script");
        }
        
        if (builder.Length > 0)
        {
            result = result.Add(new StringLiteral(builder.ToString()));
        }
        
        return result;
    }
    
}