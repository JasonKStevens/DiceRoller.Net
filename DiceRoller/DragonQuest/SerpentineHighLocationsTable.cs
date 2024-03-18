using System.Collections.Generic;

namespace DiceRoller.DragonQuest;

public class SerpentineHighLocationsTable : LookupTable
{
    public SerpentineHighLocationsTable()
    {
        Map = new Dictionary<int, string>
        {
            {15, "Rider Legs (re-roll)†"},
            {20, "Skull"},
            {35, "Face/Eye*"},
            {60, "Neck"},
            {65, "Wing*†"},
            {85, "Thorax"},
            {99, "Abdomen"},
            {100, "Tail†"}
        };
    }

    public override int GetMaxRoll()
    {
        return 100;
    }

    public override string GetRoll()
    {
        return "d100";
    }
}