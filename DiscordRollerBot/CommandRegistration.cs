using DSharpPlus.EventArgs;
using System;

namespace DiscordRollerBot
{
    public class CommandRegistration
    {
        public string CommandPrefix;
        public Func<string, MessageCreateEventArgs, string> Handler;

        public CommandRegistration(string commandPrefix, Func<string, MessageCreateEventArgs, string> handler)
        {
            CommandPrefix = commandPrefix;
            Handler = handler;
        }

        public override bool Equals(object obj)
        {
            return obj is CommandRegistration registration &&
                     CommandPrefix == registration.CommandPrefix;
        }


        public (bool, string) Handle(string commandPrefix, string instructions, MessageCreateEventArgs e)
        {
            if (CommandPrefix.Equals(commandPrefix, StringComparison.OrdinalIgnoreCase))
            {
                return (true, Handler(instructions, e));
            }

            return (false, null);
        }
    }
}
