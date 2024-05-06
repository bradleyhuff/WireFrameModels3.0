using Collections.WireFrameMesh.Basics;
using BasicObjects.GeometricObjects;

namespace Collections.WireFrameMesh.BasicWireFrameMesh
{
    public abstract class PositionTriangleMesh
    {
        private int _rowIndex = 0;
        private List<PositionNormal> _rowA = new List<PositionNormal>();
        private List<PositionNormal> _rowB = new List<PositionNormal>();


        protected class DisabledPositions
        {
            List<Position> _list;

            public DisabledPositions(List<Position> list)
            {
                _list = list;
            }

            public List<Position> Get(ref bool wasChanged)
            {
                if (wasChanged) { _list = _list.Where(e => !e.Disabled).ToList(); wasChanged = false; }
                return _list;
            }
        }

        protected class DisabledPositionTriangles
        {
            List<PositionTriangle> _list;

            public DisabledPositionTriangles(List<PositionTriangle> list)
            {
                _list = list;
            }

            public List<PositionTriangle> Get(ref bool wasChanged)
            {
                if (wasChanged) { _list = _list.Where(e => !e.Disabled).ToList(); wasChanged = false; }
                return _list;
            }
        }

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

            //BuildRowOLD(rowA, rowB);

            BuildRow(rowA.ToArray(), rowB.ToArray());
        }

        private static void BuildRowOLD(IEnumerable<PositionNormal> rowA, IEnumerable<PositionNormal> rowB)
        {
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

                if (distanceA < distanceB)//
                {
                    new PositionTriangle(pointA, pointB, queueA.Peek());
                    pointA = queueA.Dequeue();
                }
                else
                {
                    new PositionTriangle(pointA, pointB, queueB.Peek());
                    pointB = queueB.Dequeue();
                }
            }
        }

        private static void BuildRow(PositionNormal[] rowA, PositionNormal[] rowB)
        {
            if (rowB.Length > rowA.Length) { BuildRow(rowB, rowA); }

            int iA = 0;
            int iB = 0;

            do
            {
                int fork = (int)Math.Round(((iA + 1) * rowA.Length)/(double)rowB.Length) - (int)Math.Round(iA * rowA.Length / (double)rowB.Length);
                ForkB(fork, rowA, rowB, ref iA, ref iB);
                Split(rowA, rowB, ref iA, ref iB);

            } while (iA < rowA.Length - 1 && iB < rowB.Length - 1);
        }

        private static void Split(PositionNormal[] rowA, PositionNormal[] rowB, ref int iA, ref int iB)
        {
            int iAplus = (iA + 1) % rowA.Length;
            int iBplus = (iB + 1) % rowB.Length;
            new PositionTriangle(rowA[iA], rowB[iB], rowB[iBplus]);
            new PositionTriangle(rowA[iA], rowA[iAplus], rowB[iBplus]);
            iA = iAplus;
            iB = iBplus;
        }

        private static void ForkB(int forkB, PositionNormal[] rowA, PositionNormal[] rowB, ref int iA, ref int iB)
        {
            for (int i = 0; i < forkB - 1; i++)
            {
                int iAplus = (iA + 1) % rowA.Length;
                new PositionTriangle(rowA[iA], rowA[iAplus], rowB[iB]);
                iA = iAplus;
            }
        }
    }
}
