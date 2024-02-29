using Collections.WireFrameBuilder.Basics;
using BasicObjects.GeometricObjects;

namespace Collections.WireFrameBuilder.BasicWireFrameBuilder
{
    internal abstract class PositionTriangleBuilder
    {
        private int _rowIndex = 0;
        private List<PositionNormal> _rowA = new List<PositionNormal>();
        private List<PositionNormal> _rowB = new List<PositionNormal>();
        private List<PositionTriangle> _triangles = new List<PositionTriangle>();

        protected void AddPoint(PositionNormal element, int number)
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
            for (int i = 0; i < number; i++)
            {
                row.Add(element);
            }
        }

        public void AddTriangle(PositionTriangle triangle)
        {
            _triangles.Add(triangle);
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
                    distanceA = Point3D.Distance(pointB.Position.Point, queueA.Peek().Position.Point);
                }
                if (queueB.Any() && pointA is not null)
                {
                    distanceB = Point3D.Distance(pointA.Position.Point, queueB.Peek().Position.Point);
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
