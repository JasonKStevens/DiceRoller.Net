using DiceRoller.Dice;
using DiceRoller.Core.Formatting;
using DiceRoller.Core.Formatting.Core;
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

        public static IRandomNumberGenerator GetMoqGeneratorForStaticResult(int alwaysReturn)
        {
            var randomNumberGeneratorMock = new Mock<IRandomNumberGenerator>();
            var setup = randomNumberGeneratorMock
                .Setup(x => x.Next(It.IsAny<int>()))
                .Returns(alwaysReturn);

            return randomNumberGeneratorMock.Object;
        }

        public static AdaptiveFormatterManager CreateTestAdaptiveFormatterManager()
        {
            var complexityAnalyzer = new ComplexityAnalyzer();
            var enhancedFormatter = new EnhancedTextFormatter();
            return new AdaptiveFormatterManager(complexityAnalyzer, enhancedFormatter);
        }

        public static DiscordFormatter CreateTestDiscordFormatter(FormattingContext context, ResultFormat format)
        {
            var adaptiveFormatterManager = CreateTestAdaptiveFormatterManager();
            return new DiscordFormatter(context, format, adaptiveFormatterManager);
        }
    }
}
