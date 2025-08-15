using DiceRoller.DragonQuest;
using System.Collections.Generic;

namespace DiceRoller.Heroes
{
    public class LocationTable : LookupTable
    {
        public override int GetMaxRoll()
        {
            return 18;
        }

        public override string GetRoll()
        {
            return "3d6";
        }

        public LocationTable()
        {
            Map = new Dictionary<int, string>
            {
                {3, "Head - Stun:x5 NStun:x2 Body:x2 ToHit:-8"},
                {4, "Head - Stun:x5 NStun:x2 Body:x2 ToHit:-8"},
                {5, "Head - Stun:x5 NStun:x2 Body:x2 ToHit:-8"},
                {6, "Hands - Stun:x1 NStun:x½ Body:x½ ToHit:-6"},
                {7, "Arms - Stun:x2 NStun:x½ Body:x½ ToHit:-5"},
                {8, "Arms - Stun:x2 NStun:x½ Body:x½ ToHit:-5"},
                {9, "Shoulders - Stun:x3 NStun:x1 Body:x1 ToHit:-5"},
                {10, "Chest - Stun:x3 NStun:x1 Body:x1 ToHit:-3"},
                {11, "Chest - Stun:x3 NStun:x1 Body:x1 ToHit:-3"},
                {12, "Stomach - Stun:x4 NStun:x1½ Body:x1 ToHit:-7"},
                {13, "Vitals - Stun:x4 NStun:x1½ Body:x2 ToHit:-8"},
                {14, "Thighs - Stun:x2 NStun:x1 Body:x1 ToHit:-4"},
                {15, "Legs - Stun:x2 NStun:x½ Body:x½ ToHit:-6"},
                {16, "Legs - Stun:x2 NStun:x½ Body:x½ ToHit:-6"},
                {17, "Feet - Stun:x1 NStun:x½ Body:x½ ToHit:-8"},
                {18, "Feet - Stun:x1 NStun:x½ Body:x½ ToHit:-8"},
            };
        }
    }
}