using System;
using System.Collections.Generic;

namespace DiceRoller.Core.Parser
{
    public static class LinqExtensions
    {
        public static IEnumerable<T> TakeUntilInclusive<T>(this IEnumerable<T> list, Func<T, int, bool> predicate)
        {
            var i = 0;
            foreach (T el in list)
            {
                yield return el;
                if (predicate(el, i))
                    yield break;
                i++;
            }
        }
    }
}
