using System.Collections.Generic;

namespace DiceRoller.DragonQuest;

public class QuadrupedMidLocationsTable : LookupTable
{
    public QuadrupedMidLocationsTable()
    {
        Map = new Dictionary<int, string>
        {
            {10, "Rider Legs (re-roll)†"},
            {12, "Skull"},
            {20, "Face/Eye*"},
            {35, "Neck"},
            {65, "Shoulder*"},
            {75, "Wing*†"},
            {80, "Thorax"},
            {90, "Abdomen"},
            {95, "Hip/Pelvis*"},
            {96, "Groin"},
            {98, "Tail†"},
            {99, "Thigh*"},
            {100, "Knee*"}
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