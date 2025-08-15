using DiceRoller;
using DSharpPlus.EventArgs;
using System;
using System.Text.Json;
using System.Threading.Tasks;

namespace DiscordRollerBot
{
    public interface IDiscordApi
    {
        Task<bool> Start();
        Task<bool> Stop();

        DiscordApiStatus State {get; }
    }

}
