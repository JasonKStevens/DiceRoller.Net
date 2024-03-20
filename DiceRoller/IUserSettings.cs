using System.Collections.Generic;

namespace DiceRoller;

public interface IUserSettings
{
    string GetUserSetting(string userId, string setting);
    void SaveUserSetting(string userId, string setting, string value);

    Dictionary<string,string> GetUserSettings(string userId);
}

public class UserSettings : IUserSettings
{
    private readonly Dictionary<string,Dictionary<string,string>> _settings = new Dictionary<string,Dictionary<string,string>>();

    public string GetUserSetting(string userId, string setting)
    {
        if (_settings.ContainsKey(userId))
        {
            var userSettings = _settings[userId];
            if (userSettings.ContainsKey(setting))
                return userSettings[setting];
        }

        return null;
    }

    public void SaveUserSetting(string userId, string setting, string value)
    {
        if (!_settings.ContainsKey(userId))
        {
            _settings[userId] = new Dictionary<string, string>();
        }

        var userSettings = _settings[userId];
        userSettings[setting] = value;

    }

    public Dictionary<string, string> GetUserSettings(string userId)
    {
        if (_settings.ContainsKey(userId))
        {
            return _settings[userId];
        }

        return null;
    }
}