using Irony.Parsing;
using System;

namespace DiceRoller.Parser
{
    public class ExpressionGrammar : Grammar
    {
        public ExpressionGrammar() : base(false)
        {
            // Terminals
            var comment = new CommentTerminal("comment", "#", new[] {"\n"});
            var number = new NumberLiteral("number", NumberOptions.AllowLetterAfter)
            {
                DefaultIntTypes = new TypeCode[] { TypeCode.Int32 },
                DefaultFloatType = TypeCode.Single,
            };

            // Nonterminals
            var expression = new NonTerminal("expression");
            var brackets = new NonTerminal("brackets");
            var add = new NonTerminal("add");
            var subtract = new NonTerminal("subtract");
            var multiply = new NonTerminal("multiply");
            var divide = new NonTerminal("divide");
            var roll = new NonTerminal("roll");
            var dice = new NonTerminal("dice");
            var min = new NonTerminal("min");
            var repeat = new NonTerminal("repeat");
            var step  = new NonTerminal("step");

            // Rules
            expression.Rule = number | brackets | add | subtract | multiply | divide | min | roll | comment | repeat | step;
            
            brackets.Rule = "(" + expression + ")";
            add.Rule = expression + "+" + expression;
            subtract.Rule = expression + "-" + expression;
            multiply.Rule = expression + "*" + expression;
            divide.Rule = expression + "/" + expression;

            roll.Rule = dice + number | number + dice + number | dice + number + "!" | number + dice + number + "!";
            dice.Rule = new KeyTerm("d", "d") { AllowAlphaAfterKeyword = true };  // Avoid having to add whitespace either side of "d"
            repeat.Rule = new KeyTerm("repeat", "repeat") + "(" + expression + "," + number + ")";
            min.Rule = new KeyTerm("min", "min") + "(" + expression + "," + number + ")";
            step.Rule = new KeyTerm("step", "step") + expression;

            RegisterOperators(05, "min");
            RegisterOperators(08, "repeat");
            RegisterOperators(10, "+", "-");
            RegisterOperators(30, "*", "/");
            RegisterOperators(50, "step");
            RegisterOperators(40, "d");

            NonGrammarTerminals.Add(comment);
            RegisterBracePair("(", ")");
            MarkPunctuation("(", ")");
            MarkTransient(expression, brackets);

            this.Root = expression;
        }
    }
}
