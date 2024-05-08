using BasicObjects.MathExtensions;

namespace Collections.WireFrameMesh.Basics
{
    public class PositionEdge
    {
        public PositionEdge(PositionNormal a, PositionNormal b)
        {
            A = a;
            B = b;
            Key = new Combination2(a.PositionObject.Id, b.PositionObject.Id);
        }

        public PositionNormal A { get; private set; }
        public PositionNormal B { get; private set; }

        public IEnumerable<PositionNormal> Positions
        {
            get
            {
                yield return A;
                yield return B;
            }
        }

        public Combination2 Key { get; }

        public bool ContainsPosition(Position position)
        {
            return A.PositionObject?.Id == position.Id || B.PositionObject?.Id == position.Id;
        }
    }
}
