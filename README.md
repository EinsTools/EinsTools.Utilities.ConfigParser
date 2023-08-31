# EinsTools.Utilities.ConfigParser

## Description

The ConfigParser is an extension for the Microsoft.Extensions.Configuration library. 
It allows you to use referenced in your configuration files.

For example:

```json
{
  "A": "Hello",
  "B": "World",
  "C": "$(A) $(B)!"
}
```

The ConfigParser will replace the $(A) and $(B) with the values from the configuration file. Requesting the
value of C will return "Hello World!".

You can reference values from different levels. In the following example the value of C will be "Hello World!",
while "D:C" will be "Goodbye World!".

```json
{
  "A": "Hello",
  "B": "World",
  "C": "$(A) $(B)!",
  "D": {
    "A": "Goodbye",
    "C": "$(A) $(B)!"
  }
}
```

References will only be resolved from the same or upper levels. In the following example the value of B will be
"Hello !".

```json
{
  "A": "Hello",
  "B": "$(A) $(D)!",
  "C": {
    "D": "World"
  }
}
```

You can however use the Microsoft.Configuration path syntax. Thus the following example will result in the value
of B being "Hello World!".

```json
{
  "A": "Hello",
  "B": "$(A) $(C:D)!",
  "C": {
    "D": "World"
  }
}
```

## Default values

If a reference is not found, it will be replaced with an empty string. You can however specify a default value
that will be used instead. The default value is separated from the reference by a pipe character. For example:

```json
{
  "A": "Hello",
  "B": "World",
  "C": "$(A) $(D|Goodbye)!"
}
```

The value of C will be "Hello Goodbye!".

## Prefixes

You can prefix a reference with the strings "ref=", "env=" or "file=".

"ref=" will reference a value from the configuration file. This is the default behaviour, if no prefix is used.
"env=" will reference a value from the environment variables.
"file=" will reference a value from a file. The value behind the prefix will be interpreted as a reference to
another value in the configuration file, which should hold the file name.

Whitespace between the prefix and the reference will be ignored. So `$(env = PATH)` will work as well as
`$(env=PATH)`.

For example:

```json
{
  "A": "./test.txt",
  "B": "$(env=PATH)",
  "C": "$(file=C)"
}
```

The value of A will be interpreted as a file name. The value of B will be the value of the environment variable
PATH. The value of C will be the content of the file test.txt.

## Path Joining

If the name of a reference ends with a slash, we will make sure that the value ends in a Path Separator. If the
path already ends in a path separator, we will not add another one. If the path does not end in a path separator,
we will add one. This is useful to combine paths. For example:

```json
{
  "A": "C:\\Program Files",
  "B": "C:\\Program Files\\",
  "C": "MyApp",
  "D": "$(A/)$(C)",
  "E": "$(B/)$(C)"
}
```

The values of D and E will be "C:\\Program Files\\MyApp". Which seperator is used, depends on the operating system.
It will be a slash on Linux and MacOs and a backslash on Windows.

If you want to combine this mechanism with the default value mechanism, you have to use the slash before the pipe.
The syntax would be `$(A/|Default)`.

As an alternative you can also use the `path=` prefix. `$(path=A|Default)` will result in the same value as
`$(A/|Default)`.

You can also join paths by using the `join=` prefix. `$(join=A,B,C)` will result in the same value as
`$(A/)$(B/)$(C)`. The values must be separated by commas. Whitespace before and after the commas will be ignored.

For the join prefix, the default value mechanism works differently. Each element in the list can have its own
default value. The syntax is `$(join=A|DefaultA,B|DefaultB,C|DefaultC)`. There is however no way to specify a
default value for the whole list.

## Usage

You have to first create you configuration as usual. As soon as you have an IConfigurationRoot, you can use the
ConfigParser to parse the values.

```csharp
var baseCfg = new ConfigurationBuilder()
    .AddJsonFile("config.json")
    .Build();
var cfg = new ConfigurationBuilder()
    .AddEinsToolsConfiguration(baseCfg)
    .Build();
```

The cfg variable will now contain the parsed configuration. You can use it as usual.