using System.Collections.Generic;

namespace DiceRoller.DragonQuest;

public class AvianHighLocationsTable : LookupTable
{
    public AvianHighLocationsTable()
    {
        Map = new Dictionary<int, string>
        {
            {15, "Rider Legs (re-roll)†"},
            {20, "Skull"},
            {35, "Face/Eye*"},
            {55, "Neck"},
            {92, "Wing*†"},
            {97, "Thorax"},
            {99, "Abdomen"},
            {100, "Hip/Pelvis*"}
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