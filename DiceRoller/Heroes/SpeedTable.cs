﻿using DiceRoller.DragonQuest;
using System.Collections.Generic;

namespace DiceRoller.Heroes
{
    public class SpeedTable : LookupTable
    {
        protected int MaxRoll = 10;

        public SpeedTable()
        {
            Map = new Dictionary<int, string>
            {
                {1, "7"},
                {2, "6, 12"},
                {3, "4, 8, 12"},
                {4, "3, 6, 9, 12"},
                {5, "3, 5, 8, 10, 12"},
                {6, "2, 4, 6, 8, 10, 12"},
                {7, "2, 4, 6, 7, 9, 11, 12"},
                {8, "2, 3, 5, 6, 8, 9, 11, 12"},
                {9, "2, 3, 4, 6, 7, 8, 10, 11, 12"},
                {10, "2, 3, 4, 5, 6, 8, 9, 10, 11, 12"},
                {11, "2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12"},
                {12, "1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12"},
            };
        }
    }
}
