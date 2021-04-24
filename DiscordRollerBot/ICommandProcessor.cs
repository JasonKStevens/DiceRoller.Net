using DiceRoller;

namespace DiscordRollerBot
{
    public interface ICommandProcessor
    {
        string Prefix { get; }
        (bool, string) Process(DiscordUserInfo userInfo, string commandText);
    }
}
