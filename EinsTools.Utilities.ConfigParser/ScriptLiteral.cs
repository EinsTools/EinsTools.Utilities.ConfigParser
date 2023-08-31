namespace EinsTools.Utilities.ConfigParser;

internal record ScriptLiteral(string Value, string Prefix) : StringValue {
    public static ScriptLiteral FromString(string scriptTag) {
        var elements = scriptTag.Split('=', 2);
        return elements.Length switch
        {
            1 => new ScriptLiteral(elements[0], "ref"),
            2 => new ScriptLiteral(elements[1].TrimStart(), elements[0].TrimEnd()),
            _ => throw new Exception("Unexpected input")
        };
    }
}