using DSharpPlus.EventArgs;
using System;

namespace DiscordRollerBot
{
    public class CommandRegistration
    {
        public string CommandPrefix;
        public Func<string, string> Handler;

        public CommandRegistration(string commandPrefix, Func<string, string> handler)
        {
            CommandPrefix = commandPrefix;
            Handler = handler;
        }

        public override bool Equals(object obj)
        {
            return obj is CommandRegistration registration &&
                     CommandPrefix == registration.CommandPrefix;
        }


        public (bool, string) Handle(string commandPrefix, string instructions)
        {
            if (CommandPrefix.Equals(commandPrefix, StringComparison.OrdinalIgnoreCase))
            {
                return (true, Handler(instructions));
            }

            return (false, null);
        }
    }
}
