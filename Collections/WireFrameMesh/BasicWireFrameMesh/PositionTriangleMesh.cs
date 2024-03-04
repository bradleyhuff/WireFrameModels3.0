using Collections.WireFrameMesh.Basics;
using BasicObjects.GeometricObjects;
using Collections.WireFrameMesh.Interfaces;

namespace Collections.WireFrameMesh.BasicWireFrameMesh
{
    internal abstract class PositionTriangleMesh
    {
        private int _rowIndex = 0;
        private List<PositionNormal> _rowA = new List<PositionNormal>();
        private List<PositionNormal> _rowB = new List<PositionNormal>();
        private List<PositionTriangle> _triangles = new List<PositionTriangle>();

        public IReadOnlyList<PositionTriangle> Triangles { get { return _triangles; } }

        protected void AddPoint(PositionNormal element)
        {
            List<PositionNormal> row;
            if (_rowIndex == 0)
            {
                row = _rowA;
            }
            else
            {
                row = _rowB;
            }

            row.Add(element);
        }

        public void AddTriangle(PositionTriangle triangle)
        {
            _triangles.Add(triangle);
        }

        public int RemoveTriangles(IEnumerable<Triangle3D> triangles)
        {
            throw new NotImplementedException();
        }

        public void AddTriangleRange(IEnumerable<PositionTriangle> triangles)
        {
            _triangles.AddRange(triangles);
        }

        public void EndRow()
        {
            if (_rowIndex > 0)
            {
                BuildRow(_rowA, _rowB);
                _rowA = _rowB;
                _rowB = new List<PositionNormal>();
            }
            _rowIndex++;
        }

        public virtual void EndGrid()
        {
            EndRow();
            _rowIndex = 0;
            _rowA = new List<PositionNormal>();
            _rowB = new List<PositionNormal>();
        }

        private void BuildRow(IEnumerable<PositionNormal> rowA, IEnumerable<PositionNormal> rowB)
        {
            if (!rowA.Any() || !rowB.Any()) { return; }

            var queueA = new Queue<PositionNormal>(rowA);
            var queueB = new Queue<PositionNormal>(rowB);

            var pointA = queueA.Dequeue();
            var pointB = queueB.Dequeue();

            while (queueA.Any() || queueB.Any())
            {
                var distanceA = double.MaxValue;
                var distanceB = double.MaxValue;

                if (queueA.Any() && pointB is not null)
                {
                    distanceA = Point3D.Distance(pointB.Position, queueA.Peek().Position);
                }
                if (queueB.Any() && pointA is not null)
                {
                    distanceB = Point3D.Distance(pointA.Position, queueB.Peek().Position);
                }

                if (distanceA < distanceB)
                {
                    AddTriangle(new PositionTriangle(pointA, pointB, queueA.Peek()));
                    pointA = queueA.Dequeue();
                }
                else
                {
                    AddTriangle(new PositionTriangle(pointA, pointB, queueB.Peek()));
                    pointB = queueB.Dequeue();
                }
            }
        }
    }
}
