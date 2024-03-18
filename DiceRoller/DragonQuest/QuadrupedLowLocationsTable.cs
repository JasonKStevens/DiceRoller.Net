using System.Collections.Generic;

namespace DiceRoller.DragonQuest;

public class QuadrupedLowLocationsTable : LookupTable
{
    public QuadrupedLowLocationsTable()
    {
        Map = new Dictionary<int, string>
        {
            {1, "Rider Legs (re-roll)†"},
            {2, "Skull"},
            {4, "Face/Eye*"},
            {6, "Neck"},
            {10, "Shoulder*"},
            {15, "Wing*†"},
            {20, "Thorax"},
            {24, "Abdomen"},
            {29, "Hip/Pelvis*"},
            {30, "Groin"},
            {35, "Tail†"},
            {60, "Thigh*"},
            {80, "Knee*"},
            {95, "Calf*"},
            {100, "Foot/Hoof*"}
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