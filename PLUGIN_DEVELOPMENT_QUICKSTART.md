# DiceRoller.Net Plugin Development Quickstart

Get up and running with DiceRoller.Net plugin development in 10 minutes! This guide provides the fastest path to creating your first working plugin.

## 🚀 Quick Setup (2 minutes)

### Prerequisites
- [.NET 8.0 SDK](https://dotnet.microsoft.com/download) or later
- Text editor or IDE ([VS Code](https://code.visualstudio.com/) recommended)
- Basic C# knowledge

### 1. Get the Template
```bash
# Clone or download the DiceRoller.Net repository
cd DiceRoller.Net/src

# Copy the template
cp -r DiceRoller.Plugins.Template DiceRoller.Plugins.HelloWorld
cd DiceRoller.Plugins.HelloWorld
```

### 2. Quick Rename
```bash
# Rename the project file (Windows)
ren "DiceRoller.Plugins.Template.csproj" "DiceRoller.Plugins.HelloWorld.csproj"

# Or on Linux/Mac
mv DiceRoller.Plugins.Template.csproj DiceRoller.Plugins.HelloWorld.csproj
```

## 🎯 HelloWorld Plugin (5 minutes)

### 1. Update Plugin Manifest
Edit `Plugin.manifest.json`:
```json
{
  "id": "helloworld",
  "name": "HelloWorld Game System",
  "version": "1.0.0",
  "description": "My first DiceRoller.Net plugin!",
  "author": "Your Name",
  "assemblyFileName": "DiceRoller.Plugins.HelloWorld.dll",
  "pluginTypeName": "DiceRoller.Plugins.HelloWorld.HelloWorldPlugin"
}
```

### 2. Create Simple Plugin Class
Replace `TemplatePlugin.cs` with `HelloWorldPlugin.cs`:

```csharp
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using DiceRoller.Plugins.Infrastructure.Core;
using DiceRoller.Plugins.Infrastructure.SDK.Helpers;
using DiceRoller.Plugins.Infrastructure.SDK.Attributes;

namespace DiceRoller.Plugins.HelloWorld
{
    [Plugin("helloworld", "HelloWorld Game System", "1.0.0",
        Description = "My first DiceRoller.Net plugin!",
        Author = "Your Name")]
    [GameSystemPlugin("HelloWorld")]
    public class HelloWorldPlugin : PluginBase, IGameSystemPlugin
    {
        private readonly List<GameSystemCommand> _commands = new();
        private readonly List<GameSystemTable> _tables = new();

        public override string Id => "helloworld";
        public override string Name => "HelloWorld Game System";
        public override Version Version => new(1, 0, 0);
        public override string Description => "My first DiceRoller.Net plugin!";
        public override string Author => "Your Name";
        
        public string GameSystemName => "HelloWorld";
        public string GameSystemVersion => "1.0";
        public IReadOnlyList<GameSystemCommand> Commands => _commands.AsReadOnly();
        public IReadOnlyList<GameSystemTable> Tables => _tables.AsReadOnly();

        protected override async Task OnInitializeAsync(CancellationToken cancellationToken)
        {
            Logger.LogInformation("HelloWorld plugin initializing...");

            // Register a simple "hello" command
            _commands.Add(new GameSystemCommand(
                "hello",
                "Say hello to the world!",
                new List<GameSystemParameter>
                {
                    new("name", typeof(string), false, "World", "Name to greet")
                },
                "A simple greeting command to test your plugin.",
                true));

            Logger.LogInformation("HelloWorld plugin initialized!");
            await Task.CompletedTask;
        }

        protected override async Task OnStartAsync(CancellationToken cancellationToken)
        {
            Logger.LogInformation("HelloWorld plugin started!");
            await Task.CompletedTask;
        }

        protected override async Task OnStopAsync(CancellationToken cancellationToken)
        {
            Logger.LogInformation("HelloWorld plugin stopped!");
            await Task.CompletedTask;
        }

        public async Task<GameSystemCommandResult> ExecuteCommandAsync(
            string commandName, 
            Dictionary<string, object> parameters, 
            GameSystemContext context)
        {
            if (commandName.ToLowerInvariant() == "hello")
            {
                var name = parameters.TryGetValue("name", out var nameValue) 
                    ? nameValue?.ToString() ?? "World" 
                    : "World";

                var message = $"🌟 Hello, **{name}**! Welcome to DiceRoller.Net plugins! 🎲";
                
                Logger.LogInformation("HelloWorld command executed for user {UserId} with name {Name}", 
                    context.UserId, name);

                return new GameSystemCommandResult(true, message, message);
            }

            return new GameSystemCommandResult(false, null, string.Empty, $"Unknown command: {commandName}");
        }

        public async Task<GameSystemTableResult> LookupTableAsync(
            string tableName, 
            object key, 
            GameSystemContext context)
        {
            // No tables in this simple example
            return new GameSystemTableResult(false, null, string.Empty, $"Unknown table: {tableName}");
        }

        public GameSystemHelpInfo GetHelp(string? commandName = null)
        {
            var commandHelps = new List<CommandHelpInfo>
            {
                new CommandHelpInfo(
                    "hello",
                    "Say hello to the world!",
                    "hello [name]",
                    "hello - Says hello to the world\nhello Alice - Says hello to Alice",
                    new List<ParameterHelpInfo>
                    {
                        new("name", "string", false, "Name to greet", "World")
                    })
            };

            return new GameSystemHelpInfo(
                GameSystemName,
                "A simple HelloWorld plugin to demonstrate basic plugin functionality.",
                commandHelps,
                new List<TableHelpInfo>());
        }
    }
}
```

### 3. Update Project File
Edit `DiceRoller.Plugins.HelloWorld.csproj`:
```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net8.0</TargetFramework>
    <ImplicitUsings>enable</ImplicitUsings>
    <Nullable>enable</Nullable>
    <Title>DiceRoller HelloWorld Plugin</Title>
    <Authors>Your Name</Authors>
    <Description>My first DiceRoller.Net plugin</Description>
  </PropertyGroup>

  <ItemGroup>
    <ProjectReference Include="../DiceRoller.Plugins.Infrastructure/DiceRoller.Plugins.Infrastructure.csproj" />
    <ProjectReference Include="../DiceRoller.Core/DiceRoller.Core.csproj" />
  </ItemGroup>
</Project>
```

## 🔨 Build and Test (3 minutes)

### 1. Build Your Plugin
```bash
# Build the plugin
dotnet build

# Check for build success
echo $?  # Should output 0 on success
```

### 2. Deploy to DiceRoller Bot
```bash
# Copy plugin files to bot plugins directory
cp bin/Debug/net8.0/* ../../DiceRoller.Bot/bin/Debug/net8.0/plugins/

# Or for release build
cp bin/Release/net8.0/* ../../DiceRoller.Bot/bin/Release/net8.0/plugins/
```

### 3. Test in Discord
1. Start the DiceRoller bot
2. In Discord, run these commands:
   ```
   /plugin list
   /plugin info helloworld
   /helloworld hello
   /helloworld hello name:Alice
   ```

## 🎉 Success!

If you see your plugin in the list and the hello command works, congratulations! You've created your first DiceRoller.Net plugin.

## 🚀 Next Steps

### Immediate Next Steps
1. **Add Another Command**: Try adding a simple dice roll command
2. **Add a Table**: Create a basic lookup table for game data
3. **Improve Formatting**: Add emojis and better Discord formatting

### Learning Path
1. 📖 **[Full Template Documentation](src/DiceRoller.Plugins.Template/README.md)** - Comprehensive examples
2. 📖 **[Step-by-Step Development Guide](src/DiceRoller.Plugins.Template/DEVELOPMENT.md)** - Detailed walkthrough
3. 📖 **[Plugin Infrastructure Guide](src/DiceRoller.Plugins.Infrastructure/README.md)** - Technical deep dive
4. 🔍 **[CallOfCthulhu Plugin](src/DiceRoller.Plugins.CallOfCthulhu/)** - Real-world example

### Example Enhancements

**Add a Simple Dice Command:**
```csharp
// In your command registration
_commands.Add(new GameSystemCommand(
    "roll",
    "Roll dice with optional modifier",
    new List<GameSystemParameter>
    {
        new("dice", typeof(string), true, null, "Dice expression (e.g., 1d20, 3d6+2)"),
        new("comment", typeof(string), false, "", "Optional comment for the roll")
    },
    "Roll dice and show the result.",
    true));

// In your ExecuteCommandAsync method
if (commandName.ToLowerInvariant() == "roll")
{
    var dice = parameters.TryGetValue("dice", out var diceValue) 
        ? diceValue?.ToString() ?? "1d20" 
        : "1d20";
    var comment = parameters.TryGetValue("comment", out var commentValue) 
        ? commentValue?.ToString() ?? "" 
        : "";

    // Simple dice rolling (in real plugin, use DiceRoller.Core services)
    var random = new Random();
    var result = random.Next(1, 21); // Simple d20 for demo
    
    var message = $"🎲 **Dice Roll**: {dice}\n📊 **Result**: {result}";
    if (!string.IsNullOrEmpty(comment))
        message += $"\n💬 **Comment**: {comment}";

    return new GameSystemCommandResult(true, result, message);
}
```

**Add a Simple Table:**
```csharp
// In your table registration
_tables.Add(new GameSystemTable(
    "weather",
    "Simple weather conditions",
    typeof(int),
    typeof(string),
    "Roll 1d6 to get weather for the day."));

// In your LookupTableAsync method
if (tableName.ToLowerInvariant() == "weather")
{
    if (!int.TryParse(key.ToString(), out var roll))
        return new GameSystemTableResult(false, null, string.Empty, "Invalid roll value");

    var weather = roll switch
    {
        1 => "☀️ **Sunny** - Clear skies and bright sunshine",
        2 => "⛅ **Partly Cloudy** - Some clouds with sunny breaks",
        3 => "☁️ **Cloudy** - Overcast with gray clouds",
        4 => "🌧️ **Rainy** - Light to moderate rainfall",
        5 => "⛈️ **Stormy** - Heavy rain with thunder and lightning",
        6 => "🌨️ **Snowy** - Snow falling, cold temperatures",
        _ => "🌫️ **Foggy** - Poor visibility due to fog"
    };

    return new GameSystemTableResult(true, weather, weather);
}
```

## 🆘 Troubleshooting

**Plugin doesn't load:**
- Check `Plugin.manifest.json` syntax (use a JSON validator)
- Verify assembly name matches in manifest
- Check bot logs for error messages

**Commands don't work:**
- Verify command registration in `OnInitializeAsync`
- Check parameter names match between registration and execution
- Test with simple parameters first

**Build errors:**
- Ensure .NET 8.0 SDK is installed
- Check project references are correct
- Verify namespace updates are complete

## 📚 Resources

- **[Plugin Template](src/DiceRoller.Plugins.Template/)** - Full-featured example
- **[CallOfCthulhu Plugin](src/DiceRoller.Plugins.CallOfCthulhu/)** - Production example
- **[DiceRoller.Net Core](README.md)** - Main project documentation
- **[Discord Developer Docs](https://discord.com/developers/docs)** - Discord API reference

---

**Ready to create amazing dice rolling experiences!** 🎲✨

*Next: Explore the [Full Template Documentation](src/DiceRoller.Plugins.Template/README.md) for advanced features and best practices.*