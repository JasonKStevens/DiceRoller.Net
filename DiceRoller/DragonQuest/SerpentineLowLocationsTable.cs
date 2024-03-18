using System.Collections.Generic;

namespace DiceRoller.DragonQuest;

public class SerpentineLowLocationsTable : LookupTable
{
    public SerpentineLowLocationsTable()
    {
        Map = new Dictionary<int, string>
        {
            {1, "Rider Legs (re-roll)†"},
            {2, "Skull"},
            {4, "Face/Eye*"},
            {6, "Neck"},
            {10, "Wing*†"},
            {66, "Thorax"},
            {75, "Abdomen"},
            {79, "Hip/Pelvis*"},
            {70, "Groin"},
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