
namespace BasicObjects.MathExtensions
{
    public struct Combination2
    {
        public Combination2(int a, int b)
        {
            int caseNumber = (a <= b) ? 1 : 0;

            switch (caseNumber)
            {
                case 0: A = b; B = a; break;
                case 1: A = a; B = b; break;
            }
        }

        public int A { get; } = 0;
        public int B { get; } = 0;

        public static bool operator ==(Combination2 a, Combination2 b)
        {
            return a.A == b.A && a.B == b.B;
        }
        public static bool operator !=(Combination2 a, Combination2 b)
        {
            return !(a.A == b.A && a.B == b.B);
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

    public class Combination2Comparer : IEqualityComparer<Combination2>, IComparer<Combination2>
    {
        public int Compare(Combination2 a, Combination2 b)
        {
            if (a.A < b.A)
            {
                return -1;
            }
            if (a.A > b.A)
            {
                return 1;
            }
            if (a.B < b.B)
            {
                return -1;
            }
            if (a.B > b.B)
            {
                return 1;
            }
            return 0;
        }
        public bool Equals(Combination2 x, Combination2 y)
        {
            return x.A == y.A && x.B == y.B;
        }

        public int GetHashCode(Combination2 obj)
        {
            return (obj.A << 16) ^ obj.B;
        }
    }
    public class Combination2Dictionary<T> : Dictionary<Combination2, T>
    {
        public Combination2Dictionary() : base(new Combination2Comparer()) { }

        public T this[int i, int j]
        {
            get
            {
                return base[new Combination2(i, j)];
            }
            set
            {
                base[new Combination2(i, j)] = value;
            }
        }
        public bool ContainsKey(int i, int j)
        {
            return ContainsKey(new Combination2(i, j));
        }
    }

    public struct Combination3
    {
        public Combination3(int a, int b, int c)
        {
            int digit3 = (a < b) ? 1 : 0;
            int digit2 = (b < c) ? 1 : 0;
            int digit1 = (a < c) ? 1 : 0;

            int caseNumber = digit3 << 2 | digit2 << 1 | digit1;

            switch (caseNumber)
            {
                case 0: A = c; B = b; C = a; break;
                case 2: A = b; B = c; C = a; break;
                case 3: A = b; B = a; C = c; break;
                case 4: A = c; B = a; C = b; break;
                case 5: A = a; B = c; C = b; break;
                case 7: A = a; B = b; C = c; break;
            }
        }

        public int A { get; } = 0;
        public int B { get; } = 0;
        public int C { get; } = 0;

        public static bool operator ==(Combination3 a, Combination3 b)
        {
            return a.A == b.A && a.B == b.B && a.C == b.C;
        }
        public static bool operator !=(Combination3 a, Combination3 b)
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
            return $"[{A}, {B}, {C}]";
        }
    }
    public class Combination3Comparer : IEqualityComparer<Combination3>, IComparer<Combination3>
    {
        public int Compare(Combination3 a, Combination3 b)
        {
            if (a.A < b.A)
            {
                return -1;
            }
            if (a.A > b.A)
            {
                return 1;
            }
            if (a.B < b.B)
            {
                return -1;
            }
            if (a.B > b.B)
            {
                return 1;
            }
            if (a.C < b.C)
            {
                return -1;
            }
            if (a.C > b.C)
            {
                return 1;
            }
            return 0;
        }
        public bool Equals(Combination3 x, Combination3 y)
        {
            return x.A == y.A && x.B == y.B && x.C == y.C;
        }

        public int GetHashCode(Combination3 obj)
        {
            return (obj.A << 20) ^ (obj.B << 10) ^ obj.C;
        }
    }

    public class Combination3Dictionary<T> : Dictionary<Combination3, T>
    {
        public Combination3Dictionary() : base(new Combination3Comparer()) { }

        public T this[int i, int j, int k]
        {
            get
            {
                return base[new Combination3(i, j, k)];
            }
            set
            {
                base[new Combination3(i, j, k)] = value;
            }
        }
        public bool ContainsKey(int i, int j, int k)
        {
            return ContainsKey(new Combination3(i, j, k));
        }
    }

    public class Combination
    {
        public Combination(int[] array)
        {
            System.Array.Sort(array);
            Array = array;
        }

        public int[] Array { get; }

        public static bool operator ==(Combination x, Combination y)
        {
            if (x.Array.Length != y.Array.Length) return false;
            for (int i = 0; i < x.Array.Length; i++)
            {
                if (x.Array[i] != y.Array[i]) return false;
            }
            return true;
        }
        public static bool operator !=(Combination x, Combination y)
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
            throw new InvalidOperationException("Do not use this method in a dictionary.  Use CombinationDictionary.");
        }

        public override int GetHashCode()
        {
            throw new InvalidOperationException("Do not use this method in a dictionary.  Use CombinationDictionary.");
        }

        public override string ToString()
        {
            return $"[{string.Join(",", Array)}]";
        }
    }

    public class CombinationComparer : IEqualityComparer<Combination>, IComparer<Combination>
    {
        public int Compare(Combination? a, Combination? b)
        {
            if (a.Array.Length < b.Array.Length) return -1;
            if (a.Array.Length > b.Array.Length) return 1;

            for (int i = 0; i < a.Array.Length; i++)
            {
                if (a.Array[i] < b.Array[i]) return -1;
                if (a.Array[i] > b.Array[i]) return 1;
            }
            return 0;
        }
        public bool Equals(Combination? x, Combination? y)
        {
            if (x.Array.Length != y.Array.Length) return false;
            for (int i = 0; i < x.Array.Length; i++)
            {
                if (x.Array[i] != y.Array[i]) return false;
            }
            return true;
        }

        public int GetHashCode(Combination obj)
        {
            return obj.Array[0] ^ obj.Array[obj.Array.Length - 1];
        }
    }

    public class CombinationDictionary<T> : Dictionary<Combination, T>
    {
        public CombinationDictionary() : base(new CombinationComparer()) { }

        public T this[int[] input]
        {
            get
            {
                return base[new Combination(input)];
            }
            set
            {
                base[new Combination(input)] = value;
            }
        }
        public bool ContainsKey(int[] input)
        {
            return ContainsKey(new Combination(input));
        }
    }
}
