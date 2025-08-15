using System.Collections.Generic;

namespace PartyDSL
{
    public class PartyMember
    {
        public string Name;
        public List<int> LuckyNumbers;
        public PartyMember Master;

        public Dictionary<string, string> _rollDefinitions = new Dictionary<string, string>();


        public PartyMember(string name, PartyMember master = null, List<int> luckyNumbers = null)
        {
            Name = name;
            Master = master;
            LuckyNumbers = luckyNumbers ?? new List<int>();
        }

        public PartyMember()
        {
            LuckyNumbers = new List<int>();
        }

        public void SetRoll(string rollName, string roll)
        {
            _rollDefinitions[rollName.ToLower()] = roll;
        }

        public string GetRoll(string rollName)
        {
            if (!_rollDefinitions.ContainsKey(rollName.ToLower()))
                return null;

            return _rollDefinitions[rollName.ToLower()];
        }

        public void ClearLuckyNumbers()
        {
            LuckyNumbers.Clear();
        }

        public void AddLuckyNumber(int number)
        {
            LuckyNumbers.Add(number);
        }
    }

    public class PartyMemberWithRoll
    {
        public PartyMember PartyMember;
        public float Roll;
        public string RollReason;

        public PartyMemberWithRoll(PartyMember partyMember, float roll, string reason = null)
        {
            PartyMember = partyMember;
            Roll = roll;
            RollReason = reason;
        }
    }
}