﻿using BasicObjects.GeometricObjects;
using Collections.Buckets.Interfaces;

namespace Operations.Intermesh.Elastics
{
    internal class ElasticVertexContainer
    {
        private static int _id = 0;
        public ElasticVertexContainer(ElasticSegment segment, Point3D point, char tag)
        {
            Id = _id++;
            _point = point;
            _tag = tag;
            _segment = segment;
        }

        private ElasticSegment _segment;
        private char _tag;

        public ElasticVertexCore Vertex { get; set; }
        public ElasticSegment Segment { get { return _segment; } }

        public int Id { get; }
        private Point3D _point;
        public Point3D Point
        {
            get
            {
                if (Vertex is not null) { return Vertex.Point; }
                return _point;
            }
        }

        public void Link(ElasticVertexContainer b, IBoxBucket<ElasticVertexAnchor> anchors)
        {
            if (Vertex is null && b.Vertex is null)
            {
                var vertex = ElasticVertexCore.Create(Point, anchors);
                vertex.Link(this);
                vertex.Link(b);
                return;
            }
            if (Vertex is null)
            {
                b.Vertex.Link(this);
                return;
            }
            if (b.Vertex is null)
            {
                Vertex.Link(b);
                return;
            }
            if (Vertex.Id == b.Vertex.Id) { return; }

            var containers = b.Vertex.SegmentContainers.ToList();
            foreach (var container in containers)
            {
                b.Vertex?.Delink(container);
            }
            foreach (var container in containers)
            {
                Vertex.Link(container);
            }
        }

        public void VertexFill(IBoxBucket<ElasticVertexAnchor> anchors)
        {
            if (Vertex is not null) { return; }
            var vertex = ElasticVertexCore.Create(Point, anchors);
            vertex.Link(this);
        }
    }
}
