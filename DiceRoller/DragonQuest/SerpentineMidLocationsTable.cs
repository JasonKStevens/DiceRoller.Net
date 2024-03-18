using System.Collections.Generic;

namespace DiceRoller.DragonQuest;

public class SerpentineMidLocationsTable : LookupTable
{
    public SerpentineMidLocationsTable()
    {
        Map = new Dictionary<int, string>
        {
            {10, "Rider Legs (re-roll)†"},
            {12, "Skull"},
            {20, "Face/Eye*"},
            {35, "Neck"},
            {40, "Wing*†"},
            {65, "Thorax"},
            {85, "Abdomen"},
            {89, "Hip/Pelvis*"},
            {90, "Groin"},
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