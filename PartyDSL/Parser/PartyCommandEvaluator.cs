using System;
using System.Linq;
using Irony.Parsing;
using System.Collections.Generic;
using DiceRoller.Parser;

namespace PartyDSL.Parser
{
    public class PartyCommandEvaluator
    {
        private readonly PartyCommandVisitor _visitor;
        private readonly Irony.Parsing.Parser _parser;
        private readonly DiceRollEvaluator _rollEvaluator;
        private readonly IPartyManager _partyManager;

        public PartyCommandEvaluator(DiceRollEvaluator evaluator, IPartyManager partyManager)
        {
            _visitor = new PartyCommandVisitor(evaluator, partyManager);
            var grammar = new PartyGrammar();
            var language = new LanguageData(grammar);
            _parser = new Irony.Parsing.Parser(language);

            _rollEvaluator = evaluator;
            _partyManager = partyManager;
        }

        public PartyResultNode Evaluate(string input)
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

            return result;
        }

    }
}