﻿using System.Collections.Generic;

namespace DiceRoller.DragonQuest
{
    public class GrievousInjuries : LookupTable
    {
        public GrievousInjuries()
        {
            Map = new Dictionary<int, string>
            {
                { 5, "Congratulations! It’s a bleeder in your primary arm! Take 1 Damage Point from Endurance immediately and 1 per pulse thereafter until the flow is staunched by a Healer of Rank 0 or above or you die" },
                { 7, "Oh no! Your opponent’s weapon has entered your secondary arm’s elbow joint and the tip has broken off. Take 2 Damage Points immediately from Endurance and that arm is useless until the sliver has been removed by a Healer of Rank 3 or above. Also, increase the chance of infection by 30." },
                { 8, "A vicious puncture wound in your groin! Take 3 Damage Points immediately from Endurance and reduce your TMR by 2 until fully recovered, which will take two months. In addition, add 30 to your chance of being infected (assuming you live long enough for such things to matter)." },
                { 10, "You have been stabbed in your secondary arm. Drop whatever you were holding in it and take 2 Damage Points immediately from Endurance. It will take a full week for the arm to be of any use to you whatsoever." },
                { 11, "Your aorta is severed and you are quite dead. Rest assured your companions will do their best to console your widow(er)." },
                { 12, "A stomach puncture. Nasty. You suffer 3 Damage Points immediately from Endurance and lose 2 from your TMR until fully recovered, which will take two months. Also, you are automatically stunned for the next pulse (if you aren’t already), after which you may recover. Add 20 to the chance to be infected." },
                { 13, "Your opponent’s weapon has entered your eye. Roll D10. On a roll of 1, the weapon has entered your brain and you are dead. On a roll of 2–5, your left eye is blinded. On a roll of 6–10, your right eye is blinded. If you are lucky enough to be blinded instead of killed, you have suffered 2 Damage Points to Endurance. In addition, a figure who is blind in one eye suffers the following subtractions: -1 from MD, -2 from PB, -4 from Perception. A figure blinded in one eye reduces their base chance with any missile or thrown weapon by 30." },
                { 18, "Tsk, tsk. A wound of the solid viscera. Usually fatal. Take 3 Damage Points to Endurance immediately and 1 per pulse thereafter until the bleeding is stopped by a Healer of Rank 2 or above or you die. Add 30 to the chance of infection." },
                { 20, "Take a stab in the leg (your choice as to which one) resulting in a deep puncture of the thigh muscle. Suffer 1 Damage Point to Endurance immediately and reduce your TMR by 1 until you heal, which will take 4 weeks." },
                { 25, "A chest wound. Take 2 Damage Points to Endurance immediately and reduce your TMR by 1 until recovered (about 2 months). Look on the bright side, though. Your attacker’s weapon is caught in your rib cage and has been wrenched from their grasp." },
                { 27, "Bad luck! Your secondary hand has been severed at the wrist. Take 2 Damage Points to Endurance immediately and 1 point per pulse from Fatigue thereafter (Endurance when Fatigue is exhausted) until you are dead or the bleeding is staunched by a Healer of Rank 0 or above. If you live, reduce your MD by 2." },
                { 30, "Worst luck! Your primary hand has been severed. See result 26–27 for effects." },
                { 34, "A minor wound. Your face is slashed open, ruining your boyish good looks and causing blood to spurt into your eyes. Reduce your PB by 4 permanently." },
                { 35, "Your secondary arm is sliced off at the shoulder. Take 5 Damage Points immediately from Endurance and 1 per pulse thereafter from Fatigue (Endurance when Fatigue is exhausted) until you are dead or the bleeding is staunched by a Healer of Rank 1 or above. Reduce your MD by 2 and your AG by 1." },
                { 36, "Your primary arm is sliced off at the shoulder. Take 5 Damage Points immediately from Endurance and 1 per pulse thereafter from Fatigue (Endurance when Fatigue is exhausted) until you are dead or the bleeding is staunched by a Healer of Rank 1 or above. Reduce your MD by 2 and your AG by 1." },
                { 40, "You have been eviscerated! Take 4 Damage Points immediately from Endurance and 1 point per pulse from Fatigue thereafter (Endurance when Fatigue is exhausted) until you are unconscious. Increase your chance of infection by 40." },
                { 42, "A glancing blow lays open your scalp and severs one ear (your choice as to which one). Take 2 Damage Points immediately from Endurance. Reduce your Perception by 2." },
                { 43, "A savage slash rips open your cheek and jaw. Take an automatic pass action next pulse due to the shock of the blow. Your PB is increased by 1, since your disfigurement will bring out the maternal/paternal instincts in the opposite gender." },
                { 50, "A slash along one arm, and it’s a bleeder! Take 2 Damage Points immediately from Endurance and lose 1 point from Fatigue (Endurance when Fatigue is exhausted) each pulse until the bleeding is stopped by a Healer of Rank 1 or above or you die." },
                { 52, "Hamstrung! Roll D10. On a roll of 1–4, it is your left leg. On a roll of 5–10 it is your right. Take 4 Damage Points immediately from Endurance and fall prone. You may not stand unassisted until the wound is healed (which should take three months). Reduce your AG by 3 permanently." },
                { 60, "Your primary arm is crippled by a wicked slash! Take 2 damage Points immediately to Endurance and drop anything you have in your primary hand. The arm is unusable until healed, which should take 2 months." },
                { 67, "Your secondary arm is crippled; see 53–60 for details." },
                { 69, "A nasty slash in the region of the shoulder and neck. Roll D10. On a roll of 1–3, your head is severed and your corpse tumbles to the ground. On a roll of 4–6, your secondary collar bone is crushed; on a roll of 7–10 your primary collar bone is crushed. If your collar bone is crushed, the results are identical to 53–60, except you suffer 4 Damage Points to Endurance." },
                { 74, "A crushing blow smashes your helmet and causes a concussion. Take 3 Damage Points from Endurance and suffer a reduction of 4 in both MD and AG lasting for 3 days." },
                { 80, "A massive chest wound accompanied by broken ribs and crushed tissues. Very ugly, this. Take 5 Damage Points immediately from Endurance. Reduce your MD and AG by 3 each until this wound heals (which should take about 4 months). Increase your chance of infection by 10." },
                { 84, "A crushing blow smashes tissue and produces internal injuries. You suffer 2 Damage Points immediately to Endurance and 1 per pulse thereafter to Fatigue (Endurance when Fatigue is exhausted) until unconscious or you receive the attention of a Healer of Rank 2 or above." },
                { 87, "A jarring blow to your primary shoulder inflicts 2 Damage Points immediately to Endurance. Roll D10; the result is the number of pulses the arm is useless. You immediately drop anything held in that hand." },
                { 89, "Similar to 85–87 except it is your secondary shoulder." },
                { 92, "Your right hip is smashed horribly. Take 5 Damage Points immediately to Endurance and fall prone. You will be unable to walk until the damage has healed (which should take about 6 months). Good fun. When healed, you will still have a limp which will reduce your TMR by 1 and your AG by 2." },
                { 94, "The same as 90–92 except it is your left hip that is smashed." },
                { 97, "Your opponent’s weapon has come crashing down on your head and fractured your skull. You fall prone and are unconscious, and take 8 Damage Points to Endurance. If you survive, you lose 2 from AG, 2 from MD and 2 from Perception. It will take a year in bed to recover." },
                { 100, "Crushing blow to your pelvis breaks bone and tears tissue. Take 7 Damage Points immediately to Endurance and fall prone. Make a WP check to avoid falling unconscious. If you survive, you will be unable to move for D10 months." },
            };
        }
    }
}
