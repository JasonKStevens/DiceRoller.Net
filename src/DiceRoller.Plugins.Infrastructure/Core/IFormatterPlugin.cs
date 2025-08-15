using System.Collections.Generic;
using System.Threading.Tasks;

namespace DiceRoller.Plugins.Infrastructure.Core
{
    /// <summary>
    /// Interface for plugins that provide custom result formatting
    /// </summary>
    public interface IFormatterPlugin : IPlugin
    {
        /// <summary>
        /// Supported output formats
        /// </summary>
        IReadOnlyList<OutputFormat> SupportedFormats { get; }

        /// <summary>
        /// Priority for this formatter (higher numbers processed first)
        /// </summary>
        int Priority { get; }

        /// <summary>
        /// Check if this formatter can handle a specific format
        /// </summary>
        /// <param name="format">Format to check</param>
        /// <returns>True if this formatter can handle the format</returns>
        bool CanFormat(string format);

        /// <summary>
        /// Format a dice roll result
        /// </summary>
        /// <param name="result">Result to format</param>
        /// <param name="format">Desired output format</param>
        /// <param name="context">Formatting context</param>
        /// <returns>Formatted result</returns>
        Task<FormattedResult> FormatAsync(object result, string format, FormattingContext context);

        /// <summary>
        /// Get available formatting options for a specific format
        /// </summary>
        /// <param name="format">Format name</param>
        /// <returns>Available formatting options</returns>
        IReadOnlyList<FormattingOption> GetFormattingOptions(string format);

        /// <summary>
        /// Get help information for supported formats
        /// </summary>
        /// <returns>Format help information</returns>
        FormatterHelpInfo GetHelp();
    }

    /// <summary>
    /// Output format definition
    /// </summary>
    public record OutputFormat(
        string Name,
        string Description,
        string ContentType,
        IReadOnlyList<string> FileExtensions,
        bool SupportsEmbedding = false);

    /// <summary>
    /// Formatted result
    /// </summary>
    public record FormattedResult(
        string Content,
        string ContentType,
        Dictionary<string, object>? Metadata = null,
        byte[]? BinaryData = null);

    /// <summary>
    /// Formatting context
    /// </summary>
    public record FormattingContext(
        string UserId,
        string Format,
        Dictionary<string, object>? Options = null,
        Dictionary<string, object>? UserPreferences = null,
        string? TargetPlatform = null);

    /// <summary>
    /// Formatting option definition
    /// </summary>
    public record FormattingOption(
        string Name,
        string Description,
        Type ValueType,
        object? DefaultValue = null,
        IReadOnlyList<object>? AllowedValues = null);

    /// <summary>
    /// Formatter help information
    /// </summary>
    public record FormatterHelpInfo(
        string FormatterName,
        string Description,
        IReadOnlyList<OutputFormatHelp> Formats);

    /// <summary>
    /// Output format help information
    /// </summary>
    public record OutputFormatHelp(
        string Name,
        string Description,
        string Usage,
        string Examples,
        IReadOnlyList<FormattingOptionHelp> Options);

    /// <summary>
    /// Formatting option help information
    /// </summary>
    public record FormattingOptionHelp(
        string Name,
        string Description,
        string Type,
        string? DefaultValue = null,
        string? AllowedValues = null);
}