using DiceRoller;
using DiceRoller.Parser;
using Irony.Parsing;
using PartyDSL;
using PartyDSL.Parser;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DiceRollerCmd;

public class SettingsCommandVisitor
{
    private readonly string _prefix;
    private readonly IUserSettings _userSettings;

    public SettingsCommandVisitor(IUserSettings userSettings, string prefix)
    {
        _userSettings = userSettings;
        _prefix = prefix;
    }

    public SettingResultNode Visit(ParseTreeNode node, string userId)
    {
        if (_prefix == "settings")
        {
            switch (node.Term.Name)
            {
                case "list":
                    var settings = _userSettings.GetUserSettings(userId);
                    if (settings == null) return new SettingResultNode("User has no settings");

                    StringBuilder sb = new StringBuilder();
                    sb.AppendLine("User Settings:");
                    foreach (var setting in settings)
                    {
                        sb.AppendLine($"    {setting.Key}={setting.Value}");
                    }

                    return new SettingResultNode(sb.ToString());

                case "help":
                    return new SettingResultNode(SettingsGrammar.HelpText());

                case "equals":
                    var settingName = Visit(node.ChildNodes[1], userId).Value;
                    var settingValue = Visit(node.ChildNodes[3], userId).Value;

                    if (settingName == null)
                        return new SettingResultNode("Setting not specified");

                    _userSettings.SaveUserSetting(userId, settingName, settingValue);
                    return new SettingResultNode("Setting saved");
                case "delete":
                    settingName = Visit(node.ChildNodes[1], userId).Value;

                    if (settingName == null)
                        return new SettingResultNode("Setting not specified");

                    _userSettings.DeleteUserSetting(userId, settingName);
                    return new SettingResultNode("Setting deleted");
                case "stringtext":
                    return new SettingResultNode(node.Token.Text);

                case "expression":
                    return Visit(node.ChildNodes[0], userId);
            }
        }

        throw new InvalidOperationException($"Unrecognizable term '{node.Term.Name}'.");
    }

}