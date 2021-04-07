# DiceRoller.Net
A .Net dice roller based on the [Irony](https://github.com/IronyProject/Irony) language kit.

## Features

| Example                    | Notes                  |
|----------------------------|------------------------|
| (67 + 54 - 20)/15          | Arithmetic             |
| d4                         | Dice rolling           |
| 3d6+2                      |                        |
| 3d6!                       | Exploding dice         |
| (3d10 + 5) * d7 / 2 - 1d2! | Combo                  |
| 1d6+8 # Orc's Damage       | Comments               |

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

> 3d10 < 55
2 successes Reason: [45, 87, 22]

> 3d10 > 55
1 successes Reason: [45, 87, 22]

> min(d10-4,1) + 5
6 Reason: [2]-4 = -2, min 1 => 1 + 5

</pre>
