using Collections.WireFrameMesh.Basics;
using Collections.WireFrameMesh.Interfaces;
using BasicObjects.GeometricObjects;
using Collections.Buckets;
using BaseObjects.Transformations.Interfaces;
using BasicObjects.MathExtensions;

namespace Collections.WireFrameMesh.BasicWireFrameMesh
{
    public class WireFrameMesh : PositionTriangleMesh, IWireFrameMesh, IWireFrameMeshInternal
    {
        private static int _id = 0;
        internal WireFrameMesh() { Id = _id++; }

        private bool _triangleWasRemoved = false;
        private bool _positionWasRemoved = false;
        private DisabledPositions _positions = new DisabledPositions(new List<Position>());
        private DisabledPositionTriangles _triangles = new DisabledPositionTriangles(new List<PositionTriangle>());
        private Combination3Dictionary<PositionTriangle> _keys = new Combination3Dictionary<PositionTriangle>();
        private BoxBucket<Position> _bucket = new BoxBucket<Position>(Enumerable.Empty<Position>());

        public int Id { get; }

        public IReadOnlyList<Position> Positions
        {
            get
            {
                return _positions.Get(ref _positionWasRemoved);
            }
        }

        public IReadOnlyList<PositionTriangle> Triangles { 
            get 
            {
                return _triangles.Get(ref _triangleWasRemoved);
            } 
        }

        public bool AddNewTriangle(PositionTriangle triangle)
        {
            if (_keys.ContainsKey(triangle.Key)) { return false; }
            _triangles.Get(ref _triangleWasRemoved).Add(triangle);
            _keys[triangle.Key] = triangle;
            return true;
        }

        public PositionNormal AddPoint(Point3D position)
        {
            return AddPoint(position, Vector3D.Zero);
        }
        public PositionNormal AddPoint(Point3D position, Vector3D normal)
        {
            var positionNormal = AddPointNoRow(position, normal);
            AddPoint(positionNormal);
            return positionNormal;
        }

        public PositionNormal AddPoint(Point3D position, Vector3D normal, ITransform transform)
        {
            return AddPoint(transform.Apply(position), transform.Apply(normal));
        }

        public PositionTriangle AddTriangle(Point3D a, Point3D b, Point3D c, string trace = "")
        {
            return AddTriangle(a, Vector3D.Zero, b, Vector3D.Zero, c, Vector3D.Zero, trace);
        }

        public PositionTriangle AddTriangle(Triangle3D triangle, string trace = "")
        {
            return AddTriangle(triangle.A, triangle.B, triangle.C, trace);
        }

        public IEnumerable<PositionTriangle> AddRangeTriangles(IEnumerable<Triangle3D> triangles, string trace = "")
        {
            return AddRangeTrianglesIterate(triangles, trace).ToArray();
        }

        private IEnumerable<PositionTriangle> AddRangeTrianglesIterate(IEnumerable<Triangle3D> triangles, string trace = "")
        {
            foreach (var triangle in triangles)
            {
                yield return AddTriangle(triangle, trace);
            }
        }

        public PositionTriangle AddTriangle(Point3D a, Vector3D aN, Point3D b, Vector3D bN, Point3D c, Vector3D cN, string trace = "")
        {
            var aa = AddPointNoRow(a, aN);
            var bb = AddPointNoRow(b, bN);
            var cc = AddPointNoRow(c, cN);
            return new PositionTriangle(aa, bb, cc, trace);
        }

        public PositionTriangle AddTriangle(PositionNormal a, PositionNormal b, PositionNormal c, string trace = "")
        {
            return new PositionTriangle(a, b, c, trace);
        }

        public bool RemoveTriangle(Point3D a, Point3D b, Point3D c)
        {
            var aa = _bucket.Fetch(new Rectangle3D(a, BoxBucket.MARGINS)).SingleOrDefault(p => !p.Disabled && p.Point == a);
            var bb = _bucket.Fetch(new Rectangle3D(b, BoxBucket.MARGINS)).SingleOrDefault(p => !p.Disabled && p.Point == b);
            var cc = _bucket.Fetch(new Rectangle3D(c, BoxBucket.MARGINS)).SingleOrDefault(p => !p.Disabled && p.Point == c);

            if (aa is null || bb is null || cc is null) { return false; }
            var key = new Combination3(aa.Id, bb.Id, cc.Id);
            if (!_keys.ContainsKey(key)) { return false; }
            var removalTriangle = _keys[key];
            RemoveTriangle(removalTriangle);
            return true;
        }

        public bool RemoveTriangle(PositionNormal a, PositionNormal b, PositionNormal c)
        {
            var aa = a.PositionObject;
            var bb = b.PositionObject;
            var cc = c.PositionObject;
            var key = new Combination3(aa.Id, bb.Id, cc.Id);
            if (!_keys.ContainsKey(key)) { return false; }
            var removalTriangle = _keys[key];
            RemoveTriangle(removalTriangle);
            return true;
        }

        public bool RemoveTriangle(PositionTriangle removalTriangle)
        {
            var result = _keys.Remove(removalTriangle.Key);
            if (!result) { 
                return false; 
            }
            var aa = removalTriangle.A.PositionObject;
            var bb = removalTriangle.B.PositionObject;
            var cc = removalTriangle.C.PositionObject;

            removalTriangle.DelinkPositionNormals();
            _triangleWasRemoved = true;
            if (!removalTriangle.A._triangles.Any()) { removalTriangle.A.DelinkPosition(); }
            if (!removalTriangle.B._triangles.Any()) { removalTriangle.B.DelinkPosition(); }
            if (!removalTriangle.C._triangles.Any()) { removalTriangle.C.DelinkPosition(); }

            if (!aa.PositionNormals.Any())
            {
                aa.Disabled = true;
                _positionWasRemoved = true;
            }
            if (!bb.PositionNormals.Any())
            {
                bb.Disabled = true;
                _positionWasRemoved = true;
            }
            if (!cc.PositionNormals.Any())
            {
                cc.Disabled = true;
                _positionWasRemoved = true;
            }
            return true;
        }

        public int RemoveAllTriangles(IEnumerable<PositionTriangle> removalTriangles)
        {
            int count = 0;
            foreach(var triangle in removalTriangles.ToArray())
            {
                count += RemoveTriangle(triangle)? 1: 0;
            }
            return count;
        }

        private PositionNormal AddPointNoRow(Point3D position, Vector3D normal)
        {
            var positionObject = _bucket.Fetch(new Rectangle3D(position, BoxBucket.MARGINS)).SingleOrDefault(p => p.Point == position);
            if (positionObject is null)
            {
                var positionNormal = new PositionNormal(position, normal, this);
                positionObject = new Position(position);
                positionNormal.LinkPosition(positionObject);
                _bucket.Add(positionObject);
                _positions.Get(ref _positionWasRemoved).Add(positionObject);
                return positionNormal;
            }

            if (positionObject.Disabled)
            {
                _positions.Get(ref _positionWasRemoved).Add(positionObject);
                positionObject.Disabled = false;
            }

            var existingPositionNormal = positionObject.PositionNormals.SingleOrDefault(pn => Vector3D.DirectionsEqual(pn.Normal, normal));
            if (existingPositionNormal is not null) { return existingPositionNormal; }
            {
                var positionNormal = new PositionNormal(position, normal, this);
                positionNormal.LinkPosition(positionObject);
                return positionNormal;
            }
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
                _positions.Get(ref _positionWasRemoved).Add(positionObject);
                return clone;
            }

            if (positionObject.Disabled)
            {               
                _positions.Get(ref _positionWasRemoved).Add(positionObject);
                positionObject.Disabled = false;
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
                position.Point = transform.Apply(position.Point);
                foreach (var positionNormal in position.PositionNormals)
                {
                    positionNormal.Normal = transform.Apply(positionNormal.Normal);
                }
            }
            _bucket = new BoxBucket<Position>(Positions);
        }

        public IWireFrameMesh Clone(ITransform transform)
        {
            var clone = Clone();
            clone.Apply(transform);
            return clone;
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

        public static IWireFrameMesh Create()
        {
            return new WireFrameMesh();
        }

    }
}
