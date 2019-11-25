# cli-dotnet

Another command line parser and executer tool.

Driven entirely by attributes, there is no additional setup required.

## Examples

### Quick start

Create a basic command definition
```C#
public class CommandRoot
{
    [Command]
    public Task QuickStart(
        [Value]string user,
        [Option]bool someflag)
    {
        //Do something here...
        return Task.CompletedTask;
    }
}
```

Setup Program.cs
```C#
async static Task Main(string[] args)
{
    try {
        await Cli.ExecuteAsync(new CommandRoot());
    }
    catch {
        Console.WriteLine("ERROR");
    }
}
```

This will accept command such as `quickstart myuser` or `quickstart myuser --someflag`

### Verbs

Additional verbs can be defined by exposing a public property on your Root command (or subsequent Verb classes) and decorating it with the `VerbAttribute` class

Example:

The following example would accept the command `myverb mycommand`, as with the quickstart example, the MyCommand method can take parameters decorated with either the `ValueAttribute` or the `OptionAttribute`

```C#
public class MyVerbClass
{
    [Command]
    public Task MyCommand()
    {
        return Task.CompletedTask;
    }
}

public class CommandRoot
{
    [Verb]
    public MyVerbClass MyVerb { get; } = new MyVerbClass();
}
```

The name of the verb can be overridden in the VerbAttribute like so:

```C#
public class CommandRoot
{
    [Verb(name:"NotMyVerb")]
    public MyVerbClass MyVerb { get; } = new MyVerbClass();
}
```

This would then be called with `notmyverb mycommand`

### Flags and Values

Commands can accept any number of values followed by optional flags. If a flag is omitted, the command will be executed with the parameter value set to the default value of the type.

The following command can be called with:
* `mycommand myvalue --stringoption mystringvalue --booloption false`
* `mycommand myvalue --stringoption "my string value" --booloption`
* `mycommand myvalue` - This would result in a call such as `MyCommand("myvalue", default, default)`

```C#
[Command]
public Task MyCommand(
    [Value] string someValue,
    [Option] string stringOption,
    [Option] bool boolOption)
{
    ...
}
```

By default all Option parameters use the parmeter name as it's "long form" argument name. However, there are a number of configurable values for Option flags:

```C#
[Command]
public Task MyCommand(
    [Option(shortForm:'s', longForm:"string")] string stringOption)
{
    ...
}

```

This would accept `mycommand -s valNoSpaces` or `mycommand -s "val with spaces"`

Although Values must appear before Options in the executed command, the parameters can appear in any order. The only caveat is that the _values_ must appear in the order they will be used.

For example:

The following would all be called with `mycmd val1 val2 val3 --flag`

```C#
public Task MyCmd([Value] int firstValue, [Value] int secondValue, [Value] int lastValue) { }
public Task MyCmd([Option] bool flag, [Value] int firstValue, [Value] int secondValue, [Value] int lastValue) { }
public Task MyCmd([Value] int firstValue, [Option] bool flag, [Value] int secondValue, [Value] int lastValue) { }
...
```

### Arrays

If an option is of an array type, then it may be specified multiple times, or contain multiple values for a single option. For example:

```C#
public void MyCmd([Option('t')] string[] test) { }
```
can be called with: `mycmd -t value1 -t value2 -t value3` or `mycmd -t value1 value2 value3`

If a value is of an array type, it will accept multiple "value" inputs. For example:

```C#
public void MyCmd([Value] string[] test) { }
```
can be called with: mycmd value1 value2 value3

### Help

`--help` or `-h` can be used at any point in the command to display help for the current command or verb.

For example:

`program.exe --help` would show the complete help about the program

`program.exe someverb --help` would show the available commands for the "someverb" verb

`program.exe someverb somecommand --help` would show the available commands for the "somecommand" command of the "someverb" verb

The long and short form flags for help can be overridden using the `CommandExecutorOptions` class and passing it into the `CommandExecutor` constructor.

```C#
new CommandExecutor(executorOptions: new CommandExecutorOptions
{
    HelpShortForm = 'a',
    HelpLongForm = "asdf"
})
```


