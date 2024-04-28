using BasicObjects.GeometricObjects;
using Operations.Regions;

namespace Operations.PlanarFilling.Filling.Interfaces
{
    internal interface IFillingRegions
    {
        void Load(IEnumerable<InternalPlanarSegment> segments);
        bool CrossesInterior(InternalPlanarSegment testSegment);
        bool IsAtBoundary(InternalPlanarSegment testSegment);
        Region RegionOfProjectedPoint(Point3D point);
        bool HasIntersection(InternalPlanarSegment testSegment);
        InternalPlanarSegment GetNearestIntersectingSegment(InternalPlanarSegment testSegment);
    }
}
