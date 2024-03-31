using Operations.Intermesh.Elastics;
using Operations.SurfaceSegmentChaining.Basics;

namespace Operations.SurfaceSegmentChaining.Chaining.Diagnostics
{
    internal class ChainingException<T>: InvalidOperationException
    {
        public ChainingException(string message, List<LoggingElement> loggingElements, IReadOnlyList<SurfaceRayContainer<T>> referenceArray):base(message) {
            ReferenceArray = referenceArray;
            Logs = loggingElements.Select(l => new LoggedElement(l)).ToList();
        }

        public IReadOnlyList<LoggedElement> Logs { get; }
        public IReadOnlyList<SurfaceRayContainer<T>> ReferenceArray { get; }
    }

    internal class LoggingElement
    {
        public string Note { get; set; }
        public int Start { get; set; }
        public List<int> Chaining { get; set; } = new List<int>();
    }

    internal class LoggedElement
    {
        public LoggedElement(LoggingElement element)
        {
            Note = element.Note;
            Start = element.Start;
            Chaining = element.Chaining;
        }
        public string Note { get; }
        public int Start { get; }
        public IReadOnlyList<int> Chaining { get; }
    }

}
