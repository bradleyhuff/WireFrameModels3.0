
namespace Collections.Threading
{
    public class Iterator<T>
    {
        private static int _threads = Math.Min(4, Environment.ProcessorCount);

        private T[] _list;

        public Iterator(T[] list)
        {
            _list = list;
        }

        public void Run<S, TS>(Action<T, TS, S> action, S state) where S : BaseState<TS> where TS : BaseThreadState, new()
        {
            Run(_list, action, state);
        }

        public void RunSingle<S, TS>(Action<T, TS, S> action, S state) where S : BaseState<TS> where TS : BaseThreadState, new()
        {
            RunSingle(_list, action, state);
        }

        private int GetStartIndex(int groupSize)
        {
            lock (_lockObject)
            {
                var currentStartIndex = _startIndex;
                _startIndex += groupSize;
                return currentStartIndex;
            }
        }

        private void ResetStartIndex()
        {
            _startIndex = 0;
        }

        private object _lockObject = new object();
        private int _startIndex = 0;

        public void Run<R, S, TS>(R[] list, Action<R, TS, S> action, S state) where S : BaseState<TS> where TS : BaseThreadState, new()
        {
            var threadObjects = new ThreadObject<R, S, TS>[_threads];

            for (int i = 0; i < threadObjects.Length; i++)
            {
                var threadObject = new ThreadObject<R, S, TS>();
                threadObjects[i] = threadObject;
                threadObject.ThreadState = new TS();
                threadObject.ThreadState.ThreadId = i;
                threadObject.Iterations = list;
                threadObject.State = state;
                threadObject.Action = action;
                threadObject.GetStartIndex = GetStartIndex;
            }
            state.Threads = threadObjects.Length;

            ResetStartIndex();

            foreach (var threadObject in threadObjects)
            {
                threadObject.Task = new Task(threadObject.Run);
                threadObject.Task.Start();
            }
            Task.WaitAll(threadObjects.Select(t => t.Task).ToArray());
            state.Finish(threadObjects.Select(t => t.ThreadState));
        }

        public void RunSingle<R, S, TS>(R[] list, Action<R, TS, S> action, S state) where S : BaseState<TS> where TS : BaseThreadState, new()
        {
            var threadObject = new ThreadObject<R, S, TS>();
            threadObject.ThreadState = new TS();
            threadObject.ThreadState.ThreadId = 0;
            threadObject.Iterations = list;
            threadObject.State = state;
            threadObject.Action = action;
            threadObject.GetStartIndex = GetStartIndex;

            state.Threads = 1;

            ResetStartIndex();
            threadObject.Run();

            state.Finish(new[] { threadObject.ThreadState });
        }
    }


    public class BaseState<TS>
    {
        public int Threads { get; set; }
        public virtual void Finish(IEnumerable<TS> threadStates) { }
    }

    public class BaseThreadState
    {
        public int ThreadId { get; set; }
    }

    public class ThreadObject<R, S, TS> where TS : BaseThreadState
    {
        public R[] Iterations { get; set; }
        public TS ThreadState { get; set; }
        public S State { get; set; }
        public Action<R, TS, S> Action { get; set; }
        public Func<int, int> GetStartIndex { get; set; }
        public Task Task { get; set; }

        public void Run()
        {
            Run(256, Action);
        }

        private void Run(int groupSize, Action<R, TS, S> action)
        {
            while (true)
            {
                int start = GetStartIndex(groupSize);
                if (start >= Iterations.Length) { break; }

                for (int i = start; i < Math.Min(start + groupSize, Iterations.Length); i++)
                {
                    var boxNode = Iterations[i];
                    action(boxNode, ThreadState, State);
                }
            }
        }
    }
}
