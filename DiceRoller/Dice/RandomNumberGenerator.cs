using System;
using System.Security.Cryptography;

namespace DiceRoller.Dice
{
    public class RandomNumberGenerator : IRandomNumberGenerator
    {
        private static readonly RNGCryptoServiceProvider provider = new RNGCryptoServiceProvider();

        public int Next(int maxValue)
        {
            var buffer = new byte[sizeof(int)];
            provider.GetBytes(buffer);

            var randomInt = BitConverter.ToInt32(buffer);
            var result = Math.Abs(randomInt % maxValue);
            return result;
        }
    }
}
