using Irony.Parsing;
using System.Collections.Generic;
using System.Linq;

namespace DiceRoller.Parser
{
    public class UserAliases
    {
        private readonly  Dictionary<string, Dictionary<string, ParseTree>> _aliases = new Dictionary<string, Dictionary<string, ParseTree>>();

        private Dictionary<string, ParseTree> GetUsersAliases(string userId)
        {
            if (!_aliases.ContainsKey(userId))
            {
                _aliases.Add(userId, new Dictionary<string, ParseTree>());
            }

            return _aliases[userId];
        }

        public void AddUpdate(string userId, string label, ParseTree treeNode)
        {
            userId = userId.ToLower();
            var userAliases = GetUsersAliases(userId);

            userAliases[label] = treeNode;
        }

        public void Remove(string userId, string label)
        {
            userId = userId.ToLower();
            var userAliases = GetUsersAliases(userId);

            if (userAliases.ContainsKey(label))
                userAliases.Remove(label);
        }

        public ParseTree Get(string userId, string label)
        {
            userId = userId.ToLower();
            var userAliases = GetUsersAliases(userId);

            return userAliases[label];
        }

        public List<string> GetAliasList(string userId)
        {
            userId = userId.ToLower();
            var userAliases = GetUsersAliases(userId);

            return userAliases.Keys.ToList();
        }
    }

}