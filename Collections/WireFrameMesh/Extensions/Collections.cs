using Collections.WireFrameMesh.Interfaces;

namespace Collections.WireFrameMesh.Extensions
{
    public static class Collections
    {
        public static IWireFrameMesh Combine(this IEnumerable<IWireFrameMesh> meshes)
        {
            if (!meshes.Any()) { return null; }
            var output = meshes.First().CreateNewInstance();

            output.AddRangeTriangles(meshes.SelectMany(m => m.Triangles), "", 0);
            return output;
        }
    }
}
