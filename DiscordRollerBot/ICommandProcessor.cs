using DiceRoller;
using DiceRoller.Parser;

namespace DiscordRollerBot
{
    public interface ICommandProcessor
    {
        string Prefix { get; }
        (bool, TypedResult) ProcessTyped(string userId, string commandText);
    }
}
