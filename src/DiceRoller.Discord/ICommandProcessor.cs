using DiceRoller;
using DiceRoller.Core.Parser;

namespace DiscordRollerBot
{
    public interface ICommandProcessor
    {
        string Prefix { get; }
        (bool, TypedResult) ProcessTyped(string userId, string commandText);
    }
}
