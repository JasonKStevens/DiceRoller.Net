using System.Collections.Generic;

namespace DiceRoller.DragonQuest;

public class QuadrupedHighLocationsTable : LookupTable
{
    public QuadrupedHighLocationsTable()
    {
        Map = new Dictionary<int, string>
        {
            {15, "Rider Legs (re-roll)†"},
            {20, "Skull"},
            {35, "Face/Eye*"},
            {55, "Neck"},
            {70, "Shoulder*"},
            {75, "Wing*†"},
            {85, "Thorax"},
            {90, "Abdomen"},
            {95, "Hip/Pelvis*"},
            {96, "Groin"},
            {97, "Tail†"},
            {98, "Thigh*"},
            {99, "Knee*"},
            {100, "Calf*"}
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