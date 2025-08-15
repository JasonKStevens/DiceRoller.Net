using System;
using System.Linq;
using Irony.Parsing;
using DiceRoller.Dice;
using System.Collections.Generic;
using DiceRoller.Core.Abstractions;
using DiceRoller.Core.Functions;

namespace DiceRoller.Core.Parser
{
    /// <summary>
    /// Legacy dice roll evaluator that delegates to the new separated parser and evaluator
    /// Maintains backward compatibility with existing code
    /// </summary>
    public class DiceRollEvaluator
    {
        private readonly IDiceExpressionParser _parser;
        private readonly IDiceExpressionEvaluator _evaluator;
        private readonly DiceRollVisitor _visitor;
        private readonly Irony.Parsing.Parser _legacyParser;

        public DiceRollEvaluator(IRandomNumberGenerator randomNumberGenerator, IFunctionRegistry functionRegistry = null, IGameSystemRouter gameSystemRouter = null)
        {
            // Create new abstractions
            _parser = new DiceExpressionParser();
            _evaluator = new DiceExpressionEvaluator(randomNumberGenerator, _parser, functionRegistry, gameSystemRouter);
            
            // Keep legacy visitor for step evaluation
            _visitor = new DiceRollVisitor(randomNumberGenerator, EvaluateStep, functionRegistry, gameSystemRouter);
            var grammar = new ExpressionGrammar();
            GrammarExtensions.Extend(grammar);

            var language = new LanguageData(grammar);
            _legacyParser = new Irony.Parsing.Parser(language);

            ParseSteps();
        }

        private void ParseSteps()
        {
            for (int i = 0; i < _initialSteps.Count; i++)
            {
                var syntaxTree = _legacyParser.Parse(_initialSteps[i]);

                _parsedSteps.Add(i+1, new EarthDawnStepDefinition(_initialSteps[i], syntaxTree.Root));
                
            }
        }

        public DiceResultNode Evaluate(string input)
        {
            // Delegate to new parser and evaluator
            var syntaxTree = _parser.Parse(input);
            return _evaluator.Evaluate(syntaxTree);
        }

        public DiceResultNode Evaluate(ParseTree node)
        {
            // Delegate to new evaluator
            return _evaluator.Evaluate(node);
        }

        public ParseTree Parse(string input)
        {
            // Delegate to new parser
            return _parser.Parse(input);
        }

        public DiceResultNode EvaluateStep(int stepNumber)
        {
            if (!_parsedSteps.ContainsKey(stepNumber))
            {
                var adjustedStep = stepNumber-_repeatingSeqStart;
                var scales = adjustedStep / _repeatingSeqCycleCount;
                adjustedStep = adjustedStep % _repeatingSeqCycleCount;

                var baseRoll = _repeatingSteps[adjustedStep];
                if (scales > 0)
                {
                    baseRoll = $"{scales}{_repeatingDie} + {baseRoll}";
                }

                _parsedSteps.Add(stepNumber, new EarthDawnStepDefinition(baseRoll, _legacyParser.Parse(baseRoll).Root));
            }

            var rollDef = _parsedSteps[stepNumber];
            var result = _visitor.Visit(rollDef.TreeNode);
            result.TypedResult.SubText.Insert(0, TypedResult.NewSimpleResult(NodeType.StepFuncDef,rollDef.RollDefinition));

            return new DiceResultNode(result.Value, rollDef.RollDefinition + ": " + result.Breakdown, result.TypedResult);
        }


        
        private List<string> _initialSteps = new List<string>{
            "1d4!-2",
            "1d4!-1",
            "1d4!",
            "1d6!",
            "1d8!",
            "1d10!",
            "1d12!"
        };

        private List<string> _repeatingSteps = new List<string>{
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

        private string _repeatingDie = "d20!";
        private int _repeatingSeqStart = 8;
        private int _repeatingSeqCycleCount = 11;

        private Dictionary<int, EarthDawnStepDefinition> _parsedSteps = new Dictionary<int, EarthDawnStepDefinition>();

        
    }

}