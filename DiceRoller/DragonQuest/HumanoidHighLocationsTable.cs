using System.Collections.Generic;

namespace DiceRoller.DragonQuest;

public class HumanoidHighLocationsTable : LookupTable
{
    public HumanoidHighLocationsTable()
    {
        Map = new Dictionary<int, string>
        {
            {16, "Skull"},
            {27, "Face/Eye*"},
            {43, "Neck"},
            {75, "Shoulder*"},
            {79, "Upper Arm*"},
            {81, "Elbow*"},
            {85, "Forearm*"},
            {89, "Hand/Wrist*"},
            {94, "Wing*†"},
            {97, "Thorax"},
            {98, "Abdomen"},
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