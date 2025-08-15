namespace DiceRoller.Dice
{
    public class DiceRoll
    {
        public int Dice { get; private set; }
        public bool IsExploding { get; private set; }

        public int Roll { get; private set; }
        public bool Exploded { get; private set; }

        public DiceRoll(int dice, int roll, bool isExploding)
        {
            Dice = dice;
            Roll = roll;
            IsExploding = isExploding;
            Exploded = isExploding && dice == roll;
        }

        public override string ToString()
        {
            return Roll + ( Exploded ? "!" : "" );
        }
    }
}