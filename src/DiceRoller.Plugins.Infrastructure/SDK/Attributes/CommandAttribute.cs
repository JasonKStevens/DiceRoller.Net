using System;

namespace DiceRoller.Plugins.Infrastructure.SDK.Attributes
{
    /// <summary>
    /// Attribute to mark a method as a plugin command
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public class CommandAttribute : Attribute
    {
        /// <summary>
        /// Command name
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Command description
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// Command aliases
        /// </summary>
        public string[]? Aliases { get; set; }

        /// <summary>
        /// Command usage example
        /// </summary>
        public string? Usage { get; set; }

        /// <summary>
        /// Command examples
        /// </summary>
        public string? Examples { get; set; }

        /// <summary>
        /// Whether command requires randomization
        /// </summary>
        public bool RequiresRandomization { get; set; } = true;

        /// <summary>
        /// Command category
        /// </summary>
        public string? Category { get; set; }

        /// <summary>
        /// Whether command is hidden from help
        /// </summary>
        public bool IsHidden { get; set; } = false;

        /// <summary>
        /// Minimum permission level required
        /// </summary>
        public string? RequiredPermission { get; set; }

        public CommandAttribute(string name)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
        }
    }

    /// <summary>
    /// Attribute to mark a method parameter as a command parameter
    /// </summary>
    [AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false)]
    public class CommandParameterAttribute : Attribute
    {
        /// <summary>
        /// Parameter name (overrides method parameter name)
        /// </summary>
        public string? Name { get; set; }

        /// <summary>
        /// Parameter description
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// Whether parameter is required
        /// </summary>
        public bool IsRequired { get; set; } = true;

        /// <summary>
        /// Default value if parameter is optional
        /// </summary>
        public object? DefaultValue { get; set; }

        /// <summary>
        /// Parameter validation pattern (regex)
        /// </summary>
        public string? ValidationPattern { get; set; }

        /// <summary>
        /// Minimum value for numeric parameters
        /// </summary>
        public double? MinValue { get; set; }

        /// <summary>
        /// Maximum value for numeric parameters
        /// </summary>
        public double? MaxValue { get; set; }

        /// <summary>
        /// Valid choices for enum-like parameters
        /// </summary>
        public string[]? ValidChoices { get; set; }
    }

    /// <summary>
    /// Attribute to mark a method as a table lookup command
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public class TableLookupAttribute : Attribute
    {
        /// <summary>
        /// Table name
        /// </summary>
        public string TableName { get; }

        /// <summary>
        /// Table description
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// Type of lookup key (e.g., "d100", "d20", "string")
        /// </summary>
        public string KeyType { get; set; } = "int";

        /// <summary>
        /// Whether the table supports range lookups
        /// </summary>
        public bool SupportsRanges { get; set; } = true;

        /// <summary>
        /// Minimum key value for range validation
        /// </summary>
        public int? MinKey { get; set; }

        /// <summary>
        /// Maximum key value for range validation
        /// </summary>
        public int? MaxKey { get; set; }

        public TableLookupAttribute(string tableName)
        {
            TableName = tableName ?? throw new ArgumentNullException(nameof(tableName));
        }
    }

    /// <summary>
    /// Attribute to mark a method as a dice notation function
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public class DiceNotationFunctionAttribute : Attribute
    {
        /// <summary>
        /// Function name in dice notation
        /// </summary>
        public string FunctionName { get; }

        /// <summary>
        /// Function description
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// Function category
        /// </summary>
        public string? Category { get; set; }

        /// <summary>
        /// Function usage example
        /// </summary>
        public string? Usage { get; set; }

        /// <summary>
        /// Function examples
        /// </summary>
        public string? Examples { get; set; }

        /// <summary>
        /// Function priority (higher numbers processed first)
        /// </summary>
        public int Priority { get; set; } = 0;

        public DiceNotationFunctionAttribute(string functionName)
        {
            FunctionName = functionName ?? throw new ArgumentNullException(nameof(functionName));
        }
    }

    /// <summary>
    /// Attribute to mark a method as a dice notation operator
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public class DiceNotationOperatorAttribute : Attribute
    {
        /// <summary>
        /// Operator symbol
        /// </summary>
        public string Symbol { get; }

        /// <summary>
        /// Operator name
        /// </summary>
        public string? Name { get; set; }

        /// <summary>
        /// Operator description
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// Operator precedence
        /// </summary>
        public int Precedence { get; set; } = 0;

        /// <summary>
        /// Operator associativity
        /// </summary>
        public string Associativity { get; set; } = "Left";

        /// <summary>
        /// Operator type (Unary, Binary, Ternary)
        /// </summary>
        public string OperatorType { get; set; } = "Binary";

        public DiceNotationOperatorAttribute(string symbol)
        {
            Symbol = symbol ?? throw new ArgumentNullException(nameof(symbol));
        }
    }

    /// <summary>
    /// Attribute to mark a method as a formatter
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public class FormatterAttribute : Attribute
    {
        /// <summary>
        /// Format name
        /// </summary>
        public string FormatName { get; }

        /// <summary>
        /// Format description
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// Supported content types
        /// </summary>
        public string[]? ContentTypes { get; set; }

        /// <summary>
        /// Supported file extensions
        /// </summary>
        public string[]? FileExtensions { get; set; }

        /// <summary>
        /// Whether format supports embedding
        /// </summary>
        public bool SupportsEmbedding { get; set; } = false;

        /// <summary>
        /// Formatter priority
        /// </summary>
        public int Priority { get; set; } = 0;

        public FormatterAttribute(string formatName)
        {
            FormatName = formatName ?? throw new ArgumentNullException(nameof(formatName));
        }
    }

    /// <summary>
    /// Attribute to provide help information for commands or functions
    /// </summary>
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, AllowMultiple = false)]
    public class HelpAttribute : Attribute
    {
        /// <summary>
        /// Help text
        /// </summary>
        public string HelpText { get; }

        /// <summary>
        /// Extended help text
        /// </summary>
        public string? ExtendedHelp { get; set; }

        /// <summary>
        /// Help category
        /// </summary>
        public string? Category { get; set; }

        /// <summary>
        /// Help tags for searching
        /// </summary>
        public string[]? Tags { get; set; }

        /// <summary>
        /// Related commands or functions
        /// </summary>
        public string[]? SeeAlso { get; set; }

        public HelpAttribute(string helpText)
        {
            HelpText = helpText ?? throw new ArgumentNullException(nameof(helpText));
        }
    }
}