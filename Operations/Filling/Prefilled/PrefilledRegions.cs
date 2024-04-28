using BasicObjects.GeometricObjects;
using Collections.Buckets;
using Collections.WireFrameMesh.Basics;
using Operations.Filling.Interfaces;
using Operations.Regions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Operations.Filling.Prefilled
{
    internal class PrefilledRegions : IFillingRegions
    {
        private IEnumerable<FillingSegment> _segments;
        private PositionTriangle[] _region;

        public PrefilledRegions(PositionTriangle[] region)
        {
            _region = region;
        }

        private BoxBucket<FillingSegment> _segmentBucket = null;

        private BoxBucket<FillingSegment> SegmentBucket
        {
            get
            {
                if (_segmentBucket is null)
                {
                    _segmentBucket = new BoxBucket<FillingSegment>(_segments.ToArray());
                }
                return _segmentBucket;
            }
        }

        private BoxBucket<PositionTriangle> _regionBucket = null;
        private BoxBucket<PositionTriangle> RegionBucket
        {
            get
            {
                if (_regionBucket is null)
                {
                    _regionBucket = new BoxBucket<PositionTriangle>(_region);
                }
                return _regionBucket;
            }
        }

        public bool CrossesInterior(FillingSegment testSegment)
        {
            throw new NotImplementedException();
        }

        public FillingSegment GetNearestIntersectingSegment(FillingSegment testSegment)
        {
            throw new NotImplementedException();
        }

        public bool HasIntersection(FillingSegment testSegment)
        {
            throw new NotImplementedException();
        }

        public bool IsAtBoundary(FillingSegment testSegment)
        {
            throw new NotImplementedException();
        }

        public void Load(IEnumerable<FillingSegment> segments)
        {
            _segments = segments;
        }

        public bool IsInInterior(Point3D point)
        {
            throw new NotImplementedException();
        }
    }
}
