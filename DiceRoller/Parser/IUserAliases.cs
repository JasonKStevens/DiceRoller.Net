using Irony.Parsing;
using System.Collections.Generic;

namespace DiceRoller.Parser;

public interface IUserAliases
{
    void AddUpdate(string userId, string label, ParseTree treeNode, string instruction);
    void Remove(string userId, string label);
    ParseTree Get(string userId, string label);
    List<string> GetAliasList(string userId);
    string Serialize();
    void Hydrate(string data, DiceRollEvaluator evaluator);
}