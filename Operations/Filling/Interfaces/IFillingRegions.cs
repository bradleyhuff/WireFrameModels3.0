using BasicObjects.GeometricObjects;
using Operations.Regions;

namespace Operations.Filling.Interfaces
{
    internal interface IFillingRegions
    {
        void Load(IEnumerable<FillingSegment> segments);
        bool CrossesInterior(FillingSegment testSegment);
        bool IsAtBoundary(FillingSegment testSegment);
        bool HasIntersection(FillingSegment testSegment);
        FillingSegment GetNearestIntersectingSegment(FillingSegment testSegment);
        bool IsInInterior(Point3D point);
    }
}
