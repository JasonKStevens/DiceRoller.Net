using System;
using System.Linq;
using DiceRoller.Core.Abstractions;
using DiceRoller.Core.Functions;
using DiceRoller.Dice;
using Irony.Parsing;

namespace DiceRoller.Core.Parser
{
    /// <summary>
    /// Evaluator implementation for parsed dice expressions
    /// </summary>
    public class DiceExpressionEvaluator : IDiceExpressionEvaluator
    {
        private readonly DiceRollVisitor _visitor;
        private readonly IDiceExpressionParser _parser;

        /// <summary>
        /// Initializes a new instance of the DiceExpressionEvaluator
        /// </summary>
        /// <param name="randomNumberGenerator">The random number generator to use</param>
        /// <param name="parser">The parser for step functions</param>
        /// <param name="functionRegistry">Optional function registry for custom functions</param>
        /// <param name="gameSystemRouter">Optional game system router for handling game-specific commands</param>
        public DiceExpressionEvaluator(IRandomNumberGenerator randomNumberGenerator, IDiceExpressionParser parser, IFunctionRegistry functionRegistry = null, IGameSystemRouter gameSystemRouter = null)
        {
            _parser = parser ?? throw new ArgumentNullException(nameof(parser));
            
            // Create the visitor with step evaluation support, function registry, and game system router
            _visitor = new DiceRollVisitor(randomNumberGenerator, EvaluateStep, functionRegistry, gameSystemRouter);
        }

        /// <summary>
        /// Evaluates a parsed syntax tree
        /// </summary>
        /// <param name="parseTree">The parsed syntax tree</param>
        /// <returns>The result of the dice roll</returns>
        public DiceResultNode Evaluate(ParseTree parseTree)
        {
            if (parseTree == null)
                throw new ArgumentNullException(nameof(parseTree));

            return Evaluate(parseTree.Root, parseTree.Tokens);
        }

        /// <summary>
        /// Evaluates a parsed syntax tree root node
        /// </summary>
        /// <param name="rootNode">The root node of the syntax tree</param>
        /// <returns>The result of the dice roll</returns>
        public DiceResultNode Evaluate(ParseTreeNode rootNode)
        {
            if (rootNode == null)
                throw new ArgumentNullException(nameof(rootNode));

            return Evaluate(rootNode, null);
        }

        private DiceResultNode Evaluate(ParseTreeNode rootNode, TokenList tokens)
        {
            var result = _visitor.Visit(rootNode);

            // Handle comments if tokens are available
            if (tokens != null)
            {
                var comments = tokens.Where(x => x.Terminal is CommentTerminal).FirstOrDefault();
                if (comments != null)
                {
                    result.TypedResult.SubText.Add(TypedResult.NewSimpleResult(NodeType.Comment, comments.Text));
                    result = new DiceResultNode(result.Value, result.Breakdown + " " + comments.Text, result.TypedResult);
                }
            }

            return result;
        }

        private DiceResultNode EvaluateStep(int stepNumber)
        {
            if (!_parser.SupportsStepFunctions)
                throw new NotSupportedException("Step functions are not supported by the current parser");

            var stepTree = _parser.ParseStep(stepNumber);
            var result = _visitor.Visit(stepTree.Root);
            
            // Add step definition to the result
            var stepDefinition = GetStepDefinition(stepNumber);
            result.TypedResult.SubText.Insert(0, TypedResult.NewSimpleResult(NodeType.StepFuncDef, stepDefinition));

            return new DiceResultNode(result.Value, stepDefinition + ": " + result.Breakdown, result.TypedResult);
        }

        private string GetStepDefinition(int stepNumber)
        {
            // This would ideally come from the parser, but for now we'll recreate the logic
            var initialSteps = new[]
            {
                "1d4!-2", "1d4!-1", "1d4!", "1d6!", "1d8!", "1d10!", "1d12!"
            };

            if (stepNumber <= initialSteps.Length)
                return initialSteps[stepNumber - 1];

            var repeatingSteps = new[]
            {
                "2d6!", "1d8! + 1d6!", "2d8!", "1d10! + 1d8!", "2d10!",
                "1d12! + 1d10!", "2d12!", "1d12! + 2d6!", "1d12! + 1d8! + 1d6!",
                "1d12! + 2d8!", "1d12! + 1d10! + 1d8!"
            };

            const int repeatingSeqStart = 8;
            const int repeatingSeqCycleCount = 11;
            const string repeatingDie = "d20!";

            var adjustedStep = stepNumber - repeatingSeqStart;
            var scales = adjustedStep / repeatingSeqCycleCount;
            adjustedStep = adjustedStep % repeatingSeqCycleCount;

            var baseRoll = repeatingSteps[adjustedStep];
            if (scales > 0)
            {
                baseRoll = $"{scales}{repeatingDie} + {baseRoll}";
            }

            return baseRoll;
        }
    }
}