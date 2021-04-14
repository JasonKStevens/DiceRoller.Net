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

            var numericExpression = new NonTerminal("numeric-expression");
            var add = new NonTerminal("add");
            var subtract = new NonTerminal("subtract");
            var multiply = new NonTerminal("multiply");
            var divide = new NonTerminal("divide");
            var roll = new NonTerminal("roll");
            var dice = new NonTerminal("dice");
            var min = new NonTerminal("min");
            var repeat = new NonTerminal("repeat");
            var step  = new NonTerminal("step");

            var inequalityExpression = new NonTerminal("inequality-expression");
            var lt = new NonTerminal("less-than");
            var lteq = new NonTerminal("less-than-or-equal-to");
            var equalTo = new NonTerminal("equal-to");
            var gteq = new NonTerminal("greater-than-or-equal-to");
            var gt = new NonTerminal("greater-than");

            // Rules
            expression.Rule = numericExpression | inequalityExpression | comment;

            numericExpression.Rule = number | brackets | roll;
            numericExpression.Rule |= add | subtract | multiply | divide;
            numericExpression.Rule |= min | repeat | step;

            inequalityExpression.Rule = lt | lteq | equalTo | gteq | gt;
            inequalityExpression.Rule |= repeat;
            
            brackets.Rule = "(" + numericExpression + ")";
            add.Rule = numericExpression + "+" + numericExpression | "+" + numericExpression;
            subtract.Rule = numericExpression + "-" + numericExpression | "-" + numericExpression;
            multiply.Rule = numericExpression + "*" + numericExpression;
            divide.Rule = numericExpression + "/" + numericExpression;
            lt.Rule = numericExpression + "<" + numericExpression;
            lteq.Rule = numericExpression + "<=" + numericExpression;
            equalTo.Rule = numericExpression + "=" + numericExpression;
            gteq.Rule = numericExpression + ">=" + numericExpression;
            gt.Rule = numericExpression + ">" + numericExpression;

            roll.Rule = dice + number | number + dice + number | dice + number + "!" | number + dice + number + "!";
            dice.Rule = new KeyTerm("d", "d") { AllowAlphaAfterKeyword = true };  // Avoid having to add whitespace either side of "d"
            var repeatTerm = new KeyTerm("repeat", "repeat");
            repeat.Rule =
                repeatTerm + "(" + numericExpression + "," + number + ")" |
                repeatTerm + "(" + inequalityExpression + "," + number + ")";
            min.Rule = new KeyTerm("min", "min") + "(" + numericExpression + "," + number + ")";
            step.Rule = new KeyTerm("step", "step") + expression;

            // Operators
            RegisterOperators(10, "min");
            RegisterOperators(20, "repeat");
            RegisterOperators(30, "<", "<=", "=", ">=", ">");
            RegisterOperators(40, "+", "-");
            RegisterOperators(50, "*", "/");
            RegisterOperators(60, "step");
            RegisterOperators(70, "d");

            NonGrammarTerminals.Add(comment);
            RegisterBracePair("(", ")");
            MarkPunctuation("(", ")");
            MarkTransient(numericExpression, inequalityExpression, brackets);

            Root = expression;
        }
    }
}
