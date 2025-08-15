using DiceRoller.Core.Parser;

namespace DiceRoller
{
    public interface ITypedResultOutputVisitor
    {
        public string Visit(TypedResult result, int depth);
    }
}