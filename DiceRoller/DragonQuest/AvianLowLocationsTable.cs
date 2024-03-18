using System.Collections.Generic;

namespace DiceRoller.DragonQuest;

public class AvianLowLocationsTable : LookupTable
{
    public AvianLowLocationsTable()
    {
        Map = new Dictionary<int, string>
        {
            {1, "Rider Legs (re-roll)†"},
            {2, "Skull"},
            {4, "Face/Eye*"},
            {6, "Neck"},
            {40, "Wing*†"},
            {45, "Thorax"},
            {48, "Abdomen"},
            {55, "Hip/Pelvis*"},
            {57, "Groin"},
            {80, "Tail†"},
            {90, "Thigh*"},
            {92, "Knee*"},
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