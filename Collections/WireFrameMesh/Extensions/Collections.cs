using Collections.WireFrameMesh.Interfaces;

namespace Collections.WireFrameMesh.Extensions
{
    public static class Collections
    {
        public static IWireFrameMesh Combine(this IEnumerable<IWireFrameMesh> meshes)
        {
            if (meshes is null) { return null; }
            if (!meshes.Any(w => w is not null)) { return null; }
            var output = meshes.First(w => w is not null).CreateNewInstance();

            output.AddRangeTriangles(meshes.Where(m => m is not null).SelectMany(m => m.Triangles), "", 0);
            return output;
        }
    }
}
