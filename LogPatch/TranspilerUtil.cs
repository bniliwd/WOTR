using HarmonyLib;
using Kingmaker.Utility;
using System;
using System.Collections.Generic;
using System.Linq;

namespace LogPatch
{
    internal static class TranspilerUtil
    {
        public static IEnumerable<(int index, T item)> Indexed<T>(this IEnumerable<T> source)
        {
            var index = 0;
            foreach (var item in source)
                yield return (index++, item);
        }

        public static IEnumerable<T> FindSequence<T>(this IEnumerable<T> source, IEnumerable<Func<T, bool>> predicateSequence) =>
            source.FindSequence(predicateSequence.Count(), predicateSequence);

        public static IEnumerable<(int index, CodeInstruction instruction)> FindInstructionsIndexed(
            this IEnumerable<CodeInstruction> instructions, IEnumerable<Func<CodeInstruction, bool>> matchFuncs)
        {
            var matched = instructions
                .Indexed()
                .FindSequence(matchFuncs
                    .Select<Func<CodeInstruction, bool>, Func<(int index, CodeInstruction item), bool>>(f =>
                        i => f(i.item)));

            if (!matched.Any()) return [];

            return matched;
        }

        public static IEnumerable<T> FindSequence<T>(this IEnumerable<T> source, int length, IEnumerable<Func<T, bool>> predicateSequence)
        {
            var i = 0;
            foreach (var result in predicateSequence.Zip(source, (f, x) => f(x)))
            {
                if (!result) return source.Skip(1).FindSequence(length, predicateSequence);

                i++;

                if (i >= length) return source.Take(i);
            }

            return [];
        }

    }
}
