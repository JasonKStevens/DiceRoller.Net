using System;
using System.Collections.Generic;
using System.Linq;

namespace DiceRoller.DragonQuest
{
    public abstract class LookupTable
    {
        protected IDictionary<int, string> Map;

        public string LookupResult(int diceRoll)
        {
            if (Map == null)
                throw new Exception("Map not defined in lookup table");

            if (diceRoll < 1 || diceRoll > 100)
                throw new ArgumentException("Roll must be between 1 and 100 inclusive", nameof(diceRoll));

            var injuryKeys = Map.Keys.ToList();

            var key = injuryKeys
                .FirstOrDefault(x => diceRoll <= x);

            var value = Map[key];
            return value;
        }
    }

    public class DQLookupTables
    {
        public readonly GrievousInjuries Injuries;
        public readonly Backfires Backfires;
        public readonly FearResult Fear;

        public DQLookupTables(GrievousInjuries injuries, Backfires backfires, FearResult fear)
        {
            Injuries = injuries;
            Backfires = backfires;
            Fear = fear;
        }
    }
}
