namespace DiscordRollerBot
{
    public class DiscordApiConfiguration
    {
        public string Token {get;set;}
        public string CommandPrefix {get;set;}
        public bool EnableSlashCommands {get;set;} = true;
        public bool EnablePrefixCommands {get;set;} = false;  // Disabled - using slash commands only
        public bool EnableButtonInteractions {get;set;} = true;
    }
}
