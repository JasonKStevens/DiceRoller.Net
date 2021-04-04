﻿using System;
using System.Linq;
using Irony.Parsing;
using DiceRoller.Dice;

namespace DiceRoller.Parser
{
  public class Evaluator
  {
    private readonly Visitor _visitor;

    public Evaluator(IRandomNumberGenerator randomNumberGenerator)
    {
      _visitor = new Visitor(randomNumberGenerator);
    }

    public ResultNode Evaluate(string input)
    {
      var language = new LanguageData(new ExpressionGrammar());
      var parser = new Irony.Parsing.Parser(language);
      var syntaxTree = parser.Parse(input);

      if (syntaxTree.HasErrors())
      {
        var messages = syntaxTree.ParserMessages.Select(m => m.Message);
        var detail = string.Join(Environment.NewLine + "- ", messages);
        var message = $"Parser errors:{Environment.NewLine}- {detail}";
        throw new InvalidOperationException(message);
      }

      return _visitor.Visit(syntaxTree.Root);
    }
  }
}