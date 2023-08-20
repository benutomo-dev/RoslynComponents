using System.Collections.Immutable;
using System.Linq;
using System.Collections.Generic;
using System;
using SourceGeneratorDebug_StandardPatterns;

namespace SourceGeneratorDebug_StandardPatterns.ImmutableCollectionSupport
{
    internal class Class1
    {
        ImmutableArray<int> x = ImmutableArray<int>.Empty;

        public static implicit operator ImmutableArray<int>(Class1 arg) => ImmutableArray<int>.Empty;

        public void Method()
        {
            var zz = ImmutableArray.Create(ImmutableArray.Create<int>(1, 2, 3));

            zz.SelectMany(v => v.BoxlessAsReadOnlyList()).ToArray();

            //zz.Cast<int>().ToList();
            //zz.Select((v, i) => v);

            //var x = ImmutableArray.Create(1, 2, 3);

            //x.IntSum();
        }

        public void Method2() => x.Add(1);

        public ImmutableArray<int> Method3() => x.Add(1);

        /// <inheritdoc cref="Method5(object[,], ref int)"/>
        public IImmutableList<int> Method4() => x.Add(1);

        /// <summary>
        /// ddd
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public ImmutableArray<int> Method5(ImmutableArray<int> obj, ref int x)
        {
            return obj.Add(1);
        }

        public void X(IEnumerable<int> x)
        {

        }

        public void X2(IImmutableList<int> x)
        {

        }
    }
}


namespace SourceGeneratorDebug_StandardPatterns.ImmutableCollectionSupport
{
    class XXX { }

    enum FFlag
    {
        Orange,
        XValue,
    }

    static class ImmutableArrayExtensionsXxx
    {
    //    internal static IEnumerable<U> Select<T, U>(this ImmutableArray<T> @this, Func<T, int, U> selector)
    //    {
    //        return Enumerable.Empty<U>();
    //    }

        /// <summary>
        /// asdgagsdg
        /// </summary>
        /// <param name="this">THIS</param>
        /// <param name="x">param x.</param>
        /// <param name="cancellationToken">param cancellationToken</param>
        /// <returns>retun value.</returns>
        public static IEnumerable<int> IntSum(this IEnumerable<int> @this)
        {
            return @this;
        }
    }
}

static partial class ImmutableArrayExtensions__
{
    public static void Xxxx(this SourceGeneratorDebug_StandardPatterns.ImmutableCollectionSupport.Class1 class1) { }
}