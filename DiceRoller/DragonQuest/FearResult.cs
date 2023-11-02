using System.Collections.Generic;

namespace DiceRoller.DragonQuest
{
    public class FearResult : LookupTable
    {
        public override int GetMaxRoll()
        {
            return 200;
        }

        public FearResult()
        {
            Map = new Dictionary<int, string>
            {
                { 20, "[Wary] - The target will not voluntarily approach the source of their fear. If they are not aware of the source they will be very cautious and seek to optimise safety." },
                { 25, "[Berserk]- They immediately charge to attack the object of their rage. If the source is not apparent they will charge about noisily looking for it. Add '+10 to Strike Chance and -10 to Defence'." },
                { 75, "[Panic]- They will attempt to maximise their safety in relation to the source of their fear. This usually involves fleeing as rapidly as possible, but could also include cowering in the centre of the party, curling up in a small ball, hiding under a bed, etc. While a state of panic prevails, some sanity is present and the target would not normally do anything suicidal (e.g. running over the edge of a cliff) but they might use abilities to increase their safety (e.g. flying away). If the target wishes to use an ability, (e.g. casting a spell) the GM should give a suitable negative modifier to their base chance (e.g. -20)." },
                { 90, "[Frozen]- They may take no action until snapped out of it (e.g. slapped on the face, attacked, etc). The target can attempt to break out of it themselves by making a '1 × WP check per pulse'. On recovery, the target rolls again at '-30' (with no other modifiers) to determine their next action. Add '+10 to subsequent rolls on the fright table'." },
                { 95, "[Hysterical]- They stand and scream and may take no other action until snapped out of it (e.g. slapped on the face, attacked, etc). The target can attempt to break out of it themselves by making a '1 × WP check per pulse'. On recovery, roll again at '-20' (with no other modifiers). Add '+10 to subsequent rolls on the fright table'." },
                { 100, "[Catatonic]- Target becomes catatonic. Their hair turns white and they may take no other action until snapped out of it (e.g. slapped on the face, attacked, etc). The target can attempt to break out of it themselves by making a '1 × WP check per pulse'. On recovery, roll again at '-20' (with no other modifiers). Add '+10 to subsequent rolls on the fright table'." },
                { 110, "[Faints]- The target faints into unconsciousness and loses [5 Fatigue]. At the end of each minute they roll '1 × WP in order to regain consciousness'. Add '+10 to subsequent rolls on the fright table'." },
                { 115, "[Collapses]- The target collapses into unconsciousness and loses [all of their Fatigue]. After ('30 - Endurance') minutes, or being tended by a Healer, they will regain consciousness. All their Characteristics and Ranks will be reduced by 2, and they will not be able to recover Fatigue, until they have had comfortable bed rest for ('40 - Endurance - tending Healer Rank') hours. Add '+10' to subsequent rolls on the fright table." },
                { 200, "[Heart Attack]- The target suffers a heart attack and must receive the attention of a Healer of at least 'Rank 3' within 'Endurance pulses or they are dead'. If they survive they will be on '0 Endurance and 0 Fatigue', and will be unconsciousness for ('30 - Endurance') minutes. All their Characteristics and Ranks will be reduced by 5, and they will not be able to recover Fatigue or more than half their Endurance, until they have had comfortable bed rest for (60 - Endurance - tending Healer Rank) hours. Add '+10 to subsequent rolls on the fright table'." }
            };
        }
    }
}
