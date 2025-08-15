using System;
using System.Collections.Generic;
using System.Linq;
using DiceRoller.Core.Abstractions;
using Irony.Parsing;

namespace DiceRoller.Core.Parser
{
    /// <summary>
    /// Parser implementation for dice expressions
    /// </summary>
    public class DiceExpressionParser : IDiceExpressionParser
    {
        private readonly Irony.Parsing.Parser _parser;
        private readonly Dictionary<int, EarthDawnStepDefinition> _parsedSteps = new Dictionary<int, EarthDawnStepDefinition>();

        // EarthDawn step definitions
        private readonly List<string> _initialSteps = new List<string>
        {
            "1d4!-2",
            "1d4!-1", 
            "1d4!",
            "1d6!",
            "1d8!",
            "1d10!",
            "1d12!"
        };

        private readonly List<string> _repeatingSteps = new List<string>
        {
            "2d6!",
            "1d8! + 1d6!",
            "2d8!",
            "1d10! + 1d8!",
            "2d10!",
            "1d12! + 1d10!",
            "2d12!",
            "1d12! + 2d6!",
            "1d12! + 1d8! + 1d6!",
            "1d12! + 2d8!",
            "1d12! + 1d10! + 1d8!"
        };

        private const string RepeatingDie = "d20!";
        private const int RepeatingSeqStart = 8;
        private const int RepeatingSeqCycleCount = 11;

        /// <summary>
        /// Gets whether this parser supports EarthDawn step functions
        /// </summary>
        public bool SupportsStepFunctions => true;

        /// <summary>
        /// Initializes a new instance of the DiceExpressionParser
        /// </summary>
        public DiceExpressionParser()
        {
            var grammar = new ExpressionGrammar();
            GrammarExtensions.Extend(grammar);

            var language = new LanguageData(grammar);
            _parser = new Irony.Parsing.Parser(language);

            ParseInitialSteps();
        }

        /// <summary>
        /// Parses a dice expression into a syntax tree
        /// </summary>
        /// <param name="expression">The dice expression to parse</param>
        /// <returns>The parsed syntax tree</returns>
        public ParseTree Parse(string expression)
        {
            if (string.IsNullOrWhiteSpace(expression))
                throw new ArgumentException("Expression cannot be null or empty", nameof(expression));

            var syntaxTree = _parser.Parse(expression);

            if (syntaxTree.HasErrors())
            {
                var messages = syntaxTree.ParserMessages.Select(m => m.Message);
                var detail = string.Join(Environment.NewLine + " - ", messages);
                var message = $"Parser errors:{Environment.NewLine} - {detail}";
                throw new InvalidOperationException(message);
            }

            return syntaxTree;
        }

        /// <summary>
        /// Evaluates an EarthDawn step number if supported
        /// </summary>
        /// <param name="stepNumber">The step number to evaluate</param>
        /// <returns>The parsed syntax tree for the step</returns>
        public ParseTree ParseStep(int stepNumber)
        {
            if (!_parsedSteps.ContainsKey(stepNumber))
            {
                var adjustedStep = stepNumber - RepeatingSeqStart;
                var scales = adjustedStep / RepeatingSeqCycleCount;
                adjustedStep = adjustedStep % RepeatingSeqCycleCount;

                var baseRoll = _repeatingSteps[adjustedStep];
                if (scales > 0)
                {
                    baseRoll = $"{scales}{RepeatingDie} + {baseRoll}";
                }

                _parsedSteps.Add(stepNumber, new EarthDawnStepDefinition(baseRoll, _parser.Parse(baseRoll).Root));
            }

            var rollDef = _parsedSteps[stepNumber];
            // Return the existing parse tree with the cached node
            return _parser.Parse(rollDef.RollDefinition);
        }

        private void ParseInitialSteps()
        {
            for (int i = 0; i < _initialSteps.Count; i++)
            {
                var syntaxTree = _parser.Parse(_initialSteps[i]);
                _parsedSteps.Add(i + 1, new EarthDawnStepDefinition(_initialSteps[i], syntaxTree.Root));
            }
        }
    }
}