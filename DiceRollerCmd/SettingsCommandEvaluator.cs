using DiceRoller;
using Irony.Parsing;
using PartyDSL.Parser;
using System;
using System.Linq;

namespace DiceRollerCmd;

public class SettingsCommandEvaluator
{
    private readonly Irony.Parsing.Parser _parser;
    private readonly IUserSettings _userSettings;

    public SettingsCommandEvaluator(IUserSettings userSettings)
    {
        var grammar = new SettingsGrammar();
        var language = new LanguageData(grammar);
        _parser = new Irony.Parsing.Parser(language);

        _userSettings = userSettings;
    }

    public SettingResultNode Evaluate(string prefix, string input, string userId)
    {
        var visitor = new SettingsCommandVisitor(_userSettings, prefix);
        var syntaxTree = _parser.Parse(input);

        if (syntaxTree.HasErrors())
        {
            var messages = syntaxTree.ParserMessages.Select(m => m.Message);
            var detail = string.Join(Environment.NewLine + "- ", messages);
            var message = $"Parser errors:{Environment.NewLine}- {detail}";
            throw new InvalidOperationException(message);
        }

        var result = visitor.Visit(syntaxTree.Root, userId);

        return result;
    }
}