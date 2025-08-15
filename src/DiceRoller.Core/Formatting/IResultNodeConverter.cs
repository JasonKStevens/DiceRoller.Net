using DiceRoller.Core.Parser;

namespace DiceRoller.Core.Formatting;

/// <summary>
/// Converts TypedResult to DiceResultNode for Progressive Disclosure formatting
/// </summary>
public interface IResultNodeConverter
{
    /// <summary>
    /// Convert a TypedResult to a DiceResultNode for formatting
    /// </summary>
    /// <param name="typedResult">The typed result from command processors</param>
    /// <returns>A DiceResultNode suitable for Progressive Disclosure formatting</returns>
    DiceResultNode Convert(TypedResult typedResult);
}