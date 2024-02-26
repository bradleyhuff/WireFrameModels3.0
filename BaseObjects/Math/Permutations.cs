using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BasicObjects.Math
{
    public struct Permutation2
    {
        public Permutation2(int a, int b)
        {
            A = a; B = b;
        }

        public int A { get; }
        public int B { get; }

        public static bool operator ==(Permutation2 a, Permutation2 b)
        {
            return a.A == b.A && b.A == b.B;
        }
        public static bool operator !=(Permutation2 a, Permutation2 b)
        {
            return !(a.A == b.A && b.A == b.B);
        }

        public override bool Equals(object? obj)
        {
            throw new InvalidOperationException("Do not use this method in a dictionary.  Use Combination2Dictionary.");
        }

        public override int GetHashCode()
        {
            throw new InvalidOperationException("Do not use this method in a dictionary.  Use Combination2Dictionary.");
        }

        public override string ToString()
        {
            return $"[{A}, {B}]";
        }
    }

    internal class Permutation2Comparer : IEqualityComparer<Permutation2>
    {
        public bool Equals(Permutation2 x, Permutation2 y)
        {
            return x.A == y.A && x.B == y.B;
        }

        public int GetHashCode(Permutation2 obj)
        {
            return obj.A ^ obj.B;
        }
    }
    public class Permutation2Dictionary<T> : Dictionary<Permutation2, T>
    {
        public Permutation2Dictionary() : base(new Permutation2Comparer()) { }

        public T this[int i, int j]
        {
            get
            {
                return base[new Permutation2(i, j)];
            }
            set
            {
                base[new Permutation2(i, j)] = value;
            }
        }
        public bool ContainsKey(int i, int j)
        {
            return ContainsKey(new Permutation2(i, j));
        }
    }

    public struct Permutation3
    {
        public Permutation3(int a, int b, int c)
        {
            A = a; B = b; C = c;
        }

        public int A { get; }
        public int B { get; }
        public int C { get; }

        public static bool operator ==(Permutation3 a, Permutation3 b)
        {
            return a.A == b.A && a.B == b.B && a.C == b.C;
        }
        public static bool operator !=(Permutation3 a, Permutation3 b)
        {
            return !(a.A == b.A && a.B == b.B && a.C == b.C);
        }

        public override bool Equals(object? obj)
        {
            throw new InvalidOperationException("Do not use this method in a dictionary.  Use Combination3Dictionary.");
        }

        public override int GetHashCode()
        {
            throw new InvalidOperationException("Do not use this method in a dictionary.  Use Combination3Dictionary.");
        }
        public override string ToString()
        {
            return $"[{A}, {A}, {A}]";
        }
    }

    internal class Permutation3Comparer : IEqualityComparer<Permutation3>
    {
        public bool Equals(Permutation3 x, Permutation3 y)
        {
            return x.A == y.A && x.B == y.B && x.C == y.C;
        }

        public int GetHashCode(Permutation3 obj)
        {
            return obj.A ^ obj.B ^ obj.C;
        }
    }

    public class Permutation3Dictionary<T> : Dictionary<Permutation3, T>
    {
        public Permutation3Dictionary() : base(new Permutation3Comparer()) { }

        public T this[int i, int j, int k]
        {
            get
            {
                return base[new Permutation3(i, j, k)];
            }
            set
            {
                base[new Permutation3(i, j, k)] = value;
            }
        }
        public bool ContainsKey(int i, int j, int k)
        {
            return ContainsKey(new Permutation3(i, j, k));
        }
    }

    public class Permutation
    {
        public Permutation(int[] array)
        {
            Array = array;
        }

        public int[] Array { get; }

        public static bool operator ==(Permutation x, Permutation y)
        {
            if (x.Array.Length != y.Array.Length) return false;
            for (int i = 0; i < x.Array.Length; i++)
            {
                if (x.Array[i] != y.Array[i]) return false;
            }
            return true;
        }
        public static bool operator !=(Permutation x, Permutation y)
        {
            if (x.Array.Length != y.Array.Length) return true;
            for (int i = 0; i < x.Array.Length; i++)
            {
                if (x.Array[i] != y.Array[i]) return true;
            }
            return false;
        }

        public override bool Equals(object? obj)
        {
            throw new InvalidOperationException("Do not use this method in a dictionary.  Use PermutationDictionary.");
        }

        public override int GetHashCode()
        {
            throw new InvalidOperationException("Do not use this method in a dictionary.  Use PermutationDictionary.");
        }

        public override string ToString()
        {
            return $"[{string.Join(",", Array)}]";
        }
    }

    internal class PermutationComparer : IEqualityComparer<Permutation>
    {
        public bool Equals(Permutation? x, Permutation? y)
        {
            if (x.Array.Length != y.Array.Length) return false;
            for (int i = 0; i < x.Array.Length; i++)
            {
                if (x.Array[i] != y.Array[i]) return false;
            }
            return true;
        }

        public int GetHashCode(Permutation obj)
        {
            return obj.Array[0] ^ obj.Array[obj.Array.Length - 1];
        }
    }

    public class PermutationDictionary<T> : Dictionary<Permutation, T>
    {
        public PermutationDictionary() : base(new PermutationComparer()) { }

        public T this[int[] input]
        {
            get
            {
                return base[new Permutation(input)];
            }
            set
            {
                base[new Permutation(input)] = value;
            }
        }
        public bool ContainsKey(int[] input)
        {
            return ContainsKey(new Permutation(input));
        }
    }
}
