using System.Collections.Generic;
using System.Text.Json;

namespace DiceRoller;

public interface IUserSettings
{
    string GetUserSetting(string userId, string setting);
    void SaveUserSetting(string userId, string setting, string value);
    void DeleteUserSetting(string userId, string setting);

    Dictionary<string,string> GetUserSettings(string userId);

    string Serialize();
    void Hydrate(string json);
}

public class UserSettings : IUserSettings
{
    private Dictionary<string,Dictionary<string,string>> _settings = new Dictionary<string,Dictionary<string,string>>();

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

    public void DeleteUserSetting(string userId, string setting)
    {
        if (!_settings.ContainsKey(userId))
        {
            _settings[userId] = new Dictionary<string, string>();
        }

        var userSettings = _settings[userId];
        userSettings.Remove(setting);
    }

    public Dictionary<string, string> GetUserSettings(string userId)
    {
        if (_settings.ContainsKey(userId))
        {
            return _settings[userId];
        }

        return null;
    }

    public string Serialize()
    {
        return JsonSerializer.Serialize(_settings);
    }

    public void Hydrate(string json)
    {
        _settings = JsonSerializer.Deserialize<Dictionary<string, Dictionary<string, string>>>(json);
    }
}