
namespace Operations.SurfaceSegmentChaining.Basics
{
    internal class ProtectedLinkedIndexSegments<G, U> where G : class
    {
        public ProtectedLinkedIndexSegments() { }
        public ProtectedLinkedIndexSegments(IReadOnlyCollection<LinkedIndexSegment<G>> linkedIndexSegments) { LinkedIndexSegments = linkedIndexSegments; }
        protected IReadOnlyCollection<LinkedIndexSegment<G>> LinkedIndexSegments { get; private set; }

        public static T Create<T>(ProtectedLinkedIndexSegments<G, U> input) where T : ProtectedLinkedIndexSegments<G, U>, new()
        {
            T output = new T();
            output.LinkedIndexSegments = input.LinkedIndexSegments;
            return output;
        }
    }
}
