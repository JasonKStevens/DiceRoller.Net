using System;
using System.Collections.Generic;
using System.Linq;

namespace PartyDSL
{
    public class Party
    {
        public string Name;
        public Dictionary<string, PartyMember> Members = new Dictionary<string, PartyMember>();


        public Party(string name)
        {
            Name = name;
        }

        public Party()
        {
        }

        public PartyMember GetMember(string memberName)
        {
            if (!Members.ContainsKey(memberName.ToLower()))
                return null;

            return Members[memberName.ToLower()];
        }

        public IEnumerable<PartyMember> GetAll()
        {
            return Members.Values;
        }

        public PartyMember AddMember(string memberName, PartyMember master = null)
        {
            if (Members.ContainsKey(memberName.ToLower()))
                throw new InvalidOperationException($"{memberName} is already a party member");

            Members[memberName.ToLower()] = new PartyMember(memberName, master);

            return Members[memberName.ToLower()];
        }

        public PartyMember RemoveMember(string memberName)
        {
            if (!Members.ContainsKey(memberName.ToLower()))
                throw new InvalidOperationException($"{memberName} is not a party member");

            var result = Members[memberName.ToLower()];

            var ally = Members.Values.FirstOrDefault(x => x.Master == result);

            if (ally != null)
            {
                throw new InvalidOperationException($"Cannot remove {memberName} as their ally {ally.Name} relies on them!");
            }
            Members.Remove(memberName.ToLower());

            return result;
        }


    }
}