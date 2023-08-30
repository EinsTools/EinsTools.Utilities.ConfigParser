using System.Text.Json;
using EinsTools.Utilities.ConfigParser;
using Microsoft.Extensions.Configuration;

namespace TestConfigParser;

public class TestConfiguration
{
    [SetUp]
    public void Setup()
    {
    }

    [Test]
    public void TestNoReplacement()
    {
        var data = new Dictionary<string, string?>()
        {
            { "A", "X" },
            { "B", "Y" }
        };

        var baseCfg = new ConfigurationBuilder()
            .AddInMemoryCollection(data)
            .Build();
        var cfg = new ConfigurationBuilder()
            .Add(new EinsToolsConfigurationSource(baseCfg))
            .Build();

        Assert.That(cfg["A"], Is.EqualTo("X"));
        Assert.That(cfg["B"], Is.EqualTo("Y"));
        Assert.That(cfg["C"], Is.Null);
    }
    
    [Test]
    public void TestSimpleReplacement()
    {
        var data = new Dictionary<string, string?>()
        {
            { "A", "X" },
            { "B", "$(A)Y" }
        };

        var baseCfg = new ConfigurationBuilder()
            .AddInMemoryCollection(data)
            .Build();
        var cfg = new ConfigurationBuilder()
            .Add(new EinsToolsConfigurationSource(baseCfg))
            .Build();

        Assert.That(cfg["A"], Is.EqualTo("X"));
        Assert.That(cfg["B"], Is.EqualTo("XY"));
        Assert.That(cfg["C"], Is.Null);
    }
    
    [Test]
    public void TestMultipleReplacement()
    {
        var data = new Dictionary<string, string?>()
        {
            { "A", "X" },
            { "B", "$(A)Y" },
            { "C", "$(B)Z" }
        };

        var baseCfg = new ConfigurationBuilder()
            .AddInMemoryCollection(data)
            .Build();
        var cfg = new ConfigurationBuilder()
            .Add(new EinsToolsConfigurationSource(baseCfg))
            .Build();

        Assert.That(cfg["A"], Is.EqualTo("X"));
        Assert.That(cfg["B"], Is.EqualTo("XY"));
        Assert.That(cfg["C"], Is.EqualTo("XYZ"));
        Assert.That(cfg["D"], Is.Null);
    }

    [Test]
    public void TestSimpleHierarchy()
    {
        var data = new Dictionary<string, string?>()
        {
            { "A", "X" },
            { "B:B", "Y" },
            { "B:C", "$(A)$(B)" }
        };
        
        var baseCfg = new ConfigurationBuilder()
            .AddInMemoryCollection(data)
            .Build();
        var cfg = new ConfigurationBuilder()
            .Add(new EinsToolsConfigurationSource(baseCfg))
            .Build();
        
        Assert.That(cfg["A"], Is.EqualTo("X"));
        Assert.That(cfg["B:A"], Is.Null);
        Assert.That(cfg["B:B"], Is.EqualTo("Y"));
        Assert.That(cfg["B:C"], Is.EqualTo("XY"));
    }
    
    [Test]
    public void TestSimpleHierarchy2()
    {
        var data = new Dictionary<string, string?>()
        {
            { "A", "X" },
            { "B:A", "Y" },
            { "B:C", "$(A)$(B)" }
        };
        
        var baseCfg = new ConfigurationBuilder()
            .AddInMemoryCollection(data)
            .Build();
        var cfg = new ConfigurationBuilder()
            .Add(new EinsToolsConfigurationSource(baseCfg))
            .Build();
        
        Assert.That(cfg["A"], Is.EqualTo("X"));
        Assert.That(cfg["B:A"], Is.EqualTo("Y"));
        Assert.That(cfg["B:B"], Is.Null);
        Assert.That(cfg["B:C"], Is.EqualTo("Y"));
    }
    
    [Test]
    public void TestSimpleHierarchy3()
    {
        var data = new Dictionary<string, string?>()
        {
            { "A", "X" },
            { "B", "Z" },
            { "B:A", "Y" },
            { "B:C:D", "$(A)$(B)" }
        };
        
        var baseCfg = new ConfigurationBuilder()
            .AddInMemoryCollection(data)
            .Build();
        var cfg = new ConfigurationBuilder()
            .Add(new EinsToolsConfigurationSource(baseCfg))
            .Build();
        
        Assert.That(cfg["A"], Is.EqualTo("X"));
        Assert.That(cfg["B:A"], Is.EqualTo("Y"));
        Assert.That(cfg["B:B"], Is.Null);
        Assert.That(cfg["B:C:D"], Is.EqualTo("YZ"));
    }
    
    [Test]
    public void TestSimpleHierarchy4()
    {
        // We test, that the replacement does not work "downwards"
        var data = new Dictionary<string, string?>()
        {
            { "A", "X" },
            { "B", "Z$(D)" },
            { "B:A", "Y" },
            { "B:C:D", "$(A)$(B)" }
        };
        
        var baseCfg = new ConfigurationBuilder()
            .AddInMemoryCollection(data)
            .Build();
        var cfg = new ConfigurationBuilder()
            .Add(new EinsToolsConfigurationSource(baseCfg))
            .Build();
        
        Assert.That(cfg["A"], Is.EqualTo("X"));
        Assert.That(cfg["B:A"], Is.EqualTo("Y"));
        Assert.That(cfg["B:B"], Is.Null);
        Assert.That(cfg["B:C:D"], Is.EqualTo("YZ"));
    }

    [Test]
    public void TestEnvReplacement()
    {
        var guidName = Guid.NewGuid().ToString();
        Environment.SetEnvironmentVariable("EINSTOOLS_TEST", guidName);
        var data = new Dictionary<string, string?>()
        {
            { "A", "X" },
            { "B", "$(env:EINSTOOLS_TEST)" },
            { "C", "$(ENV:EINSTOOLS_TEST)" },
            { "D", "$(env:NOT_EXISTING)" }
        };
        
        var baseCfg = new ConfigurationBuilder()
            .AddInMemoryCollection(data)
            .Build();
        var cfg = new ConfigurationBuilder()
            .Add(new EinsToolsConfigurationSource(baseCfg))
            .Build();
        
        Assert.That(cfg["A"], Is.EqualTo("X"));
        Assert.That(cfg["B"], Is.EqualTo(guidName));
        Assert.That(cfg["C"], Is.EqualTo(guidName));
        Assert.That(cfg["D"], Is.EqualTo(""));
    }

    [Test]
    public void TestFileReplacement()
    {
        // create a temporary file with some content
        var tempFile = Path.GetTempFileName();
        var guidName = Guid.NewGuid().ToString();
        File.WriteAllText(tempFile, guidName);
        try
        {
            var data = new Dictionary<string, string?>()
            {
                { "A", $"{tempFile}" },
                { "B", Guid.NewGuid().ToString()},
                { "C", "$(file:A)" },
                { "D", "$(FILE:A)" },
                { "E", "$(file:NOT_EXISTING)" },
                { "F", "$(file:B)" }
            };
        
            var baseCfg = new ConfigurationBuilder()
                .AddInMemoryCollection(data)
                .Build();
            var cfg = new ConfigurationBuilder()
                .Add(new EinsToolsConfigurationSource(baseCfg))
                .Build();
            Assert.That(cfg["A"], Is.EqualTo(tempFile));
            Assert.That(cfg["C"], Is.EqualTo(guidName));
            Assert.That(cfg["D"], Is.EqualTo(guidName));
            Assert.Throws<JsonException>(() =>
            {
                var _ = cfg["E"];
            });
            Assert.Throws<JsonException>(() =>
            {
                var _ = cfg["F"];
            });
        }
        finally
        {
            File.Delete(tempFile);
        }
    }

    [Test]
    public void TestPathJoining()
    {
        var data = new Dictionary<string, string?>
        {
            { "A", "C:\\Program Files" },
            { "B", "C:\\Program Files\\" },
            { "C", "MyApp" },
            { "D", "$(A/)$(C)" },
            { "E", "$(B/)$(C)" }
        };
        
        var baseCfg = new ConfigurationBuilder()
            .AddInMemoryCollection(data)
            .Build();
        var cfg = new ConfigurationBuilder()
            .AddEinsToolsConfiguration(baseCfg)
            .Build();
        
        Assert.That(cfg["D"], Is.EqualTo("C:\\Program Files\\MyApp"));
        Assert.That(cfg["E"], Is.EqualTo("C:\\Program Files\\MyApp"));
    }
}