using BasicObjects.GeometricObjects;
using Operations.Regions;

namespace Operations.Filling.Interfaces
{
    internal interface IFillingRegions
    {
        void Load(IEnumerable<FillingSegment> segments);
        bool CrossesInterior(FillingSegment testSegment);
        bool IsAtBoundary(FillingSegment testSegment);
        Region RegionOfAppliedPoint(Point3D point);
        bool HasIntersection(FillingSegment testSegment);
        FillingSegment GetNearestIntersectingSegment(FillingSegment testSegment);
    }
}
