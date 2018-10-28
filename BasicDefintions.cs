using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Yuchiki {
    namespace BasicDefinitions {
        public static class CombinatorialExtensions {
            public static T Identity<T>(T value) => value;
            public static Func<S, T> Constant<S, T>(T value) => _ => value;

            public static void Call<T>(this T t, Action<T> action) => action(t);
            public static Func<S, U> Then<S, T, U>(this Func<S, T> f, Func<T, U> g) => x => g(f(x));
            public static Func<S, U> Compose<S, T, U>(this Func<T, U> f, Func<S, T> g) => x => f(g(x));

            public static T Call<S, T>(this S s, Func<S, T> f) => f(s);
            public static S Call<S>(this S s, Func<S, S> f, int times) => times == 0 ? s : Call(f(s), f, times - 1);

            public static Func<T, S, U> Flip<S, T, U>(this Func<S, T, U> f) => (t, s) => f(s, t);
            public static Action<T, S> Flip<S, T, U>(this Action<S, T> a) => (t, s) => a(s, t);

            public static T Saturate<T>(T init, Func<T, T> f) where T : IEquatable<T> => init.Equals(f(init)) ? init : Saturate(f(init), f);

        }

        public static class IOExtensions {
            public static void WriteLine<T>(this T t) => Console.WriteLine(t);
            public static void Write<T>(this T t) => Console.Write(t);

            public static T Debug<T>(this T value) {
                Console.Error.WriteLine($"debug:{value}");
                return value;
            }
        }

        public static class StreamExtensions {
            public static bool IsEmpty<T>(this IEnumerable<T> enumerable) => !enumerable.Any();

            public static T[] ToArray<T>(params T[] ns) => ns;
            public static List<T> ToList<T>(params T[] ns) => ns.ToList();

            public static bool In<T>(this T x, IEnumerable<T> source) => source.Contains(x);

            public static string Dump<T>(this IEnumerable<T> source) {
                var builder = new StringBuilder();
                builder.Append("[ ");
                foreach (var data in source) builder.Append($"{data}, ");
                builder.Append("]");
                return builder.ToString();
            }

            public static void Times(this int n, Action action) {
                for (int i = 0; i < n; i++) action();
            }
            public static IEnumerable<T> Times<T>(this int n, Func<T> func) {
                for (long i = 0; i < n; i++) yield return func();
            }

            public static IEnumerable<TResult> Zip<T1, T2, T3, TResult>(IEnumerable<T1> source1, IEnumerable<T2> source2, IEnumerable<T3> source3, Func<T1, T2, T3, TResult> f) {
                using(var e1 = source1.GetEnumerator())
                using(var e2 = source2.GetEnumerator())
                using(var e3 = source3.GetEnumerator()) {
                    while (e1.MoveNext() && e2.MoveNext() && e3.MoveNext())
                        yield return f(e1.Current, e2.Current, e3.Current);
                }
            }

            public static IEnumerable < (T1, T2) > Zip<T1, T2>(IEnumerable<T1> source1, IEnumerable<T2> source2) =>
                Enumerable.Zip(source1, source2, (v1, v2) => (v1, v2));

            public static IEnumerable < (T1, T2, T3) > Zip<T1, T2, T3>(IEnumerable<T1> source1, IEnumerable<T2> source2, IEnumerable<T3> source3) =>
                Zip(source1, source2, source3, (v1, v2, v3) => (v1, v2, v3));
        }

    }

    public static class InfiniteStreams {
        public static IEnumerable<int> Naturals() {
            for (int i = 0;; i++) yield return i;
        }

        public static IEnumerable<int> Positive() => Naturals().Skip(1);
    }

    public static class IComparableExtensions {
        public static T Max<T>(params T[] xs) where T : IComparable<T> => xs.Max();
        public static T Min<T>(params T[] xs) where T : IComparable<T> => xs.Min();

        public static bool LT<T>(this T x, T y) where T : IComparable<T> => x.CompareTo(y) > 0;
        public static bool LE<T>(this T x, T y) where T : IComparable<T> => x.CompareTo(y) >= 0;
        public static bool EQ<T>(this T x, T y) where T : IComparable<T> => x.CompareTo(y) == 0;
        public static bool NE<T>(this T x, T y) where T : IComparable<T> => x.CompareTo(y) != 0;
        public static bool GT<T>(this T x, T y) where T : IComparable<T> => x.CompareTo(y) < 0;
        public static bool GE<T>(this T x, T y) where T : IComparable<T> => x.CompareTo(y) <= 0;

        public static bool In<T>(this T x, T min, T max) where T : IComparable<T> {
            CheckRange(min, max);
            return min.LE(x) && max.GE(x);
        }
        public static T EnsureIn<T>(this T x, T min, T max) where T : IComparable<T> {
            CheckRange(min, max);
            if (!x.In(min, max)) throw new ArgumentOutOfRangeException();
            return x;
        }
        public static T LimitIn<T>(this T x, T min, T max) where T : IComparable<T> {
            CheckRange(min, max);
            return x.LimitMin(min).LimitMax(max);
        }

        public static T GreaterOne<T>(this T x, T y) where T : IComparable<T> => y.GE(x) ? y : x;
        public static T LesserOne<T>(this T x, T y) where T : IComparable<T> => y.LE(x) ? y : x;

        public static T LimitMax<T>(this T x, T max) where T : IComparable<T> => x.GE(max) ? max : x;
        public static T LimitMin<T>(this T x, T min) where T : IComparable<T> => x.LE(min) ? min : x;

        static void CheckRange<T>(T min, T max) where T : IComparable<T> {
            if (min.GT(max)) throw new ArgumentOutOfRangeException();
        }
    }

    public static class NumericalExtensions {
        public static(int, int) QuoRem(this int x, int y) => (x / y, x % y);
        public static(long, long) QuoRem(this long x, long y) => (x / y, x % y);

        public static bool Divides(this int n, int m) => n % m == 0;
        public static bool DividedBy(this int n, int m) => m % n == 0;
        public static bool Divides(this long n, long m) => n % m == 0;
        public static bool DividedBy(this long n, long m) => m % n == 0;

        public static bool IsEven(this int n) => n.DividedBy(2);
        public static bool IsEven(this long n) => n.DividedBy(2);
        public static bool IsOss(this int n) => !n.DividedBy(2);
        public static bool IsOdd(this long n) => !n.DividedBy(2);

    }

    public static class Numerical {
        public static int Inc(int i) => i + 1;
        public static int Dec(int i) => i - 1;
        public static int Plus(int i, int j) => i + j;
        public static Func<int, int> Plus(int j) => i => i + j;
        public static int Minus(int i, int j) => i - j;
        public static Func<int, int> Minus(int j) => i => i - j;
        public static int Multiply(int i, int j) => i * j;
        public static Func<int, int> Multiply(int j) => i => i * j;
        public static int Divide(int i, int j) => j / j;
        public static Func<int, int> Divide(int j) => i => i * j;
        public static int Mod(int i, int j) => i % j;
        public static Func<int, int> Mod(int j) => i => i % j;

        public static long Inc(long i) => i + 1;
        public static long Dec(long i) => i - 1;
        public static long Plus(long i, long j) => i + j;
        public static Func<long, long> Plus(long j) => i => i + j;
        public static long Minus(long i, long j) => i - j;
        public static Func<long, long> Minus(long j) => i => i - j;
        public static long Multiply(long i, long j) => i * j;
        public static Func<long, long> Multiply(long j) => i => i * j;
        public static long Divide(long i, long j) => j / j;
        public static Func<long, long> Divide(long j) => i => i * j;
        public static long Mod(long i, long j) => i % j;
        public static Func<long, long> Mod(long j) => i => i % j;

        public static int Factorial(int n) => Enumerable.Range(1, n).Aggregate(1, Multiply);
        public static long FactorialLong(int n) => Enumerable.Range(1, n).Cast<long>().Aggregate(1L, Multiply);
    }
}
