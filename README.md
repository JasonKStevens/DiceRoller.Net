# DiceRoller.Net
A .Net dice roller based on the [Irony](https://github.com/IronyProject/Irony) language kit.

## Features

| Feature                | Example                    |
|------------------------|----------------------------|
| Arithmetic             | (67 + 54 - 20)/15          |
| Dice rolling           | d4                         |
|                        | 3d6+2                      |
| Exploding dice         | 3d6!                       |
| Combo                  | (3d10 + 5) * d7 / 2 - 1d2! |
| Comments               | 1d6+8 # Orc's Damage       |
| Minimum roll outcome   | min(d10-4,1)               |
| Repeat rolls           | repeat(d10, 5)             |

| Custom Command         | Description                    |
|------------------------|--------------------------------|
| backfire               | Generates a DQ Backfire result |
| injury                 | Generates a DQ Grievous Injury |


## REPL
<pre>
> d4
1 Reason: [1]

> 3d6
9 Reason: [1, 4, 4]

> 3/4
0.75 Reason: 3 / 4

> (3d10 + 5) / 2 + 1d2!
14.5 Reason: [9, 4, 5] + 5 / 2 + [2!, 1]

> 3d100 < 55  OR should this be: repeat(d100 < 55, 3) ???
2 successes Reason: [45, 87, 22]

> 3d10 > 55 OR alternate - need to decide which one
1 successes Reason: [45, 87, 22]

> min(d10-4,1) + 5
6 Reason: [2]-4 = -2, min 1 => 1 + 5

> repeat(d10,6)
24 Reason: [1] [2] [6] [1] [9] [5]

> backfire
32 Reason: The Adept becomes the target of the spell with some or all effects doubled.

> injury
99 Reason: Crushing blow to your pelvis breaks bone and tears tissue. Take 7 Damage Points immediately to Endurance and fall prone. Make a WP check to avoid falling unconscious. If you survive, you will be unable to move for D10 months.
</pre>

## TODO
Add --help support
