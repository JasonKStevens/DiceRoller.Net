using DiceRoller;
using DiceRoller.Dice;
using DiceRoller.DragonQuest;
using DiceRoller.Heroes;
using DiceRoller.Parser;
using DiceRollerCmd;
using DiscordRollerBot;
using DSharpPlus;
using Microsoft.AspNetCore;
using PartyDSL.Parser;
using PartyDSL;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Add services to the container.
        builder.WebHost.ConfigureServices(ConfigureServices);
        builder.Logging.SetMinimumLevel(LogLevel.Information);

        builder.Services.AddControllers();

        var app = builder.Build();

        // Configure the HTTP request pipeline.

        app.UseHttpsRedirection();

        app.UseAuthorization();

        app.MapControllers();

        app.Run();
    }



    private static void ConfigureServices(WebHostBuilderContext context, IServiceCollection services)
    {
        var botConfig = new DiscordApiConfiguration();
        context.Configuration.Bind("BotConfig", botConfig);
        services.AddSingleton(botConfig);

        var config = new DiscordConfiguration()
        {
            Token =  botConfig.Token,
            TokenType = TokenType.Bot,
            Intents = DiscordIntents.AllUnprivileged
        };
        services.AddSingleton(config);

        services.AddSingleton<DiscordClient, DiscordClient>();

        services.AddSingleton<IDiscordApi, DiscordApi>();

        services.AddSingleton<IRandomNumberGenerator, RandomNumberGenerator>();
        services.AddSingleton<DiceRollEvaluator, DiceRollEvaluator>();
        services.AddSingleton<GrievousInjuries, GrievousInjuries>();
        services.AddSingleton<Backfires, Backfires>();
        services.AddSingleton<FearResult, FearResult>();
        services.AddSingleton<DQLookupTables, DQLookupTables>();
        services.AddSingleton<SpeedTable, SpeedTable>();
        services.AddSingleton<LocationTable, LocationTable>();
        services.AddSingleton<HerosLookupTables, HerosLookupTables>();

        services.AddSingleton<IPartyManager, PartyManager>();
        services.AddSingleton<PartyCommandEvaluator>();
        services.AddSingleton<IUserSettings, UserSettings>();
        services.AddSingleton<SettingsCommandEvaluator>();

        services.RegisterAllTypes<ICommandProcessor>(new[] { typeof(BotHost).Assembly }, ServiceLifetime.Singleton);

        services.AddHostedService<BotHost>();
    }
}