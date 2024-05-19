
namespace Operations.PositionRemovals.Interfaces
{
    public interface IFillConditionals<T>
    {
        public bool AllowFill(T a, T b, T c);
    }
}
