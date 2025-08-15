using Irony.Parsing;

namespace DiceRoller.Core.Parser
{
    public class EarthDawnStepDefinition
    {
        public string RollDefinition;
        public ParseTreeNode TreeNode;

        public EarthDawnStepDefinition(string rollDefinition, ParseTreeNode treeNode)
        {
            RollDefinition = rollDefinition;
            TreeNode = treeNode;
        }
    }
}