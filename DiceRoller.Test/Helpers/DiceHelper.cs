﻿using DiceRoller.Dice;
using Moq;

namespace DiceRoller.Test.Helpers
{
    public static class DiceHelper
    {
        public static IRandomNumberGenerator GetSequenceGenerator(params int[] sequence)
        {
            var randomNumberGeneratorMock = new Mock<IRandomNumberGenerator>();
            var setup = randomNumberGeneratorMock
                .SetupSequence(foo => foo.Next(It.IsAny<int>()));

            foreach (var value in sequence)
            {
                setup = setup.Returns(value - 1);
            }

            return randomNumberGeneratorMock.Object;
        }
    }
}
