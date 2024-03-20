using Irony.Parsing;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;

namespace DiceRoller.Parser
{
    public class UserAliases : IUserAliases
    {
        private readonly  Dictionary<string, Dictionary<string, ParseTree>> _aliases = new Dictionary<string, Dictionary<string, ParseTree>>();
        private Dictionary<string, Dictionary<string, string>> _aliasInstructions = new Dictionary<string, Dictionary<string, string>>();

        private Dictionary<string, ParseTree> GetUsersAliases(string userId)
        {
            if (!_aliases.ContainsKey(userId))
            {
                _aliases.Add(userId, new Dictionary<string, ParseTree>());
            }

            return _aliases[userId];
        }

        private Dictionary<string, string> GetUsersAliasInstructions(string userId)
        {
            if (!_aliasInstructions.ContainsKey(userId))
            {
                _aliasInstructions.Add(userId, new Dictionary<string, string>());
            }

            return _aliasInstructions[userId];
        }

        public void AddUpdate(string userId, string label, ParseTree treeNode, string instruction)
        {
            userId = userId.ToLower();
            var userAliases = GetUsersAliases(userId);
            userAliases[label] = treeNode;

            var instructions = GetUsersAliasInstructions(userId);
            instructions[label] = instruction;

        }

        public void Remove(string userId, string label)
        {
            userId = userId.ToLower();
            var userAliases = GetUsersAliases(userId);

            if (userAliases.ContainsKey(label))
                userAliases.Remove(label);

            var instructions = GetUsersAliasInstructions(userId);
            if (instructions.ContainsKey(label))
                instructions.Remove(label);
        }

        public ParseTree Get(string userId, string label)
        {
            userId = userId.ToLower();
            var userAliases = GetUsersAliases(userId);

            if (userAliases.ContainsKey(label))
                return userAliases[label];

            return null;
        }

        public List<string> GetAliasList(string userId)
        {
            userId = userId.ToLower();
            var userAliases = GetUsersAliasInstructions(userId);

            var result = new List<string>();
            foreach (var alias in userAliases)
            {
                result.Add($"{alias.Key}: {alias.Value}");
            }

            return result;
        }

        public string Serialize()
        {
            var result = JsonSerializer.Serialize(_aliasInstructions);
            return result;
        }

        public void Hydrate(string data, DiceRollEvaluator evaluator)
        {
            _aliasInstructions = JsonSerializer.Deserialize<Dictionary<string, Dictionary<string, string>>>(data);
            foreach (var userAliases in _aliasInstructions)
            {
                var aliasList = GetUsersAliases(userAliases.Key);

                foreach (var userAlias in userAliases.Value)
                {
                    aliasList[userAlias.Key] = evaluator.Parse(userAlias.Value);
                }
            }
        }
    }

}