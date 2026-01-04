
namespace BasicObjects.Math
{
    public struct λ
    {
        public λ(double λ1, double λ2, double λ3)
        {
            this.λ1 = λ1;
            this.λ2 = λ2;
            this.λ3 = λ3;
        }

        public double λ1 { get; }
        public double λ2 { get; }
        public double λ3 { get; }

        public override string ToString()
        {
            return $"λ: [{λ1}, {λ2}, {λ3}]";
        }

        public bool IsInUnitInterval()
        {
            return Double.IsInInterval(0, λ1, 1) && Double.IsInInterval(0, λ2, 1) && Double.IsInInterval(0, λ3, 1);
        }

        public bool IsOnUnitInterval()
        {
            return Double.IsOnInterval(0, λ1, 1) && Double.IsOnInterval(0, λ2, 1) && Double.IsOnInterval(0, λ3, 1);
        }

        public bool IsOnUnitInterval(double zone)
        {
            return Double.IsOnInterval(0, λ1, 1, zone) && Double.IsOnInterval(0, λ2, 1, zone) && Double.IsOnInterval(0, λ3, 1, zone);
        }
    }
}
