using Operations.Intermesh.Basics;
using Operations.Intermesh.Classes.Support.ExtractFillTriangles.Interfaces;

namespace Operations.Intermesh.Classes.Support.ExtractFillTriangles
{
    internal class NearDegenerateFillStrategy : IFillStrategy
    {
        public void GetFillTriangles(IntermeshTriangle triangle)
        {
            if (!ShouldUseStrategy(triangle)) { throw new InvalidOperationException($"Incorrect triangle fill strategy used."); }
            var strategy = new AbstractNearDegenerateFill<IntermeshPoint>(triangle.NonSpurDivisions.Select(d => (d.A, d.B)), p => p.Id, p => triangle.Verticies.Any(v => v.Id == p.Id));

            foreach (var filling in strategy.GetFill())
            {
                var fillTriangle = new FillTriangle(triangle, filling.Item1, filling.Item2, filling.Item3);
                triangle.Fillings.Add(fillTriangle);
            }
        }

        public bool ShouldUseStrategy(IntermeshTriangle triangle)
        {
            return triangle.IsNearDegenerate;
        }
    }
}
