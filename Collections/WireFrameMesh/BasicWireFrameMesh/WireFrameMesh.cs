using Collections.WireFrameMesh.Basics;
using Collections.WireFrameMesh.Interfaces;
using BasicObjects.GeometricObjects;
using System.Collections.ObjectModel;
using Collections.Buckets;
using BaseObjects.Transformations.Interfaces;

namespace Collections.WireFrameMesh.BasicWireFrameMesh
{
    public class WireFrameMesh : PositionTriangleMesh, IWireFrameMesh
    {
        internal WireFrameMesh()
        {
            Positions = new ReadOnlyCollection<Position>(_positions);
        }

        private List<Position> _positions = new List<Position>();
        private BoxBucket<Position> _bucket = new BoxBucket<Position>(Enumerable.Empty<Position>());

        public IReadOnlyList<Position> Positions { get; }//re-evaluate normals if transform was used in adding points.

        public PositionNormal AddPoint(Point3D position, Vector3D normal)
        {
            var positionNormal = AddPointNoRow(position, normal);
            AddPoint(positionNormal);
            return positionNormal;
        }

        public PositionNormal AddPoint(Point3D position, Vector3D normal, ITransform transform)
        {
            return AddPoint(transform.Apply(position), transform.Apply(position, normal));
        }

        public PositionNormal AddPointNoRow(Point3D position, Vector3D normal)
        {
            var positionObject = _bucket.Fetch(new Rectangle3D(position, BoxBucket.MARGINS)).SingleOrDefault(p => p.Point == position);
            if (positionObject is null)
            {
                var positionNormal = new PositionNormal(position, normal, this);
                positionObject = new Position(position);
                positionNormal.LinkPosition(positionObject);
                _bucket.Add(positionObject);
                _positions.Add(positionObject);
                return positionNormal;
            }

            var existingPositionNormal = positionObject.PositionNormals.SingleOrDefault(pn => Vector3D.DirectionsEqual(pn.Normal, normal));
            if (existingPositionNormal is not null) { return existingPositionNormal; }
            {
                var positionNormal = new PositionNormal(position, normal, this);
                positionNormal.LinkPosition(positionObject);
                return positionNormal;
            }
        }

        public PositionNormal AddPointNoRow(Point3D position, Vector3D normal, ITransform transform)
        {
            return AddPointNoRow(transform.Apply(position), transform.Apply(position, normal));
        }

        private PositionNormal CloneAddOrExisting(PositionNormal positionNormal)
        {
            var positionObject = _bucket.Fetch(positionNormal).SingleOrDefault(p => p.Point == positionNormal.Position);
            if (positionObject is null)
            {
                var clone = new PositionNormal(positionNormal.Position, positionNormal.Normal, this);
                positionObject = new Position(positionNormal.Position);
                clone.LinkPosition(positionObject);
                _bucket.Add(positionObject);
                _positions.Add(positionObject);
                return clone;
            }

            var existingPositionNormal = positionObject.PositionNormals.SingleOrDefault(pn => Vector3D.DirectionsEqual(pn.Normal, positionNormal.Normal));
            if (existingPositionNormal is not null) { return existingPositionNormal; }
            {
                var clone = new PositionNormal(positionNormal.Position, positionNormal.Normal, this);
                clone.LinkPosition(positionObject);
                return clone;
            }
        }

        public void AddGrid(IWireFrameMesh inputMesh)
        {
            EndGrid();

            Dictionary<int, Element> mapping = new Dictionary<int, Element>();

            foreach (var position in inputMesh.Positions)
            {
                foreach (var positionNormal in position.PositionNormals)
                {
                    foreach (var triangle in positionNormal.Triangles)
                    {
                        if (!mapping.ContainsKey(triangle.Id)) { mapping[triangle.Id] = new Element(); }
                        var element = mapping[triangle.Id];
                        element.PositionNormals.Add(CloneAddOrExisting(positionNormal));
                        element.Trace = triangle.Trace;
                    }
                }
            }
            foreach (var element in mapping.Values.Where(v => v.PositionNormals.Count == 3))
            {
                new PositionTriangle(element.PositionNormals[0], element.PositionNormals[1], element.PositionNormals[2], element.Trace);
            }
            EndGrid();
        }
        public void AddGrids(IEnumerable<IWireFrameMesh> grids)
        {
            foreach (var grid in grids)
            {
                AddGrid(grid);
            }
        }
        private class Element
        {
            public List<PositionNormal> PositionNormals { get; set; } = new List<PositionNormal>(3);
            public string Trace { get; set; }
        }

        public IWireFrameMesh Clone()
        {
            var clone = new WireFrameMesh();

            Dictionary<int, Element> mapping = new Dictionary<int, Element>();

            foreach (var position in Positions)
            {
                foreach (var positionNormal in position.PositionNormals)
                {
                    foreach (var triangle in positionNormal.Triangles)
                    {
                        if (!mapping.ContainsKey(triangle.Id)) { mapping[triangle.Id] = new Element(); }
                        var element = mapping[triangle.Id];
                        element.PositionNormals.Add(clone.CloneAddOrExisting(positionNormal));
                        element.Trace = triangle.Trace;
                    }
                }
            }

            foreach (var element in mapping.Values.Where(v => v.PositionNormals.Count == 3))
            {
                new PositionTriangle(element.PositionNormals[0], element.PositionNormals[1], element.PositionNormals[2], element.Trace);
            }

            return clone;
        }

        public IEnumerable<IWireFrameMesh> Clones(int number)
        {
            var clones = new IWireFrameMesh[number];

            for (int i = 0; i < number; i++)
            {
                clones[i] = Clone();
            }

            return clones;
        }

        public void Apply(ITransform transform)
        {
            foreach (var position in Positions)
            {
                var oldPoint = position.Point;
                position.Point = transform.Apply(position.Point);
                foreach (var positionNormal in position.PositionNormals)
                {
                    positionNormal.Normal = transform.Apply(oldPoint, positionNormal.Normal);
                }
            }
            _bucket = new BoxBucket<Position>(Positions);
        }

        //public void Transformation(Func<Point3D, Point3D> pointTransform, Func<Vector3D, Vector3D> normalTransform)
        //{
        //    foreach (var position in Positions)
        //    {
        //        position.Point = pointTransform(position.Point);
        //        foreach (var positionNormal in position.PositionNormals)
        //        {
        //            positionNormal.Normal = normalTransform(positionNormal.Normal);
        //        }

        //        //var point = position.Point;
        //        //var pointT = pointTransform(position.Point);
        //        //position.Point = pointT;
        //        //foreach (var positionNormal in position.PositionNormals)
        //        //{
        //        //    var pointOffsetT = transform(position.Point + positionNormal.Normal);
        //        //    positionNormal.Normal = (pointOffsetT - pointT).Direction;
        //        //}
        //        //position.Point = pointT;
        //    }
        //    //foreach (var positionNormal in Positions.SelectMany(p => p.PositionNormals))
        //    //{
        //    //    var triangles = positionNormal.Triangles;
        //    //    var angles = triangles.Select(t => t.Triangle.AngleAtPoint(positionNormal.Position)).ToArray();
        //    //    double totalAngle = angles.Sum();
        //    //    var weights = angles.Select(a => a / totalAngle).ToArray();
        //    //    var normals = triangles.Select(t => t.Triangle.Normal).ToArray();
        //    //    Vector3D weightedSum = Vector3D.Zero;
        //    //    for (int i = 0; i < weights.Length; i++)
        //    //    {
        //    //        weightedSum += weights[i] * normals[i];
        //    //    }
        //    //    //var sum = Vector3D.Average(normals);
        //    //    positionNormal.Normal = weightedSum.Direction;
        //    //}
        //    _bucket = new BoxBucket<Position>(Positions);
        //}

        public IWireFrameMesh CreateNewInstance()
        {
            return new WireFrameMesh();
        }

        public static IWireFrameMesh CreateMesh()
        {
            return new WireFrameMesh();
        }

    }
}
