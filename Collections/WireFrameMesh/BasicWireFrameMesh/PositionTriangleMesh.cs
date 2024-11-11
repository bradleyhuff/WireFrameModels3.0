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

        public IEnumerable<PositionTriangle> EndRow()
        {
            PositionTriangle[] output = Array.Empty<PositionTriangle>();
            if (_rowIndex > 0)
            {
                output = BuildRow(_rowA, _rowB).ToArray();
                _rowA = _rowB;
                _rowB = new List<PositionNormal>();
            }
            _rowIndex++;
            return output;
        }

        public virtual IEnumerable<PositionTriangle> EndGrid()
        {
            var output = EndRow();
            _rowIndex = 0;
            _rowA = new List<PositionNormal>();
            _rowB = new List<PositionNormal>();
            return output;
        }

        private IEnumerable<PositionTriangle> BuildRow(IEnumerable<PositionNormal> rowA, IEnumerable<PositionNormal> rowB)
        {
            if (!rowA.Any() || !rowB.Any()) { return Enumerable.Empty<PositionTriangle>(); }

            return BuildRow(rowA.ToArray(), rowB.ToArray());
        }

        private static IEnumerable<PositionTriangle> BuildRow(PositionNormal[] rowA, PositionNormal[] rowB)
        {
            if (rowB.Length > rowA.Length) { foreach (var t in BuildRow(rowB, rowA)) { yield return t; }; }

            int iA = 0;
            int iB = 0;

            do
            {
                int fork = (int)Math.Round(((iA + 1) * rowA.Length)/(double)rowB.Length) - (int)Math.Round(iA * rowA.Length / (double)rowB.Length);
                foreach (var t in ForkB(fork, rowA, rowB, ref iA, ref iB)) { yield return t; };
                foreach (var t in Split(rowA, rowB, ref iA, ref iB)){ yield return t; };

            } while (iA < rowA.Length - 1 && iB < rowB.Length - 1);
        }

        private static IEnumerable<PositionTriangle> Split(PositionNormal[] rowA, PositionNormal[] rowB, ref int iA, ref int iB)
        {
            int iAplus = (iA + 1) % rowA.Length;
            int iBplus = (iB + 1) % rowB.Length;
            var output = new PositionTriangle[2];
            output[0] =  new PositionTriangle(rowA[iA], rowB[iB], rowB[iBplus]);
            output[1] = new PositionTriangle(rowA[iA], rowA[iAplus], rowB[iBplus]);
            iA = iAplus;
            iB = iBplus;
            return output;
        }

        private static IEnumerable<PositionTriangle> ForkB(int forkB, PositionNormal[] rowA, PositionNormal[] rowB, ref int iA, ref int iB)
        {
            var output = new PositionTriangle[forkB - 1];
            for (int i = 0; i < forkB - 1; i++)
            {
                int iAplus = (iA + 1) % rowA.Length;
                output[i] = new PositionTriangle(rowA[iA], rowA[iAplus], rowB[iB]);
                iA = iAplus;
            }
            return output;
        }
    }
}
