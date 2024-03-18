using System.Collections.Generic;

namespace DiceRoller.DragonQuest;

public class HumanoidMidLocationsTable : LookupTable
{
    public HumanoidMidLocationsTable()
    {
        Map = new Dictionary<int, string>
        {
            {2, "Skull"},
            {5, "Face/Eye*"},
            {8, "Neck"},
            {16, "Shoulder*"},
            {24, "Upper Arm*"},
            {28, "Elbow*"},
            {32, "Forearm*"},
            {34, "Hand/Wrist*"},
            {40, "Wing*†"},
            {55, "Thorax"},
            {68, "Abdomen"},
            {84, "Hip/Pelvis*"},
            {85, "Groin"},
            {87, "Tail†"},
            {95, "Thigh*"},
            {97, "Knee*"},
            {99, "Calf*"},
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