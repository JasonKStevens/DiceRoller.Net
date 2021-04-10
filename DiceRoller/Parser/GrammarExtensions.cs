using Irony.Parsing;

namespace DiceRoller.Parser
{
    public class GrammarExtensions
    {
        public void Extend(Grammar grammar)
        {
            // Terminals
            var injury = grammar.ToTerm("injury");

            // Nonterminals

            // Rules
            grammar.Root.Rule |= injury;
        }
    }
}
