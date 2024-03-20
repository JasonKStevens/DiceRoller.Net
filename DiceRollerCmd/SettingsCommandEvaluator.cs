using DiceRoller;
using DiceRoller.Parser;
using Irony.Parsing;
using PartyDSL;
using PartyDSL.Parser;
using System;
using System.Linq;

namespace DiceRollerCmd;

public class SettingsCommandEvaluator
{
    private readonly Irony.Parsing.Parser _parser;
    private readonly IUserSettings _userSettings;
    private readonly IUserAliases _aliases;
    private readonly IPartyManager _partyManager;
    private readonly DiceRollEvaluator _diceRollEvaluator;

    public SettingsCommandEvaluator(IUserSettings userSettings, IUserAliases aliases, IPartyManager partyManager, DiceRollEvaluator diceRollEvaluator)
    {
        var grammar = new SettingsGrammar();
        var language = new LanguageData(grammar);
        _parser = new Irony.Parsing.Parser(language);

        _userSettings = userSettings;
        _aliases = aliases;
        _partyManager = partyManager;
        _diceRollEvaluator = diceRollEvaluator;
    }

    public SettingResultNode Evaluate(string prefix, string input, string userId)
    {
        var visitor = new SettingsCommandVisitor(_userSettings, prefix,_aliases, _partyManager,_diceRollEvaluator);
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