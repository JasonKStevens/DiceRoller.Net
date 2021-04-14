using System;
using System.Linq;
using Irony.Parsing;
using DiceRoller.Dice;
using System.Collections.Generic;

namespace DiceRoller.Parser
{
    public class Evaluator
    {
        private readonly Visitor _visitor;
        private readonly Irony.Parsing.Parser _parser;

        public Evaluator(IRandomNumberGenerator randomNumberGenerator)
        {
            _visitor = new Visitor(randomNumberGenerator, EvaluateStep);
            var grammar = new ExpressionGrammar();
            GrammarExtensions.Extend(grammar);

            var language = new LanguageData(grammar);
            _parser = new Irony.Parsing.Parser(language);

            ParseSteps();
        }

        private void ParseSteps()
        {
            for (int i = 0; i < _initialSteps.Count; i++)
            {
                var syntaxTree = _parser.Parse(_initialSteps[i]);

                _parsedSteps.Add(i+1, new EarthDawnStepDefinition(_initialSteps[i], syntaxTree.Root));
                
            }
        }

        public ResultNode Evaluate(string input)
        {
            var syntaxTree = _parser.Parse(input);

            if (syntaxTree.HasErrors())
            {
                var messages = syntaxTree.ParserMessages.Select(m => m.Message);
                var detail = string.Join(Environment.NewLine + "- ", messages);
                var message = $"Parser errors:{Environment.NewLine}- {detail}";
                throw new InvalidOperationException(message);
            }

            var result = _visitor.Visit(syntaxTree.Root);

            var comments = syntaxTree.Tokens.Where(x => x.Terminal is CommentTerminal).FirstOrDefault();
            if (comments != null)
            {
                result = new ResultNode(result.Value, result.Breakdown + " " + comments.Text);
            }

            return result;
        }

        public ResultNode EvaluateStep(int stepNumber)
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

                _parsedSteps.Add(stepNumber, new EarthDawnStepDefinition(baseRoll, _parser.Parse(baseRoll).Root));
            }

            var rollDef = _parsedSteps[stepNumber];
            var result = _visitor.Visit(rollDef.TreeNode);

            return new ResultNode(result.Value, rollDef.RollDefinition + ": " + result.Breakdown);
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