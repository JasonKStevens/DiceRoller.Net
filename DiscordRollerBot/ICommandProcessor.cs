using DiceRoller;
using DiceRoller.Parser;

namespace DiscordRollerBot
{
    public interface ICommandProcessor
    {
        string Prefix { get; }
        (bool, string) Process(DiscordUserInfo userInfo, string commandText);
        (bool, TypedResult) ProcessTyped(string userId, string commandText);
    }
}
