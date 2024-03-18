using System.Collections.Generic;

namespace DiceRoller.DragonQuest;

public class AvianMidLocationsTable : LookupTable
{
    public AvianMidLocationsTable()
    {
        Map = new Dictionary<int, string>
        {
            {10, "Rider Legs (re-roll)†"},
            {12, "Skull"},
            {20, "Face/Eye*"},
            {35, "Neck"},
            {80, "Wing*†"},
            {90, "Thorax"},
            {95, "Abdomen"},
            {97, "Hip/Pelvis*"},
            {99, "Groin"},
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