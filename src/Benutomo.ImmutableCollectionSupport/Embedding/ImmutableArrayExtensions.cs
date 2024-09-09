//using System.Collections.Immutable;
//using System.Diagnostics;
//using System.Runtime.InteropServices;

//namespace Benutomo.ImmutableCollectionSupport.Embedding
//{
//    /// <summary>
//    /// Equalsメソッドの自動実装でこの属性を付与したメンバの等価性判定に使用する<see cref="IEqualityComparer{T}"。/>
//    /// </summary>
//    [StaticSource("Benutomo.Linq.ImmutableArraySupport",
//        Usings = new[] {
//            "using System;",
//            "using System.Linq;",
//            "using System.Collections.Generic;",
//        },
//        Directives = new[] {
//            "#pragma warning disable CS0436",
//            "#nullable enable",
//        },
//        Attributes = new[] {
//            @"[System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]",
//        })]
//    internal static partial class ImmutableArrayExtensions
//    {
//        /// <summary>
//        /// <see cref="ImmutableArray{T}"/>をボックス化を回避して<see cref="IReadOnlyList{T}"/>に変換する。
//        /// </summary>
//        /// <typeparam name="T"></typeparam>
//        /// <param name="immutableArray"></param>
//        /// <returns></returns>
//        public static IReadOnlyList<T> BoxlessAsReadOnlyList<T>(this ImmutableArray<T> immutableArray)
//        {
//            if (immutableArray.IsDefaultOrEmpty) return Array.Empty<T>();

//            if (MemoryMarshal.TryGetArray(immutableArray.AsMemory(), out var innerArray) && innerArray.Offset == 0 && innerArray.Array?.Length == immutableArray.Length)
//            {
//                return innerArray.Array;
//            }
//            else
//            {
//                Debug.Fail("ImmutableArrayの内部配列の取得に失敗");
//#pragma warning disable ImmutableCollection0002
//                return (IReadOnlyList<T>)immutableArray;
//#pragma warning restore ImmutableCollection0002
//            }
//        }
//    }
//}

