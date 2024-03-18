using System.Collections.Generic;

namespace DiceRoller.DragonQuest;

public class HumanoidLegsLocationsTable : LookupTable
{
    public HumanoidLegsLocationsTable()
    {
        Map = new Dictionary<int, string>
        {
            {2, "Forearm*"},
            {4, "Hand/Wrist*"},
            {6, "Thorax"},
            {11, "Abdomen"},
            {25, "Hip/Pelvis*"},
            {27, "Groin"},
            {33, "Tail†"},
            {63, "Thigh*"},
            {75, "Knee*"},
            {89, "Calf*"},
            {95, "Foot/Hoof*"},
            {100, "Steed (re-roll)†"}
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