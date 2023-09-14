using System.Collections.Generic;

namespace DiceRoller.DragonQuest
{
    public class Backfires : LookupTable
    {
        public Backfires()
        {
            Map = new Dictionary<int, string>
            {
                { 9, "Fatigue loss equal to that already expended." },
                { 16, "Fatigue loss equal to twice that already expended." },
                { 21, "Fatigue loss equal to 3 times that already expended." },
                { 24, "Fatigue loss equal to 4 times that already expended." },
                { 25, "Fatigue loss equal to 5 times that already expended." },
                { 30, "The Adept becomes the target of the spell." },
                { 32, "The Adept becomes the target of the spell with some or all effects doubled." },
                { 33, "The Adept becomes the target of the spell with some or all effects tripled." },
                { 35, "The Adept becomes the target of the spell and the spell is delayed by D10 pulses." },
                { 36, "The Adept becomes the target of the spell and the spell is delayed by D100 pulses." },
                { 39, "The spell has opposite or different effect to that which it was designed." },
                { 41, "The spell’s effects are delayed by D10 pulses." },
                { 42, "The spell’s effects are delayed by D100 pulses." },
                { 45, "The spell’s effects are intermittent with D10 pulses or minutes on, followed by D10 pulses or minutes off." },
                { 48, "The spell affects a random target or area within range, or goes in a random direction." },
                { 49, "The spell affects a random target or area within twice range." },
                { 50, "The spell affects a random target or area within three times range." },
                { 51, "The spell is cast at random as though the caster is a random entity within D10 hexes." },
                { 53, "The spell affects a random target or area within range with some or all effects doubled." },
                { 54, "The spell’s effects are delayed by D10 pulses and affects a random target or area." },
                { 55, "The spell’s effects are delayed by D100 pulses and affects a random target or area." },
                { 57, "The spell works with some or all effects halved." },
                { 58, "The spell works as normal." },
                { 59, "The spell works with some or all effects doubled." },
                { 60, "The spell works with some or all effects tripled." },
                { 61, "Blind for D10 pulses." },
                { 62, "Blind for D10 × D10 minutes." },
                { 63, "Blind for D10 × D10 hours." },
                { 64, "Blind for D10 days." },
                { 65, "Deaf for D10 pulses." },
                { 66, "Deaf for D10 × D10 minutes." },
                { 67, "Deaf for D10 × D10 hours." },
                { 68, "Deaf for D10 days." },
                { 69, "Mute for D10 pulses." },
                { 70, "Mute for D10 × D10 minutes." },
                { 71, "Mute for D10 × D10 hours." },
                { 72, "Mute for D10 days." },
                { 73, "Lose smell and taste for D10 days." },
                { 74, "Lose smell and taste for D10 × D10 days." },
                { 75, "Lose tactile sense for D10 days." },
                { 76, "Lose tactile sense for D10 × D10 days." },
                { 77, "Insomnia such that only 1 Fatigue is recovered for each hour of sleep for D10 days." },
                { 78, "Insomnia such that only 1 Fatigue is recovered for each hour of sleep for D10 × D10 days." },
                { 80, "A virulent skin disease halves Physical Beauty and causes intense itching which will increase the difficulty of concentration checks by 1, until stopped by Cure Disease." },
                { 81, "Wasting disease causes -1 Strength and -1 Endurance per day until stopped by Cure Disease. The Strength and Endurance lost will be recovered at 1 point per day, or by being treated by Repair Muscles." },
                { 83, "Periodic muscle spasms lasting D10 pulses cause a loss of 1 Fatigue each pulse. There is D10 × D10 minutes between spasms. This can be cured by Repair Muscles." },
                { 84, "A deep sleep for D10 pulses." },
                { 85, "A deep sleep for D10 × D10 minutes." },
                { 87, "Recurring migraines cause a loss of 2 Magical Aptitude and 2 Willpower. Each minute of concentration requires a 4 × Willpower concentration check. The effects can be treated by Soothe Pain and cured by Repair Vital Organs." },
                { 88, "Periodic hallucinations for D10 hours. Each hallucination lasts D10 pulses and there is D10 × D10 minutes between them. Can be cured by Repair Vital Organs." },
                { 90, "Arthritis causes -4 Dexterity, -4 Agility and increases by 1 per hour the Fatigue loss due to exercise, until treated by Repair Tissues." },
                { 92, "Enfeeblement causes -4 Strength, -4 Endurance and doubles the Fatigue loss due to exercise, until treated by Repair Muscles." },
                { 93, "Asthma causes TMR to be halved, doubles the Fatigue loss due to exercise, and the Adept cannot perform strenuous exercise until treated by Repair Vital Organs." },
                { 95, "Creeping senility will cause a loss of 1 Magical Aptitude every two days until treated by Regenerate Vital Organs." },
                { 96, "Partial Amnesia causes the loss of all Magical abilities for D10 days." },
                { 97, "Partial Amnesia causes the loss of all Skills (excluding Magic and Weapons) for D10 days." },
                { 98, "Partial Amnesia causes the loss of all memories from the past 2D10 months. The Adept will operate at lower ranks in the abilities that have been ranked during this period. The memories will return at a rate of 1 month each day." },
                { 99, "Total Amnesia causes the loss of all memories for D5 × D5 days. All magic and skills other than the primary language will be lost, and all weapon ranks will be halved (round down) or lost if Rank 0. The Adept’s original personality will come to the fore and they may need to make a reaction roll to determine their initial feelings towards each person." },
                { 100, "Roll two more times and apply both effects." },
            };
        }
    }
}


