using System;
using System.Linq;
using Irony.Parsing;
using System.Collections.Generic;
using DiceRoller.Parser;

namespace PartyDSL.Parser
{
    public class PartyCommandEvaluator
    {
        private readonly Irony.Parsing.Parser _parser;
        private readonly DiceRollEvaluator _rollEvaluator;
        private readonly IPartyManager _partyManager;
        private readonly DiceRollEvaluator _evaluator;

        public PartyCommandEvaluator(DiceRollEvaluator evaluator, IPartyManager partyManager)
        {
            var grammar = new PartyGrammar();
            var language = new LanguageData(grammar);
            _parser = new Irony.Parsing.Parser(language);

            _rollEvaluator = evaluator;
            _partyManager = partyManager;
            _evaluator = evaluator;
        }

        public PartyResultNode Evaluate(string prefix, string input)
        {
            var visitor = new PartyCommandVisitor(_evaluator, _partyManager, prefix);
            var syntaxTree = _parser.Parse(input);

            if (syntaxTree.HasErrors())
            {
                var messages = syntaxTree.ParserMessages.Select(m => m.Message);
                var detail = string.Join(Environment.NewLine + "- ", messages);
                var message = $"Parser errors:{Environment.NewLine}- {detail}";
                throw new InvalidOperationException(message);
            }

            var result = visitor.Visit(syntaxTree.Root);

            return result;
        }

        public bool HasParty(string party)
        {
            return _partyManager.GetAll().Any(x => x.Name.Equals(party, StringComparison.OrdinalIgnoreCase));
        }

    }
}