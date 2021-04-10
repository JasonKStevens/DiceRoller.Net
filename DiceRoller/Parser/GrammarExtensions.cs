using Irony.Parsing;

namespace DiceRoller.Parser
{
    public class GrammarExtensions
    {
        public void Extend(Grammar grammar)
        {
            grammar.Root.Rule |= grammar.ToTerm("injury");
            grammar.Root.Rule |= grammar.ToTerm("backfire");
        }
    }
}
