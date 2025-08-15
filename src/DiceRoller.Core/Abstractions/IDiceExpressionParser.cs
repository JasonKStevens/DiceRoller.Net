using Irony.Parsing;

namespace DiceRoller.Core.Abstractions
{
    /// <summary>
    /// Interface for parsing dice expressions into syntax trees
    /// </summary>
    public interface IDiceExpressionParser
    {
        /// <summary>
        /// Parses a dice expression into a syntax tree
        /// </summary>
        /// <param name="expression">The dice expression to parse</param>
        /// <returns>The parsed syntax tree</returns>
        ParseTree Parse(string expression);

        /// <summary>
        /// Determines if the parser has special handling for EarthDawn step functions
        /// </summary>
        bool SupportsStepFunctions { get; }

        /// <summary>
        /// Evaluates an EarthDawn step number if supported
        /// </summary>
        /// <param name="stepNumber">The step number to evaluate</param>
        /// <returns>The parsed syntax tree for the step</returns>
        ParseTree ParseStep(int stepNumber);
    }
}