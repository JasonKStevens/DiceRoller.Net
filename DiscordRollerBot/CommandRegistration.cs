using DiceRoller;
using DSharpPlus.EventArgs;
using System;

namespace DiscordRollerBot
{
    public class CommandRegistration
    {
        public string CommandPrefix;
        public Func<BotUser, string, string> Handler;

        public CommandRegistration(string commandPrefix, Func<BotUser, string, string> handler)
        {
            CommandPrefix = commandPrefix;
            Handler = handler;
        }

        public override bool Equals(object obj)
        {
            return obj is CommandRegistration registration &&
                     CommandPrefix == registration.CommandPrefix;
        }


        public (bool, string) Handle(BotUser user, string commandPrefix, string instructions)
        {
            if (CommandPrefix.Equals(commandPrefix, StringComparison.OrdinalIgnoreCase))
            {
                return (true, Handler(user, instructions));
            }

            return (false, null);
        }
    }
}
