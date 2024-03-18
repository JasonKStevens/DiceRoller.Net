using System.Collections.Generic;

namespace DiceRoller.DragonQuest;

public class HumanoidArmsLocationsTable : LookupTable
{
    public HumanoidArmsLocationsTable()
    {
        Map = new Dictionary<int, string>
        {
            {1, "Skull"},
            {5, "Face/Eye*"},
            {7, "Neck"},
            {21, "Shoulder*"},
            {33, "Upper Arm*"},
            {39, "Elbow*"},
            {69, "Forearm*"},
            {89, "Hand/Wrist*"},
            {95, "Wing*†"},
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