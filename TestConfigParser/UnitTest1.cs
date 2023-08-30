using EinsTools.Utilities.ConfigParser;

namespace TestConfigParser;

public class TestStringSplitter
{
    [SetUp]
    public void Setup()
    {
    }

    [Test]
    public void TestNoReplacement()
    {
        var result = StringSplitter.Split("aba").ToArray();
        Assert.That(result, Has.Length.EqualTo(1));
        var StringLiteral = result[0] as StringLiteral;
        Assert.That(StringLiteral, Is.Not.Null);
        Assert.That(StringLiteral?.Value, Is.EqualTo("aba"));
    }

    [Test]
    public void TestReplacementOnly()
    {
        var result = StringSplitter.Split("$(aba)").ToArray();
        Assert.That(result, Has.Length.EqualTo(1));
        var referenceElement = result[0] as ScriptLiteral;
        Assert.That(referenceElement, Is.Not.Null);
        Assert.That(referenceElement?.Value, Is.EqualTo("aba"));
    }
    
    [Test]
    public void TestReplacementInTheMiddle()
    {
        var result = StringSplitter.Split("abc$(def)ghi").ToArray();
        Assert.That(result, Has.Length.EqualTo(3));
        var StringLiteral = result[0] as StringLiteral;
        Assert.That(StringLiteral, Is.Not.Null);
        Assert.That(StringLiteral?.Value, Is.EqualTo("abc"));
        var referenceElement = result[1] as ScriptLiteral;
        Assert.That(referenceElement, Is.Not.Null);
        Assert.That(referenceElement?.Value, Is.EqualTo("def"));
        StringLiteral = result[2] as StringLiteral;
        Assert.That(StringLiteral, Is.Not.Null);
        Assert.That(StringLiteral?.Value, Is.EqualTo("ghi"));
    }
    
    [Test]
    public void TestReplacementAtTheEnd()
    {
        var result = StringSplitter.Split("abc$(def)").ToArray();
        Assert.That(result, Has.Length.EqualTo(2));
        var StringLiteral = result[0] as StringLiteral;
        Assert.That(StringLiteral, Is.Not.Null);
        Assert.That(StringLiteral?.Value, Is.EqualTo("abc"));
        var referenceElement = result[1] as ScriptLiteral;
        Assert.That(referenceElement, Is.Not.Null);
        Assert.That(referenceElement?.Value, Is.EqualTo("def"));
        Assert.That(referenceElement?.Prefix, Is.EqualTo("ref"));
    }
    
    [Test]
    public void TestReplacementAtTheBeginning()
    {
        var result = StringSplitter.Split("$(def)ghi").ToArray();
        Assert.That(result, Has.Length.EqualTo(2));
        var referenceElement = result[0] as ScriptLiteral;
        Assert.That(referenceElement, Is.Not.Null);
        Assert.That(referenceElement?.Value, Is.EqualTo("def"));
        Assert.That(referenceElement?.Prefix, Is.EqualTo("ref"));
        var StringLiteral = result[1] as StringLiteral;
        Assert.That(StringLiteral, Is.Not.Null);
        Assert.That(StringLiteral?.Value, Is.EqualTo("ghi"));
    }
    
    [Test]
    public void TestReplacementWithColon()
    {
        var result = StringSplitter.Split("$(def:ghi)").ToArray();
        Assert.That(result, Has.Length.EqualTo(1));
        var referenceElement = result[0] as ScriptLiteral;
        Assert.That(referenceElement, Is.Not.Null);
        Assert.That(referenceElement?.Value, Is.EqualTo("ghi"));
        Assert.That(referenceElement?.Prefix, Is.EqualTo("def"));
    }
    
    [Test]
    public void TestReplacementWithColonAndSpace()
    {
        var result = StringSplitter.Split("$(def: ghi)").ToArray();
        Assert.That(result, Has.Length.EqualTo(1));
        var referenceElement = result[0] as ScriptLiteral;
        Assert.That(referenceElement, Is.Not.Null);
        Assert.That(referenceElement?.Value, Is.EqualTo("ghi"));
        Assert.That(referenceElement?.Prefix, Is.EqualTo("def"));
    }
    
    [Test]
    public void TestReplacementWithColonAndSpaceAndText()
    {
        var result = StringSplitter.Split("$(def: ghi) jkl").ToArray();
        Assert.That(result, Has.Length.EqualTo(2));
        var referenceElement = result[0] as ScriptLiteral;
        Assert.That(referenceElement, Is.Not.Null);
        Assert.That(referenceElement?.Value, Is.EqualTo("ghi"));
        Assert.That(referenceElement?.Prefix, Is.EqualTo("def"));
        var stringLiteral = result[1] as StringLiteral;
        Assert.That(stringLiteral, Is.Not.Null);
        Assert.That(stringLiteral?.Value, Is.EqualTo(" jkl"));
    }
}