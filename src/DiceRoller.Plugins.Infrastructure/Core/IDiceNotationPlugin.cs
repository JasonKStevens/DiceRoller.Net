using System.Collections.Generic;
using System.Threading.Tasks;

namespace DiceRoller.Plugins.Infrastructure.Core
{
    /// <summary>
    /// Interface for plugins that extend dice notation grammar
    /// </summary>
    public interface IDiceNotationPlugin : IPlugin
    {
        /// <summary>
        /// Priority for this notation extension (higher numbers processed first)
        /// </summary>
        int Priority { get; }

        /// <summary>
        /// Grammar extensions provided by this plugin
        /// </summary>
        IReadOnlyList<DiceNotationExtension> GrammarExtensions { get; }

        /// <summary>
        /// Function extensions provided by this plugin
        /// </summary>
        IReadOnlyList<DiceNotationFunction> FunctionExtensions { get; }

        /// <summary>
        /// Operator extensions provided by this plugin
        /// </summary>
        IReadOnlyList<DiceNotationOperator> OperatorExtensions { get; }

        /// <summary>
        /// Check if this plugin can handle a specific notation pattern
        /// </summary>
        /// <param name="notation">Dice notation to check</param>
        /// <returns>True if this plugin can handle the notation</returns>
        bool CanHandle(string notation);

        /// <summary>
        /// Parse and evaluate a dice notation extension
        /// </summary>
        /// <param name="notation">Dice notation to evaluate</param>
        /// <param name="context">Evaluation context</param>
        /// <returns>Evaluation result</returns>
        Task<DiceNotationResult> EvaluateAsync(string notation, DiceNotationContext context);

        /// <summary>
        /// Transform notation before standard parsing (preprocessing)
        /// </summary>
        /// <param name="notation">Original notation</param>
        /// <param name="context">Transformation context</param>
        /// <returns>Transformed notation</returns>
        Task<string> TransformNotationAsync(string notation, DiceNotationContext context);

        /// <summary>
        /// Post-process evaluation results (postprocessing)
        /// </summary>
        /// <param name="result">Original result</param>
        /// <param name="context">Post-processing context</param>
        /// <returns>Modified result</returns>
        Task<DiceNotationResult> PostProcessResultAsync(DiceNotationResult result, DiceNotationContext context);
    }

    /// <summary>
    /// Dice notation grammar extension
    /// </summary>
    public record DiceNotationExtension(
        string Name,
        string Pattern,
        string Description,
        DiceNotationExtensionType Type,
        int Precedence = 0);

    /// <summary>
    /// Dice notation function extension
    /// </summary>
    public record DiceNotationFunction(
        string Name,
        string Description,
        IReadOnlyList<DiceNotationParameter> Parameters,
        Type ReturnType,
        Func<object[], DiceNotationContext, Task<object>> Implementation);

    /// <summary>
    /// Dice notation operator extension
    /// </summary>
    public record DiceNotationOperator(
        string Symbol,
        string Name,
        string Description,
        DiceNotationOperatorType Type,
        int Precedence,
        DiceNotationAssociativity Associativity,
        Func<object, object, DiceNotationContext, Task<object>> Implementation);

    /// <summary>
    /// Dice notation parameter definition
    /// </summary>
    public record DiceNotationParameter(
        string Name,
        Type Type,
        bool IsRequired,
        object? DefaultValue = null,
        string? Description = null);

    /// <summary>
    /// Dice notation evaluation result
    /// </summary>
    public record DiceNotationResult(
        bool Success,
        object? Value,
        string Breakdown,
        string? ErrorMessage = null,
        Dictionary<string, object>? AdditionalData = null);

    /// <summary>
    /// Dice notation evaluation context
    /// </summary>
    public record DiceNotationContext(
        string UserId,
        string OriginalNotation,
        Dictionary<string, object>? Variables = null,
        Dictionary<string, object>? UserSettings = null,
        object? RandomNumberGenerator = null);

    /// <summary>
    /// Types of dice notation extensions
    /// </summary>
    public enum DiceNotationExtensionType
    {
        Token,
        Expression,
        Statement,
        Modifier
    }

    /// <summary>
    /// Types of dice notation operators
    /// </summary>
    public enum DiceNotationOperatorType
    {
        Unary,
        Binary,
        Ternary
    }

    /// <summary>
    /// Operator associativity
    /// </summary>
    public enum DiceNotationAssociativity
    {
        Left,
        Right,
        None
    }
}